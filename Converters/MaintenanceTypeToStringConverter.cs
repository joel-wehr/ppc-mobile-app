using System.Globalization;
using powered_parachute.Models.Enums;

namespace powered_parachute.Converters
{
    public class MaintenanceTypeToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is MaintenanceType maintenanceType)
            {
                return maintenanceType switch
                {
                    MaintenanceType.Inspection => "Inspection",
                    MaintenanceType.Repair => "Repair",
                    MaintenanceType.Service => "Service",
                    MaintenanceType.Overhaul => "Overhaul",
                    _ => maintenanceType.ToString()
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
