using System.Text.Json.Serialization;

namespace PopupBlocker.Models
{
    public enum RuleType
    {
        Process,
        WindowClass,
        WindowTitle
    }

    public class InterceptorRule
    {
        [JsonPropertyName("type")]
        public RuleType Type { get; set; }

        [JsonPropertyName("pattern")]
        public string Pattern { get; set; } = string.Empty;

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("blockedCount")]
        public long BlockedCount { get; set; } = 0;

        [JsonPropertyName("lastBlockedTime")]
        public DateTime? LastBlockedTime { get; set; }

        public InterceptorRule() { }

        public InterceptorRule(RuleType type, string pattern, bool enabled = true, string? description = null)
        {
            Type = type;
            Pattern = pattern;
            Enabled = enabled;
            Description = description;
        }

        public void IncrementBlockCount()
        {
            BlockedCount++;
            LastBlockedTime = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{(Enabled ? "✓" : "✗")}] {Type}: {Pattern}";
        }
    }
}