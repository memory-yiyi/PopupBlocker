using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PopupBlocker.Controls
{
    public class InterceptorRuleCard : ItemsControl
    {
        public static readonly DependencyProperty SaveRuleCommandProperty =
            DependencyProperty.Register(
                nameof(SaveRuleCommand),
                typeof(ICommand),
                typeof(InterceptorRuleCard));

        public ICommand SaveRuleCommand
        {
            get => (ICommand)GetValue(SaveRuleCommandProperty);
            set => SetValue(SaveRuleCommandProperty, value);
        }

        public static readonly DependencyProperty RemoveRuleCommandProperty =
            DependencyProperty.Register(
                nameof(RemoveRuleCommand),
                typeof(ICommand),
                typeof(InterceptorRuleCard));

        public ICommand RemoveRuleCommand
        {
            get => (ICommand)GetValue(RemoveRuleCommandProperty);
            set => SetValue(RemoveRuleCommandProperty, value);
        }

        static InterceptorRuleCard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(InterceptorRuleCard),
                new FrameworkPropertyMetadata(typeof(InterceptorRuleCard)));
        }
    }
}
