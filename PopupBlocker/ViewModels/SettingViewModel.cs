using PopupBlocker.Core;
using PopupBlocker.Core.Services;
using PopupBlocker.Utility.Commons;
using System.Text.Json.Serialization;

namespace PopupBlocker.ViewModels
{
    public class SettingViewModel : ViewModelBase, IService
    {
        private static ServiceManager ServiceManager => Singleton<ServiceManager>.Instance;

        /// <summary>
        /// 是否启用日志记录
        /// </summary>
        [JsonPropertyName("isEnableLog")]
        public bool IsEnableLog
        {
            get => ServiceManager.CurrentLoggerService.Switch;
            set
            {
                ServiceManager.CurrentLoggerService.Switch = value;
                NotifyPropertyChanged();
            }
        }
        /// <summary>
        /// 是否启用拦截器
        /// </summary>
        [JsonPropertyName("isEnableBlock")]
        public bool IsEnableBlock
        {
            get => ServiceManager.CurrentPopupBlockService.Switch;
            set
            {
                try
                {
                    ServiceManager.CurrentPopupBlockService.Switch = value;
                }
                catch (UnauthorizedAccessException)
                {
                    ServiceManager.CurrentLoggerService.Error("拦截服务启动失败，请以管理员身份运行程序！");
                }
                catch (Exception ex)
                {
                    // 启动失败不能中断整个设置的加载，否则用户其余的设置会被一起丢掉
                    ServiceManager.CurrentLoggerService.Error($"拦截服务启动失败：{ex.Message}");
                }
                NotifyPropertyChanged();
            }
        }
        /// <summary>
        /// 是否开机自启
        /// </summary>
        [JsonPropertyName("isAutoRun")]
        public bool IsAutoRun
        {
            get => ServiceManager.CurrentAutoRunService.Switch;
            set
            {
                try
                {
                    ServiceManager.CurrentAutoRunService.Switch = value;
                }
                catch (UnauthorizedAccessException)
                {
                    ServiceManager.CurrentLoggerService.Error("设置开机自启失败，请以管理员身份运行程序！");
                }
                catch (Exception ex)
                {
                    ServiceManager.CurrentLoggerService.Error($"设置开机自启失败：{ex.Message}");
                }
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 是否使用深色主题
        /// </summary>
        [JsonPropertyName("isDarkTheme")]
        public bool IsDarkTheme
        {
            get => Components.ThemeSwitcher.IsDark;
            set
            {
                try
                {
                    Components.ThemeSwitcher.Apply(value);
                }
                catch (Exception ex)
                {
                    ServiceManager.CurrentLoggerService.Warning($"切换主题失败：{ex.Message}");
                }
                NotifyPropertyChanged();
            }
        }

        #region 加载和保存设置
        private bool _isLoaded;
        public void ConfirmLoaded() => _isLoaded = true;
        public static SettingViewModel LoadSetting()
        {
            SettingViewModel setting;
            try
            {
                setting = FileOperation.ReadJsonFromFile<SettingViewModel>(AppPath.SettingFilePath);
            }
            catch (Exception ex)
            {
                // 读不出来就退回默认值，但要把原因记下来，
                // 否则用户只会看到"设置莫名其妙全没了"
                ServiceManager.CurrentLoggerService.Warning($"读取设置失败，将使用默认设置：{ex.Message}");
                setting = new SettingViewModel();
            }
            setting.ConfirmLoaded();
            return setting;
        }

        public void SaveSetting()
        {
            try
            {
                FileOperation.WriteJsonToFile(AppPath.SettingFilePath, this);
            }
            catch (Exception ex)
            {
                // 保存失败只记录，界面上已经生效的开关不受影响
                ServiceManager.CurrentLoggerService.Error($"保存设置失败：{ex.Message}");
            }
        }
        #endregion

        #region IService
        [JsonIgnore]
        public bool Switch { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        #endregion

        protected override void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            if (_isLoaded)
                SaveSetting();
            base.NotifyPropertyChanged(propertyName);
        }
    }
}
