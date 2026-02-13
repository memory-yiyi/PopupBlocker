using PopupBlocker.Core;
using PopupBlocker.Core.Services;
using PopupBlocker.Utility.Commons;

namespace PopupBlocker.ViewModels
{
    public class SettingViewModel : ViewModelBase
    {
        /// <summary>
        /// 是否启用日志记录
        /// </summary>
        public bool IsEnableLog     // 放最上面，日志优先级应该最高
        {
            get => _logger.IsActive;
            set
            {
                _logger.IsActive = value;
                NotifyPropertyChanged();
            }
        }
        /// <summary>
        /// 是否启用拦截器
        /// </summary>
        public bool IsEnableInterceptor     // 引用不负责释放
        {
            get => Singleton<PopupInterceptorViewModel>.Instance.IsEnableInterceptor;
            set
            {
                Singleton<PopupInterceptorViewModel>.Instance.IsEnableInterceptor = value;
                NotifyPropertyChanged();
            }
        }
        /// <summary>
        /// 是否开机自启
        /// </summary>
        public bool IsAutoRun
        {
            get
            {
                return AutoRunService.GetTask(out _, out _);
            }
            set
            {
                if (value)
                {
                    try
                    {
                        AutoRunService.RegisterTask();
                    }
                    catch (UnauthorizedAccessException)
                    {
                        _logger.Error("设置开机自启失败，请以管理员身份运行程序！");
                    }
                }
                else
                    AutoRunService.UnregisterTask(out _, out _);
                NotifyPropertyChanged();
            }
        }

        #region 加载和保存设置
        private readonly LoggerService _logger = Singleton<LoggerService>.Instance;

        /* 设置耦合度太高了，它只是从其它地方读取和修改数据，所以可以这么加载设置
         * 不过也没有必要做那么复杂就是了，你就当这是反面教材，这么写其实很糟糕
         * 就目前来看这样就好了，以后有需要的话再重构，不要过度设计
         * （真的好颠
        */
        public SettingViewModel()
        {
            if (System.IO.File.Exists(AppPath.SettingFilePath))
            {
                var isEnableInterceptor = System.IO.File.ReadAllText(AppPath.SettingFilePath);
                if (isEnableInterceptor[0] == '1')
                    IsEnableInterceptor = true;
                else
                    IsEnableInterceptor = false;
            }
        }
        #endregion
    }
}
