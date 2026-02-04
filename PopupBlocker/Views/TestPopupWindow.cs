using System.Windows;
using System.Windows.Controls;

namespace PopupBlocker.Views
{
    public class TestPopupWindow : Window
    {
        public TestPopupWindow()
        {
            Title = "测试弹窗 - 这是一个广告窗口";
            Width = 300;
            Height = 200;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var stackPanel = new StackPanel();

            var label = new Label
            {
                Content = "这是一个测试弹窗内容",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var button = new Button
            {
                Content = "关闭",
                Width = 80,
                Margin = new Thickness(0, 10, 0, 0)
            };
            button.Click += (s, e) => Close();

            stackPanel.Children.Add(label);
            stackPanel.Children.Add(button);

            Content = stackPanel;
        }

        public static void ShowTestPopup()
        {
            var popup = new TestPopupWindow();
            popup.Show();
        }
    }
}
