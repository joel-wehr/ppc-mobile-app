using System.Globalization;
using powered_parachute.Models.Enums;

namespace powered_parachute.Converters
{
    public class FlightTypeToBadgeColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is FlightType flightType)
            {
                return flightType switch
                {
                    FlightType.Local => Color.FromArgb("#003366"),       // Primary
                    FlightType.CrossCountry => Color.FromArgb("#28A745"), // Success
                    FlightType.TrainingDual => Color.FromArgb("#FF9933"), // Secondary
                    FlightType.TrainingSolo => Color.FromArgb("#FFD700"), // Tertiary
                    FlightType.Practice => Color.FromArgb("#0055A5"),    // PrimaryLight
                    FlightType.CheckRide => Color.FromArgb("#D600AA"),   // Magenta
                    _ => Color.FromArgb("#919191")                       // Gray400
                };
            }
            return Color.FromArgb("#919191");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
