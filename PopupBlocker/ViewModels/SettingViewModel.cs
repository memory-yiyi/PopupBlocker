using PopupBlocker.Core.Services;
using PopupBlocker.Utility.Commons;
using System.Text.Json.Serialization;

namespace PopupBlocker.ViewModels
{
    public class SettingViewModel : ViewModelBase
    {
        /// <summary>
        /// 是否启用日志记录
        /// </summary>
        [JsonPropertyName("isEnableLog")]
        public bool IsEnableLog     // 放最上面，日志优先级应该最高
        {
            get => Singleton<LoggerService>.Instance.IsActive;
            set
            {
                Singleton<LoggerService>.Instance.IsActive = value;
                NotifyPropertyChanged();
            }
        }
        /// <summary>
        /// 是否启用拦截器
        /// </summary>
        [JsonPropertyName("isEnableInterceptor")]
        public bool IsEnableInterceptor
        {
            get => Singleton<PopupInterceptorViewModel>.Instance.IsEnableInterceptor;
            set
            {
                Singleton<PopupInterceptorViewModel>.Instance.IsEnableInterceptor = value;
                Singleton<PopupInterceptorViewModel>.Instance.ChangeInterceptorStatusCommand.Execute(null);
                NotifyPropertyChanged();
            }
        }
    }
}
