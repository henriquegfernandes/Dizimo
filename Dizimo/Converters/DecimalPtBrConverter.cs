using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Dizimo.Converters;

/// <summary>
/// DEPRECATED: Use DecimalFormatterConverter com ConverterParameter="N2"
/// Mantido por compatibilidade com código legado
/// </summary>
[Obsolete("Use DecimalFormatterConverter with ConverterParameter='N2' instead.", false)]
public class DecimalPtBrConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
        {
            var ptBr = new CultureInfo("pt-BR");
            return decimalValue.ToString("N2", ptBr);
        }

        if (value is double doubleValue)
        {
            var ptBr = new CultureInfo("pt-BR");
            return doubleValue.ToString("N2", ptBr);
        }

        return value?.ToString() ?? string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            var ptBr = new CultureInfo("pt-BR");
            if (decimal.TryParse(stringValue, NumberStyles.Number, ptBr, out var result))
            {
                return result;
            }
        }
        return 0m;
    }
}
