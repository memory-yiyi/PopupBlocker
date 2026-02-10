using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace PopupBlocker.Controls
{
    public class CardToggleSwitch : System.Windows.Controls.ContentControl
    {
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(
                nameof(Icon),
                typeof(IconElement),
                typeof(CardToggleSwitch));

        public object Icon
        {
            get => (IconElement)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(
                nameof(IsChecked),
                typeof(bool),
                typeof(CardToggleSwitch),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                nameof(Command),
                typeof(ICommand),
                typeof(CardToggleSwitch));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        static CardToggleSwitch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(CardToggleSwitch),
                new FrameworkPropertyMetadata(typeof(CardToggleSwitch)));
        }
    }
}
