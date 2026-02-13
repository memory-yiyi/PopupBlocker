using Microsoft.Win32;
using PopupBlocker.Core.Models;
using PopupBlocker.Core.Services;
using PopupBlocker.Utility.Commons;
using System.Windows.Input;
using RemoveRuleParameters = System.Tuple<object, object?>;

namespace PopupBlocker.ViewModels
{
    public class PopupInterceptorViewModel : ViewModelBase, IDisposable
    {
        private readonly PopupInterceptorService _popupBlocker = Singleton<PopupInterceptorService>.Instance;
        private readonly RuleConfigService _ruleConfig = Singleton<RuleConfigService>.Instance;
        public PopupInterceptorViewModel()
        {
            _ruleConfig.RulesChanged += RuleChangedEvent;
            RuleChangedEvent(_ruleConfig.GetAllRules());
        }

        #region 属性
        public long BlockedCount => _ruleConfig.BlockedCount;
        public IEnumerable<InterceptorRules> RuleList { get; private set; }

        public bool IsEnableInterceptor
        {
            get => _popupBlocker.Status != Utility.Interfaces.Status.Init;
            set
            {
                if (value)
                {
                    try
                    {
                        _popupBlocker.Start();
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Singleton<LoggerService>.Instance.Error("拦截服务启动失败，请以管理员身份运行程序！");
                    }
                }
                else
                    _popupBlocker.Stop();
                System.IO.File.WriteAllText(Core.AppPath.SettingFilePath, $"{(value ? '1' : '0')}");    // 太臭了，但只有这一个设置需要保存，，，
                NotifyPropertyChanged();
            }
        }

        public ICommand ResetAllCountCommand => new RelayCommand(obj =>
        {
            _ruleConfig.ResetAllCounts();
        });

        public ICommand ImportRulesCommand => new RelayCommand(obj =>
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON配置文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                Title = "导入配置"
            };
            if (openFileDialog.ShowDialog() == true)
                _ruleConfig.LoadRuleList(openFileDialog.FileName);
        });

        public ICommand ExportRulesCommand => new RelayCommand(obj =>
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON配置文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                FileName = Core.AppPath.RuleConfigFileName,
                Title = "导出配置"
            };
            if (saveFileDialog.ShowDialog() == true)
                _ruleConfig.SaveRuleList(saveFileDialog.FileName, false);
        });

        public ICommand WindowSelectCommand => new RelayCommand(obj =>
        {
            new Views.Selector().Show();
        });

        public ICommand SaveRuleCommand => new RelayCommand(obj =>
        {
            _ruleConfig.SaveRuleList(isNotifyUI: false);
        });

        public ICommand ResetCountCommand => new RelayCommand(obj =>
        {
            if (obj is InterceptorRules rules)
            {
                _ruleConfig.ResetRulesCount(rules);
            }
        });

        public ICommand RemoveRuleCommand => new RelayCommand(obj =>
        {
            if (obj is RemoveRuleParameters parameters)
            {
                _ruleConfig.RemoveRule((InterceptorRules)parameters.Item1, (InterceptorRule?)parameters.Item2);
            }
        });
        #endregion

        #region 方法
        private void RuleChangedEvent(IEnumerable<InterceptorRules> ruleList)
        {
            RuleList = ruleList;
            NotifyPropertyChanged(nameof(RuleList));
            NotifyPropertyChanged(nameof(BlockedCount));
        }
        #endregion

        #region IDisposable
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    _popupBlocker.Stop();
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~PopupInterceptorViewModel()
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
