using System.Text.Json.Serialization;

namespace PopupBlocker.Core.Models
{
    public enum RuleType
    {
        Process,
        WindowClass,
        WindowTitle
    }

    public class InterceptorRule
    {
        #region 属性
        /// <summary>
        /// 规则类型
        /// </summary>
        [JsonPropertyName("type")]
        public RuleType Type { get; set; }
        /// <summary>
        /// 匹配模式
        /// </summary>
        [JsonPropertyName("pattern")]
        public string Pattern { get; set; } = string.Empty;
        /// <summary>
        /// 是否启用
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;
        /// <summary>
        /// 描述信息
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        /// <summary>
        /// 拦截次数
        /// </summary>
        [JsonPropertyName("blockedCount")]
        public long BlockedCount { get; set; } = 0;
        /// <summary>
        /// 最后一次拦截时间
        /// </summary>
        [JsonPropertyName("lastBlockedTime")]
        public DateTime? LastBlockedTime { get; set; }
        #endregion

        public InterceptorRule() { }

        public InterceptorRule(RuleType type, string pattern, bool enabled = true, string? description = null)
        {
            Type = type;
            Pattern = pattern;
            Enabled = enabled;
            Description = description;
        }

        #region 方法
        /// <summary>
        /// 更新规则信息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pattern"></param>
        /// <param name="enabled"></param>
        /// <param name="description"></param>
        public void Update(RuleType type, string pattern, bool enabled = true, string? description = null)
        {
            UpdateType(type);
            UpdatePattern(pattern);
            UpdateEnabled(enabled);
            UpdateDescription(description);
        }
        /// <summary>
        /// 更新规则类型
        /// </summary>
        /// <param name="type"></param>
        public void UpdateType(RuleType type) => Type = type;
        /// <summary>
        /// 更新匹配模式
        /// </summary>
        /// <param name="pattern"></param>
        public void UpdatePattern(string pattern) => Pattern = pattern;
        /// <summary>
        /// 更新是否启用
        /// </summary>
        /// <param name="enabled"></param>
        public void UpdateEnabled(bool enabled) => Enabled = enabled;
        /// <summary>
        /// 更新描述信息
        /// </summary>
        /// <param name="description"></param>
        public void UpdateDescription(string? description) => Description = description;
        /// <summary>
        /// 复制规则信息
        /// </summary>
        /// <param name="rule"></param>
        public void Copy(InterceptorRule rule)
        {
            Update(rule.Type, rule.Pattern, rule.Enabled, rule.Description);
            BlockedCount = rule.BlockedCount;
            LastBlockedTime = rule.LastBlockedTime;
        }
        /// <summary>
        /// 切换启用状态
        /// </summary>
        public void ToggleEnabled() => Enabled = !Enabled;
        /// <summary>
        /// 重置拦截次数
        /// </summary>
        public void ResetBlockCount()
        {
            BlockedCount = 0;
            LastBlockedTime = null;
        }
        /// <summary>
        /// 增加拦截次数
        /// </summary>
        public void IncrementBlockCount()
        {
            ++BlockedCount;
            LastBlockedTime = DateTime.Now;
        }
        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"[{(Enabled ? "✓" : "✗")}] {Type}: {Pattern}";
        #endregion
    }
}
