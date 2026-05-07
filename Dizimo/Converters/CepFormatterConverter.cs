using System.Globalization;
using Avalonia.Data.Converters;

namespace Dizimo.Converters;

public class CepFormatterConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string cep || string.IsNullOrWhiteSpace(cep))
            return string.Empty;

        // Remove caracteres não numéricos
        var cleaned = new string(cep.Where(char.IsDigit).ToArray());

        if (cleaned.Length == 0)
            return string.Empty;

        // Formatar: 99999-999
        if (cleaned.Length == 8)
            return $"{cleaned.Substring(0, 5)}-{cleaned.Substring(5)}";

        return cep;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string formatted)
            return string.Empty;

        // Remove caracteres não numéricos
        return new string(formatted.Where(char.IsDigit).ToArray());
    }
}
