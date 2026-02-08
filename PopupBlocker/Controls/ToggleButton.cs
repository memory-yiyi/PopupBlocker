using System.Windows;
using System.Windows.Controls;

namespace PopupBlocker.Controls
{
    public class ToggleButton : Button
    {
        protected override void OnClick()
        {
            IsChecked = !IsChecked;
            base.OnClick();
        }

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(
                nameof(IsChecked),
                typeof(bool),
                typeof(ToggleButton),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
            );

        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        public static readonly DependencyProperty CheckedContentProperty =
            DependencyProperty.Register(
                nameof(CheckedContent),
                typeof(object),
                typeof(ToggleButton));

        public object CheckedContent
        {
            get => GetValue(CheckedContentProperty);
            set => SetValue(CheckedContentProperty, value);
        }

        public static readonly DependencyProperty UncheckedContentProperty =
            DependencyProperty.Register(
                nameof(UncheckedContent),
                typeof(object),
                typeof(ToggleButton));

        public object UncheckedContent
        {
            get => GetValue(UncheckedContentProperty);
            set => SetValue(UncheckedContentProperty, value);
        }

        static ToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ToggleButton),
                new FrameworkPropertyMetadata(typeof(ToggleButton)));
        }
    }
}
