using System.Globalization;
using powered_parachute.Models.Enums;

namespace powered_parachute.Converters
{
    public class FlightTypeToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is FlightType flightType)
            {
                return flightType switch
                {
                    FlightType.Local => "Local",
                    FlightType.CrossCountry => "Cross Country",
                    FlightType.TrainingDual => "Training (Dual)",
                    FlightType.TrainingSolo => "Training (Solo)",
                    FlightType.Practice => "Practice",
                    FlightType.CheckRide => "Check Ride",
                    _ => flightType.ToString()
                };
            }
            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
