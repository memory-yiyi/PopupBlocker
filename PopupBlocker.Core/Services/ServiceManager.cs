namespace PopupBlocker.Core.Services
{
    public class ServiceManager : IDisposable
    {
        private readonly Dictionary<int, IService> _services;

        public ServiceManager()
        {
            _services = [];
            CurrentLoggerServiceType = ServiceType.DefaultLoggerService;
            _services.Add(CurrentLoggerServiceType, new LoggerService());
            CurrentRuleConfigServiceType = ServiceType.DefaultRuleConfigService;
            _services.Add(CurrentRuleConfigServiceType, new RuleConfigService(logger: CurrentLoggerService));
            CurrentPopupBlockServiceType = ServiceType.DefaultPopupBlockService;
            _services.Add(CurrentPopupBlockServiceType, new PopupBlockService());
            CurrentAutoRunServiceType = ServiceType.DefaultAutoRunService;
            _services.Add(CurrentAutoRunServiceType, new AutoRunService());
            CurrentSettingServiceType = ServiceType.DefaultSettingService;
        }

        #region 当前服务设置与查询
        public int CurrentLoggerServiceType { get; set; }
        public LoggerService CurrentLoggerService { get => GetService<LoggerService>(CurrentLoggerServiceType) ?? GetService<LoggerService>(ServiceType.DefaultLoggerService)!; }

        public int CurrentRuleConfigServiceType { get; set; }
        public RuleConfigService CurrentRuleConfigService { get => GetService<RuleConfigService>(CurrentRuleConfigServiceType) ?? GetService<RuleConfigService>(ServiceType.DefaultRuleConfigService)!; }

        public int CurrentPopupBlockServiceType { get; set; }
        public PopupBlockService CurrentPopupBlockService { get => GetService<PopupBlockService>(CurrentPopupBlockServiceType) ?? GetService<PopupBlockService>(ServiceType.DefaultPopupBlockService)!; }

        public int CurrentAutoRunServiceType { get; set; }
        public AutoRunService CurrentAutoRunService { get => GetService<AutoRunService>(CurrentAutoRunServiceType) ?? GetService<AutoRunService>(ServiceType.DefaultAutoRunService)!; }

        public int CurrentSettingServiceType { get; set; }
        public T GetCurrentSettingService<T>() where T : IService => GetService<T>(CurrentSettingServiceType) ?? GetService<T>(ServiceType.DefaultSettingService)!;
        public bool InitSettingService(IService service) => RegisterService(ServiceType.DefaultSettingService, service);
        #endregion

        public T? GetService<T>(int sKey) where T : IService
        {
            if (_services.TryGetValue(sKey, out var service))
                return (T)service;
            else
                return default;
        }

        public bool RegisterService(int sKey, IService service) => _services.TryAdd(sKey, service);
        public bool UnregisterService(int sKey) => _services.Remove(sKey);

        #region IDisposable
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    foreach (var service in _services.Values)
                    {
                        var serviceType = service.GetType();
                        if (serviceType.GetInterface("IDisposable") is not null)
                            ((IDisposable)Convert.ChangeType(service, serviceType)).Dispose();
                    }
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~ServiceManager()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
