using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Dizimo.Converters;

/// <summary>
/// Converter genérico para formatar valores decimais em diferentes formatos
/// Usar ConverterParameter para escolher:
/// - "N2" : Número com 2 casas decimais (100.50 → "100,50")
/// - "C" : Moeda (100.50 → "R$ 100,50")
/// Padrão: "N2" (número com 2 casas)
/// </summary>
public class DecimalFormatterConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var format = parameter?.ToString()?.ToUpper() ?? "N2";
        var ptBr = new CultureInfo("pt-BR");

        if (value is decimal decimalValue)
            return decimalValue.ToString(format, ptBr);

        if (value is double doubleValue)
            return doubleValue.ToString(format, ptBr);

        return value?.ToString() ?? string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string stringValue)
            return 0m;

        var ptBr = new CultureInfo("pt-BR");
        return decimal.TryParse(stringValue, NumberStyles.Number, ptBr, out var result) ? result : 0m;
    }
}

