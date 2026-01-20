using PopupBlocker.Core.Services;
using PopupBlocker.Core.ViewModels;
using System.Windows;

namespace PopupBlocker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly PopupInterceptorViewModel _viewModel;
        private readonly LoggerService _logger = Utility.Commons.Singleton<LoggerService>.Instance;

        public MainWindow()
        {
            InitializeComponent();

            // 配置测试的可见性
#if DEBUG
            btnTest.Visibility = Visibility.Visible;
#endif
            // 初始化ViewModel并设置DataContext
            _viewModel = new PopupInterceptorViewModel();
            DataContext = _viewModel;
            // 订阅事件，并确保UI线程安全
            _logger.LogWritingEvent += (text) => Dispatcher.Invoke(() => tbLog.AppendText(text));
        }

        protected override void OnClosed(EventArgs e)
        {
            // 清理资源
            _viewModel.Dispose();
            // 取消订阅事件
            _logger.LogWritingEvent -= (text) => Dispatcher.Invoke(() => tbLog.AppendText(text));
            // 基类调用
            base.OnClosed(e);
        }
    }
}
