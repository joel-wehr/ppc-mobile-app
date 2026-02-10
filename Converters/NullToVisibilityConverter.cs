using System.Globalization;

namespace powered_parachute.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool invert = parameter is string s && s.Equals("invert", StringComparison.OrdinalIgnoreCase);
            bool isNull = value is null || (value is string str && string.IsNullOrEmpty(str));
            return invert ? isNull : !isNull;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
