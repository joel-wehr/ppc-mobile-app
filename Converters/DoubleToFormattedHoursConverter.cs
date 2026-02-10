using System.Globalization;

namespace powered_parachute.Converters
{
    public class DoubleToFormattedHoursConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double hours)
            {
                return hours.ToString("F1") + "h";
            }
            return "--";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
