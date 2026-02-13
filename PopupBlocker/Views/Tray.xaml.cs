using PopupBlocker.Utility.Commons;
using PopupBlocker.ViewModels;
using System.Windows;

namespace PopupBlocker.Views
{
    /// <summary>
    /// Tray.xaml 的交互逻辑
    /// </summary>
    public partial class Tray : Window
    {
        public Tray(bool isShowMainWindow = true)
        {
            // 强制加载设置，确保核心能正常运行
            // 其创建了一些IDisposable对象，记得在OnClosed中释放资源
            _ = Singleton<SettingViewModel>.Instance;
            // 创建主窗口，按需显示
            if (isShowMainWindow)
            {
                _mainWindow = new();
                _mainWindow.Show();
            }
            // 如果是图标异常，请务必按照项目文件(PopupBlocker.csproj)中的注释进行操作
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (!_mainWindow!.IsClosed)
                _mainWindow.Close();
            // 谁创建谁释放！
            // 对于有IDisposable的ViewModel，使用单例模式，并在窗口关闭时释放资源
            Singleton<PopupInterceptorViewModel>.Instance.Dispose();
            base.OnClosed(e);
        }

        #region 系统托盘菜单事件
        private MainWindow? _mainWindow;

        private void ShowMainWindow(string pageIdOrTargetTag)
        {
            if (_mainWindow?.IsClosed ?? true)
                _mainWindow = new MainWindow(pageIdOrTargetTag);
            else
            {
                _mainWindow.ChangePage(pageIdOrTargetTag);
                if (_mainWindow.WindowState == WindowState.Minimized)
                    _mainWindow.WindowState = WindowState.Normal;
            }
            _mainWindow.Show();
        }

        private void niMenu_Home_Click(object sender, RoutedEventArgs e) => ShowMainWindow("Home");
        private void niMenu_Setting_Click(object sender, RoutedEventArgs e) => ShowMainWindow("Setting");
        private void niMenu_Close_Click(object sender, RoutedEventArgs e) => this.Close();
        #endregion


    }
}
