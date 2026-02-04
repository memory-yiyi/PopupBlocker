namespace PopupBlocker.Views
{
    /// <summary>
    /// Selector.xaml 的交互逻辑
    /// </summary>
    public partial class Selector
    {
        public Selector()
        {
            InitializeComponent();
            this.DataContext = new ViewModels.SelectorViewModel();
        }
    }
}
