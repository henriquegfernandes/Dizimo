using System.Globalization;
using Avalonia.Data.Converters;

namespace Dizimo.Converters;

public class BoolToWidthConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isExpanded) return isExpanded ? 260.0 : 0.0;
        return 260.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}