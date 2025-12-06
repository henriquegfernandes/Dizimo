using System;
using System.Globalization;

namespace Dizimo.Converters;

/// <summary>
/// Conversor para exibir valores decimais no formato PT-BR (com vírgula como separador decimal)
/// Exemplo: 100.50 -> "100,50"
/// </summary>
public class DecimalPtBrConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
        {
            // Sempre usar PT-BR culture
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
