using PopupBlocker.Models;
using System.Collections;
using System.IO;
using System.Text.Json;
using InterceptorRuleList = System.Collections.Generic.List<PopupBlocker.Models.InterceptorRule>;

namespace PopupBlocker.Services
{
    public class ConfigService : IEnumerable<InterceptorRule>
    {
        #region 私有字段
        // 存储拦截规则的列表
        private readonly InterceptorRuleList _rules = [];
        // 配置文件名称和路径
        private const string ConfigFileName = "popup_blocker_config.json";
        private readonly string _configFilePath;
        // JSON 序列化选项，用于美化输出和驼峰命名
        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        // 日志服务，用于记录日志信息
        private readonly LoggerService _logger = Utility.Commons.Singleton<LoggerService>.Instance;
        #endregion

        public ConfigService()
        {
            // 创建C:\Users\User\AppData\Roaming\PopupBlocker文件夹，如果不存在的话
            // 并且初始化配置文件路径
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "PopupBlocker");
            Directory.CreateDirectory(appFolder);
            _configFilePath = Path.Combine(appFolder, ConfigFileName);
            // 加载配置文件中的拦截规则
            LoadRules();
        }

        /// <summary>
        /// 规则变化通知事件，当规则发生变化时触发此事件。
        /// </summary>
        public event Action<InterceptorRuleList>? RulesChanged;

        #region 规则集合的公共方法
        public void LoadRules()
        {
            _rules.Clear();
            try
            {
                if (File.Exists(_configFilePath))
                {
                    using var jsonStream = File.OpenRead(_configFilePath);
                    var rules = JsonSerializer.Deserialize<InterceptorRuleList>(jsonStream);
                    if (rules is not null)
                        _rules.AddRange(rules);
                }
            }
            catch (Exception ex)
            {
                _logger.Warning($"加载配置失败: {ex.Message}");
                return;
            }
            _logger.Debug($"配置已成功加载");
        }

        public void SaveRules()
        {
            try
            {
                using var jsonStream = File.OpenWrite(_configFilePath);
                jsonStream.SetLength(0);
                JsonSerializer.Serialize(jsonStream, _rules, _jsonSerializerOptions);
            }
            catch (Exception ex)
            {
                _logger.Warning($"保存配置失败: {ex.Message}");
                return;
            }
            _logger.Debug($"配置已成功保存");
            RulesChanged?.Invoke(GetAllRules());
        }

        public void ExportRules(string filePath)
        {
            // 创建不包含计数和时间的新规则列表
            var exportRules = _rules.Select(r => new InterceptorRule
            {
                Type = r.Type,
                Pattern = r.Pattern,
                Enabled = r.Enabled,
                Description = r.Description,
                BlockedCount = 0,  // 重置为0
                LastBlockedTime = null  // 清除时间
            }).ToList();
            try
            {
                using var jsonStream = File.OpenWrite(filePath);
                jsonStream.SetLength(0);
                JsonSerializer.Serialize(jsonStream, exportRules, _jsonSerializerOptions);
            }
            catch (Exception ex)
            {
                _logger.Warning($"导出配置失败: {ex.Message}");
                return;
            }
            _logger.Info($"配置已成功导出到: {filePath}");
        }

        public void ImportRules(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"配置文件不存在: {filePath}");

                using var jsonStream = File.OpenRead(filePath);
                var rules = JsonSerializer.Deserialize<InterceptorRuleList>(jsonStream) ?? throw new Exception("配置文件中没有有效的规则");
                foreach (var rule in rules)
                    AddRule(rule, true);
            }
            catch (Exception ex)
            {
                _logger.Warning($"导入配置失败: {ex.Message}");
                return;
            }
            SaveRules();
            _logger.Info($"配置已成功导入: {filePath}");
        }

        public void ResetAllCounts()
        {
            foreach (var rule in _rules)
                rule.ResetBlockCount();
            SaveRules();
            _logger.Info("所有拦截规则的计数已重置");
        }

        public void SaveDefaultRules()
        {
            _rules.Clear();
            _rules.Add(new InterceptorRule(RuleType.WindowClass, "#32770", true, "标准对话框窗口"));
            _rules.Add(new InterceptorRule(RuleType.WindowClass, "Popup", true, "弹出窗口"));
            _rules.Add(new InterceptorRule(RuleType.WindowClass, "Tooltip", true, "工具提示窗口"));
            _rules.Add(new InterceptorRule(RuleType.WindowTitle, "广告", true, "包含广告关键词的窗口"));
            _rules.Add(new InterceptorRule(RuleType.WindowTitle, "推广", true, "包含推广关键词的窗口"));
            _rules.Add(new InterceptorRule(RuleType.WindowTitle, "alert", true, "警告弹窗"));
            _rules.Add(new InterceptorRule(RuleType.WindowTitle, "popup", true, "弹出窗口"));

            SaveRules();
        }

        // 这是浅拷贝，所以你对新列表的修改也会影响原始列表
        // 这是你能通过数据绑定直接影响原始列表的原因
        // 也是此处对于规则方法能如此轻松实现的原因
        // 如果使用深拷贝，你必须给每个规则方法加上搜索逻辑，以找到并更新原始列表中的规则
        public InterceptorRuleList GetAllRules() => [.. _rules];
        public IEnumerator<InterceptorRule> GetEnumerator() => _rules.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion

        #region 规则的公共方法
        public void AddRule(InterceptorRule rule, bool isImport = false)
        {
            if (_rules.Exists(r => r.Type == rule.Type && r.Pattern == rule.Pattern))
            {
                _logger.Warning($"规则已存在: {rule}");
                return;
            }
            _rules.Add(rule);
            if (isImport)
                return;
            SaveRules();
            _logger.Info($"规则已成功添加: {rule}");
        }

        public void RemoveRule(InterceptorRule rule)
        {
            _rules.Remove(rule);
            SaveRules();
            _logger.Info($"规则已成功删除: {rule}");
        }

        public void UpdateRule(InterceptorRule oldRule, InterceptorRule newRule)
        {
            oldRule.Copy(newRule);
            SaveRules();
        }

        public void UpdateRule(InterceptorRule rule, RuleType type, string pattern, bool enabled = true, string? description = null)
        {
            rule.Update(type, pattern, enabled, description);
            SaveRules();
        }

        public void UpdateRuleType(InterceptorRule rule, RuleType type)
        {
            rule.UpdateType(type);
            SaveRules();
        }

        public void UpdateRulePattern(InterceptorRule rule, string pattern)
        {
            rule.UpdatePattern(pattern);
            SaveRules();
        }

        public void UpdateRuleEnabled(InterceptorRule rule, bool enabled)
        {
            rule.UpdateEnabled(enabled);
            SaveRules();
        }

        public void UpdateRuleDescription(InterceptorRule rule, string? description)
        {
            rule.UpdateDescription(description);
            SaveRules();
        }

        public void ToggleRule(InterceptorRule rule)
        {
            rule.ToggleEnabled();
            SaveRules();
        }

        public void ResetRuleCount(InterceptorRule rule)
        {
            rule.ResetBlockCount();
            SaveRules();
        }

        public void IncrementRuleCount(InterceptorRule rule)
        {
            rule.IncrementBlockCount();
            SaveRules();
        }
        #endregion
    }
}
