using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Dizimo.Converters;

/// <summary>
/// Converter genérico para validar null/empty com diferentes estratégias
/// Usar ConverterParameter para escolher:
/// - "null" : null → false (padrão)
/// - "empty" : empty/null string → false
/// - "invert" : inverte bool
/// </summary>
public class NullOrEmptyToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var strategy = parameter?.ToString()?.ToLower() ?? "null";

        return strategy switch
        {
            "null" => value is not null,
            "empty" => value is string str ? !string.IsNullOrEmpty(str) : value is not null,
            "invert" => value is bool boolValue ? !boolValue : false,
            _ => value is not null
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

