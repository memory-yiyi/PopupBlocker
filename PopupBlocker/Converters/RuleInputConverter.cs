namespace PopupBlocker.Converters
{
    public class RuleInputConverter : System.Windows.Data.IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length == 2 &&
                values[0] is string pattern &&
                values[1] is int selectedIndex)
            {
                if (pattern == string.Empty)
                    return null;
                return Tuple.Create(pattern, (Models.RuleType?)selectedIndex ?? Models.RuleType.WindowTitle);
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}