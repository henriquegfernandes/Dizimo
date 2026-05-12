using System.Globalization;
using Avalonia.Data.Converters;

namespace Dizimo.Converters;

public class IntToIndexConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Converte m�s (1-12) para �ndice (0-11)
        if (value is int mes && mes >= 1 && mes <= 12) return mes - 1;
        return 0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Converte �ndice (0-11) para m�s (1-12)
        if (value is int index && index >= 0 && index <= 11) return index + 1;
        return DateTime.Now.Month;
    }
}