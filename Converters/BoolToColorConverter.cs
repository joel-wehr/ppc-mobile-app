using System.Globalization;

namespace powered_parachute.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not bool boolValue || parameter is not string colorPair)
                return Colors.Transparent;

            var colors = colorPair.Split('|');
            if (colors.Length != 2) return Colors.Transparent;

            var colorName = boolValue ? colors[0] : colors[1];

            // Map color names to actual colors
            return colorName switch
            {
                "Success" => Color.FromArgb("#28A745"),
                "Primary" => Color.FromArgb("#003366"),      // Navy Blue
                "Secondary" => Color.FromArgb("#FF9933"),    // Warm Orange
                "Gray100" => Color.FromArgb("#E1E1E1"),
                "Gray200" => Color.FromArgb("#C8C8C8"),
                "Gray300" => Color.FromArgb("#ACACAC"),
                "Gray400" => Color.FromArgb("#919191"),
                "Gray500" => Color.FromArgb("#6E6E6E"),
                "Gray600" => Color.FromArgb("#404040"),
                "Black" => Colors.Black,
                "White" => Colors.White,
                _ => Colors.Transparent
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
