using System.Globalization;

namespace powered_parachute.Converters
{
    public class CompletionColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not int percentage)
                return Color.FromArgb("#6E6E6E"); // Gray500

            return percentage switch
            {
                >= 90 => Color.FromArgb("#28A745"), // Success Green
                >= 70 => Color.FromArgb("#FFD700"), // Gold/Warning
                _ => Color.FromArgb("#FF9933")      // Secondary Orange
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
