using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PopupBlocker.Controls
{
    public class InterceptorRulesCard : ItemsControl
    {
        public static readonly DependencyProperty SaveRuleCommandProperty =
            DependencyProperty.Register(
                nameof(SaveRuleCommand),
                typeof(ICommand),
                typeof(InterceptorRulesCard));

        public ICommand SaveRuleCommand
        {
            get => (ICommand)GetValue(SaveRuleCommandProperty);
            set => SetValue(SaveRuleCommandProperty, value);
        }

        public static readonly DependencyProperty ResetCountCommandProperty =
            DependencyProperty.Register(
                nameof(ResetCountCommand),
                typeof(ICommand),
                typeof(InterceptorRulesCard));

        public ICommand ResetCountCommand
        {
            get => (ICommand)GetValue(ResetCountCommandProperty);
            set => SetValue(ResetCountCommandProperty, value);
        }

        public static readonly DependencyProperty RemoveRuleCommandProperty =
            DependencyProperty.Register(
                nameof(RemoveRuleCommand),
                typeof(ICommand),
                typeof(InterceptorRulesCard));

        public ICommand RemoveRuleCommand
        {
            get => (ICommand)GetValue(RemoveRuleCommandProperty);
            set => SetValue(RemoveRuleCommandProperty, value);
        }

        static InterceptorRulesCard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(InterceptorRulesCard),
                new FrameworkPropertyMetadata(typeof(InterceptorRulesCard)));
        }
    }
}
