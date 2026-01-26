using System.Text.Json.Serialization;

namespace PopupBlocker.Core.Models
{
    public sealed class InterceptorRules(string processName, List<InterceptorRule>? rules, bool isActive = true) : Utility.Interfaces.IPopupInfo
    {
        #region 属性
        /// <summary>
        /// 规则类型是否为进程名称
        /// </summary>
        [JsonIgnore]
        public bool IsProcessName => Rules is null;
        /// <summary>
        /// 进程名称
        /// </summary>
        [JsonPropertyName("processName")]
        public string ProcessName { get; init; } = processName;
        /// <summary>
        /// 拦截规则
        /// </summary>
        [JsonPropertyName("rules")]
        public List<InterceptorRule>? Rules { get; init; } = rules;
        #endregion

        #region 构造函数
        public InterceptorRules(string processName, bool isActive = true) : this(processName, null, isActive) { }
        #endregion

        #region 方法
        /// <summary>
        /// 添加规则
        /// </summary>
        /// <param name="rule"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public bool AddRule(InterceptorRule rule)
        {
            if (IsProcessName)
                throw new InvalidOperationException("按进程名称拦截时，不能添加规则");
            if (Rules!.Exists(r => r.Pattern == rule.Pattern && r.IsWindowClass == rule.IsWindowClass))
                return false;
            Rules.Add(rule);
            return true;
        }
        /// <summary>
        /// 删除规则
        /// </summary>
        /// <param name="rule"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void RemoveRule(InterceptorRule rule)
        {
            if (IsProcessName)
                throw new InvalidOperationException("按进程名称拦截时，不能删除规则");
            Rules!.Remove(rule);
        }
        /// <summary>
        /// 匹配规则
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public bool MatchRule(string pattern)
        {
            if (IsProcessName)
                throw new InvalidOperationException("按进程名称拦截时，不能匹配规则");
            /* 为什么不用Exists(r => r.Pattern == pattern && r.IsActive)？
             * 因为Find找到就会停下，而上面的在规则被禁用时，即使找到规则也会遍历所有规则
             * Find可以减少不必要的遍历，提高效率
             */
            var rule = Rules!.Find(r => r.Pattern == pattern);
            if (rule is null)
                return false;
            return rule.IsActive;
        }
        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"[{(IsActive ? "✓" : "✗")}] {ProcessName}: {(IsProcessName ? "<Null>" : "<List>")}";
        #endregion

        #region IPopupInfo
        private long _blockedCount = 0;

        /// <summary>
        /// 是否启用
        /// </summary>
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; } = isActive;
        /// <summary>
        /// 匹配字符串
        /// </summary>
        [JsonIgnore]
        public string Pattern => IsProcessName ? ProcessName : throw new InvalidOperationException("不按进程名称拦截时，不能获取匹配字符串");
        /// <summary>
        /// 拦截次数
        /// </summary>
        [JsonPropertyName("blockedCount")]
        public long BlockedCount
        {
            get
            {
                if (IsProcessName)
                    return _blockedCount;
                return Rules!.Sum(r => r.BlockedCount);
            }
            init => _blockedCount = value;
        }

        /// <summary>
        /// 重置拦截次数
        /// </summary>
        public void ResetBlockCount()
        {
            if (IsProcessName)
                _blockedCount = 0;
            else
                Rules!.ForEach(r => r.ResetBlockCount());
        }
        /// <summary>
        /// 增加拦截次数
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void AddBlockCount()
        {
            if (!IsProcessName)
                throw new InvalidOperationException("不按进程名称拦截时，不能增加拦截次数");
            ++_blockedCount;
        }
        #endregion
    }
}
