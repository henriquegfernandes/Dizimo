using System.Globalization;
using Avalonia.Data.Converters;

namespace Dizimo.Converters;

public class StringToMesVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string visualizacao) return visualizacao == "Mês";
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return boolValue ? "Mês" : string.Empty;
        return string.Empty;
    }
}