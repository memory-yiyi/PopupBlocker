using PopupBlocker.Utility.Commons;
using PopupBlocker.Utility.Interfaces;

namespace PopupBlocker.Core.Services
{
    public class PopupBlockService : ETWThreadMonitor, IService
    {
        private static ServiceManager ServiceManager => Singleton<ServiceManager>.Instance;

        #region IService
        public bool Switch
        {
            get => Status != Status.Init && Status != Status.Stop;
            set
            {
                if (value)
                    Start();
                else
                    Stop();
            }
        }
        #endregion

        #region 服务启动与停止逻辑
        protected override void OnStart()
        {
            base.OnStart();
            ServiceManager.CurrentLoggerService.Info("拦截器已启动");
        }

        protected override void OnStop()
        {
            base.OnStop();
            ServiceManager.CurrentLoggerService.Info("拦截器已停止");
        }
        #endregion

        #region 监控逻辑
        private readonly Dictionary<Models.BlockRules, WindowCloseService> _checkList = [];

        protected override void OnThreadCreated(Microsoft.Diagnostics.Tracing.Parsers.Kernel.ThreadTraceData data)
        {
            var rules = ServiceManager.CurrentRuleConfigService.FindRules(data.ProcessName);
            if (rules is not null)
            {
                if (!_checkList.ContainsKey(rules))
                    _checkList.Add(rules, new WindowCloseService(rules));
                _checkList[rules].CheckAndBlockWindows();
            }
        }
        #endregion
    }
}
