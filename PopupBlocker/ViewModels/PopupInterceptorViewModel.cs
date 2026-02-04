//using Microsoft.Win32;
//using PopupBlocker.Core.Models;
//using PopupBlocker.Core.Services;
//using PopupBlocker.Utility.Commons;
//using System.ComponentModel;
//using System.Runtime.CompilerServices;
//using System.Windows;
//using System.Windows.Data;
//using System.Windows.Input;
//using InterceptorRuleList = System.Collections.Generic.List<PopupBlocker.Core.Models.InterceptorRules>;

//namespace PopupBlocker.Core.ViewModels
//{
//    public class PopupInterceptorViewModel : INotifyPropertyChanged, IDisposable
//    {
//        private readonly PopupInterceptorService _popupInterceptor = Singleton<PopupInterceptorService>.Instance;
//        private readonly ConfigService _config = Singleton<ConfigService>.Instance;

//        #region 通知属性更改
//        public event PropertyChangedEventHandler? PropertyChanged;
//        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
//        {
//            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
//        }
//        #endregion

//        #region 公共属性的私有字段
//        private string _status = "状态: 已停止";
//        private string _statusInfo = "规则: 0 (启用: 0) | 总拦截: 0";
//        private ListCollectionView _rulesView;
//        #endregion

//        #region 公共属性
//        public string Status
//        {
//            get => _status;
//            set
//            {
//                if (_status != value)
//                {
//                    _status = value;
//                    NotifyPropertyChanged();
//                }
//            }
//        }

//        public string StatusInfo
//        {
//            get => _statusInfo;
//            set
//            {
//                if (_statusInfo != value)
//                {
//                    _statusInfo = value;
//                    NotifyPropertyChanged();
//                }
//            }
//        }

//        public ListCollectionView RulesView
//        {
//            get => _rulesView;
//            private set
//            {
//                if (_rulesView != value)
//                {
//                    _rulesView = value;
//                    NotifyPropertyChanged();
//                }
//            }
//        }
//        #endregion

//        public PopupInterceptorViewModel()
//        {
//            /* 初始化，_rulesView已赋值，请忽略关于其的CS8618警告
//             * 遇到奇怪的bug可以尝试注释预处理器指令
//             * 你问我为什么？因为ai会非常固执的修复这个警告，即使你确认这个警告是多余的
//             */
//            RefreshRulesList(_config.GetAllRules());
//            // 订阅事件
//            _config.RulesChanged += RefreshRulesList;
//        }

//        #region 拦截功能控制与测试命令
//        public ICommand StartInterceptCommand
//        {
//            get => new RelayCommand((obj) =>
//            {
//                _popupInterceptor.Start();
//                Status = "状态: 运行中";
//            }, (obj) => _popupInterceptor.IsStopped);
//        }

//        public ICommand StopInterceptCommand
//        {
//            get => new RelayCommand((obj) =>
//            {
//                _popupInterceptor.Stop();
//                Status = "状态: 已停止";
//            }, (obj) => _popupInterceptor.IsRunning);
//        }

//        public static ICommand TestPopupCommand
//        {
//            get => new RelayCommand((obj) =>
//            {
//                TestPopupWindow.ShowTestPopup();
//            });
//        }
//        #endregion

//        #region 规则管理命令
//        public ICommand AddRuleCommand
//        {
//            get => new RelayCommand((obj) =>
//            {
//                if (obj is Tuple<string, RuleType> input)
//                {
//                    _config.AddRule(new InterceptorRule
//                    {
//                        Pattern = input.Item1,
//                        Type = input.Item2,
//                        Enabled = true,
//                        Description = $"用户添加的规则 - {DateTime.Now:yyyy-MM-dd}"
//                    });
//                }
//            });
//        }

//        public ICommand RemoveRuleCommand
//        {
//            get => new RelayCommand((obj) =>
//            {
//                if (obj is InterceptorRule rule)
//                    _config.RemoveRule(rule);
//            });
//        }

//        public ICommand ToggleRuleCommand
//        {
//            get => new RelayCommand((obj) =>
//            {
//                // 因为数据绑定，传过来的已经切换状态了，直接保存即可
//                if (obj is InterceptorRule rule)
//                    _config.SaveRules();
//            });
//        }
//        #endregion

//        #region 规则集合管理命令
//        public ICommand ExportRulesCommand
//        {
//            get => new RelayCommand((obj) =>
//            {
//                var saveFileDialog = new SaveFileDialog
//                {
//                    Filter = "JSON配置文件 (*.json)|*.json|所有文件 (*.*)|*.*",
//                    FileName = "popup_blocker_config.json",
//                    Title = "导出配置"
//                };
//                if (saveFileDialog.ShowDialog() == true)
//                    _config.ExportRules(saveFileDialog.FileName);
//            });
//        }

//        public ICommand ImportRulesCommand
//        {
//            get => new RelayCommand((obj) =>
//            {
//                var openFileDialog = new OpenFileDialog
//                {
//                    Filter = "JSON配置文件 (*.json)|*.json|所有文件 (*.*)|*.*",
//                    Title = "导入配置"
//                };
//                if (openFileDialog.ShowDialog() == true)
//                    _config.ImportRules(openFileDialog.FileName);
//            });
//        }

//        public ICommand ResetAllCountsCommand
//        {
//            get => new RelayCommand((obj) =>
//            {
//                var result = MessageBox.Show("确定要将所有拦截计数清零吗？", "确认",
//                    MessageBoxButton.YesNo, MessageBoxImage.Question);
//                if (result == MessageBoxResult.Yes)
//                    _config.ResetAllCounts();
//            });
//        }
//        #endregion

//        #region 私有方法
//        private void RefreshRulesList(InterceptorRuleList rules)
//        {
//            RulesView = new ListCollectionView(rules);
//            RulesView.GroupDescriptions.Add(new PropertyGroupDescription("Type"));
//            StatusInfo = $"规则: {rules.Count} (启用: {rules.Count(r => r.Enabled)}) | 总拦截: {rules.Sum(r => r.BlockedCount)}";
//        }
//        #endregion

//        #region 释放模式
//        private bool disposedValue;

//        protected virtual void Dispose(bool disposing)
//        {
//            if (!disposedValue)
//            {
//                if (disposing)
//                {
//                    // TODO: 释放托管状态(托管对象)
//                    _popupInterceptor.Dispose();
//                    // 取消订阅事件
//                    _config.RulesChanged -= RefreshRulesList;
//                }

//                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
//                // TODO: 将大型字段设置为 null
//                disposedValue = true;
//            }
//        }

//        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
//        // ~PopupInterceptorViewModel()
//        // {
//        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
//        //     Dispose(disposing: false);
//        // }

//        public void Dispose()
//        {
//            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
//            Dispose(disposing: true);
//            GC.SuppressFinalize(this);
//        }
//        #endregion
//    }
//}
