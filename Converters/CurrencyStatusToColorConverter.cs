using System.Globalization;
using powered_parachute.Models.Enums;

namespace powered_parachute.Converters
{
    public class CurrencyStatusToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is CurrencyStatus status)
            {
                return status switch
                {
                    CurrencyStatus.Current => Color.FromArgb("#28A745"),  // Success
                    CurrencyStatus.Warning => Color.FromArgb("#FFD700"),  // Tertiary/Gold
                    CurrencyStatus.Expired => Color.FromArgb("#FF9933"),  // Secondary/Orange
                    _ => Color.FromArgb("#6E6E6E")                       // Gray500
                };
            }
            return Color.FromArgb("#6E6E6E");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
