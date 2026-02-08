using PopupBlocker.Utility.Commons;
using PopupBlocker.ViewModels;

namespace PopupBlocker.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            // 如果是图标异常，请务必按照项目文件(PopupBlocker.csproj)中的注释进行操作
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            // 对于有IDisposable的ViewModel，使用单例模式，并在窗口关闭时释放资源
            Singleton<PopupInterceptorViewModel>.Instance.Dispose();
            base.OnClosed(e);
        }

        #region 窗口事件
        private bool _isClose = false;

        private void fwMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !_isClose;
            if (e.Cancel)
                fwMain.Hide();
        }

        private void fwMain_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            nvMain.Navigate("Home");
        }
        #endregion

        #region 系统托盘菜单事件
        private void fwMain_Show()
        {
            if (fwMain.Visibility == System.Windows.Visibility.Hidden)
            {
                fwMain.Show();
                fwMain.Activate();
            }
            else if (fwMain.WindowState == System.Windows.WindowState.Minimized)
                fwMain.WindowState = System.Windows.WindowState.Normal;
        }

        private void niMenu_Home_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            fwMain_Show();
            nvMain.Navigate("Home");
        }

        private void niMenu_Setting_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            fwMain_Show();
            nvMain.Navigate("Setting");
        }

        private void niMenu_Close_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _isClose = true;
            fwMain.Close();
        }
        #endregion
    }
}
