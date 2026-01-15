using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using PopupBlocker.Models;

namespace PopupBlocker.Services
{
    public class ConfigService
    {
        private const string ConfigFileName = "popup_blocker_config.json";
        private readonly string _configFilePath;

        public ConfigService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "PopupBlocker");
            Directory.CreateDirectory(appFolder);
            _configFilePath = Path.Combine(appFolder, ConfigFileName);
        }

        public List<InterceptorRule> LoadRules()
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    var json = File.ReadAllText(_configFilePath);
                    return JsonSerializer.Deserialize<List<InterceptorRule>>(json) ?? new List<InterceptorRule>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载配置失败: {ex.Message}");
            }

            return new List<InterceptorRule>();
        }

        public void SaveRules(List<InterceptorRule> rules)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(rules, options);
                File.WriteAllText(_configFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存配置失败: {ex.Message}");
            }
        }

        public void ExportRules(List<InterceptorRule> rules, string filePath)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                // 创建不包含计数和时间的新规则列表
                var exportRules = rules.Select(r => new InterceptorRule
                {
                    Type = r.Type,
                    Pattern = r.Pattern,
                    Enabled = r.Enabled,
                    Description = r.Description,
                    BlockedCount = 0,  // 重置为0
                    LastBlockedTime = null  // 清除时间
                }).ToList();

                var json = JsonSerializer.Serialize(exportRules, options);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"导出配置失败: {ex.Message}");
            }
        }

        public List<InterceptorRule> ImportRules(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"配置文件不存在: {filePath}");
                }

                var json = File.ReadAllText(filePath);
                var rules = JsonSerializer.Deserialize<List<InterceptorRule>>(json);
                
                if (rules == null)
                {
                    throw new Exception("配置文件中没有有效的规则");
                }

                return rules;
            }
            catch (Exception ex)
            {
                throw new Exception($"导入配置失败: {ex.Message}");
            }
        }

        public void SaveDefaultRules()
        {
            var defaultRules = new List<InterceptorRule>
            {
                new InterceptorRule(RuleType.WindowClass, "#32770", true, "标准对话框窗口"),
                new InterceptorRule(RuleType.WindowClass, "Popup", true, "弹出窗口"),
                new InterceptorRule(RuleType.WindowClass, "Tooltip", true, "工具提示窗口"),
                new InterceptorRule(RuleType.WindowTitle, "广告", true, "包含广告关键词的窗口"),
                new InterceptorRule(RuleType.WindowTitle, "推广", true, "包含推广关键词的窗口"),
                new InterceptorRule(RuleType.WindowTitle, "alert", true, "警告弹窗"),
                new InterceptorRule(RuleType.WindowTitle, "popup", true, "弹出窗口")
            };

            SaveRules(defaultRules);
        }
    }
}