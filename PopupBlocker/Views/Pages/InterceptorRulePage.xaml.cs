namespace PopupBlocker.Views.Pages
{
    /// <summary>
    /// InterceptorRulePage.xaml 的交互逻辑
    /// </summary>
    public partial class InterceptorRulePage : System.Windows.Controls.Page
    {
        public InterceptorRulePage()
        {
            InitializeComponent();
            this.DataContext = Utility.Commons.Singleton<ViewModels.PopupInterceptorViewModel>.Instance;
        }
    }
}
