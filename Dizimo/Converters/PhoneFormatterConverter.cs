using System.Globalization;
using Avalonia.Data.Converters;

namespace Dizimo.Converters;

public class PhoneFormatterConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string phone || string.IsNullOrWhiteSpace(phone))
            return string.Empty;

        // Remove caracteres não numéricos
        var cleaned = new string(phone.Where(char.IsDigit).ToArray());

        if (cleaned.Length == 0)
            return string.Empty;

        // Formatar: (99) 9999-9999 ou (99) 99999-9999
        if (cleaned.Length == 10)
            return $"({cleaned.Substring(0, 2)}) {cleaned.Substring(2, 4)}-{cleaned.Substring(6)}";
        
        if (cleaned.Length == 11)
            return $"({cleaned.Substring(0, 2)}) {cleaned.Substring(2, 5)}-{cleaned.Substring(7)}";

        return phone;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string formatted)
            return string.Empty;

        // Remove caracteres não numéricos
        return new string(formatted.Where(char.IsDigit).ToArray());
    }
}
