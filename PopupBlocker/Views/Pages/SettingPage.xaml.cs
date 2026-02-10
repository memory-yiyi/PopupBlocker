namespace PopupBlocker.Views.Pages
{
    /// <summary>
    /// SettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingPage : System.Windows.Controls.Page
    {
        public SettingPage()
        {
            InitializeComponent();
            this.DataContext = Utility.Commons.Singleton<ViewModels.SettingViewModel>.Instance;
        }
    }
}
