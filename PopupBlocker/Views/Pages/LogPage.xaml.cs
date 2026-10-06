namespace PopupBlocker.Views.Pages
{
    /// <summary>
    /// LogPage.xaml 的交互逻辑
    /// </summary>
    public partial class LogPage : System.Windows.Controls.Page
    {
        private static Core.Services.ServiceManager ServiceManager => Utility.Commons.Singleton<Core.Services.ServiceManager>.Instance;

        public LogPage()
        {
            InitializeComponent();
            this.DataContext = ServiceManager.GetCurrentSettingService<ViewModels.SettingViewModel>();
            // 订阅事件，并确保UI线程安全
            ServiceManager
                .CurrentLoggerService
                .LogWritingEvent += (text) => Dispatcher.Invoke(() => tbLog.AppendText(text));
        }

        private void Page_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // 取消订阅事件，防止内存泄露
            ServiceManager
                .CurrentLoggerService
                .LogWritingEvent -= (text) => Dispatcher.Invoke(() => tbLog.AppendText(text));
        }
    }
}
