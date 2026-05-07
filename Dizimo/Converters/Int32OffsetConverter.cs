using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Dizimo.Converters;

public class Int32OffsetConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int intValue && parameter is string offsetStr && int.TryParse(offsetStr, out int offset))
        {
            // Converter de MesRef (1-12) para SelectedIndex (0-11)
            return intValue - offset;
        }
        return 0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int intValue && parameter is string offsetStr && int.TryParse(offsetStr, out int offset))
        {
            // Converter de SelectedIndex (0-11) para MesRef (1-12)
            return intValue + offset;
        }
        return 1;
    }
}
