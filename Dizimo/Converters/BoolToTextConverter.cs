using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Dizimo.Converters;

/// <summary>
/// Converter genérico para converter bool em texto com diferentes contextos
/// Usar ConverterParameter para escolher:
/// - "status" : true → "Ativo", false → "Inativo"
/// - "action" : true → "Inativar", false → "Ativar"
/// </summary>
public class BoolToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue)
            return string.Empty;

        var context = parameter?.ToString()?.ToLower() ?? "status";

        return context switch
        {
            "status" => boolValue ? "Ativo" : "Inativo",
            "action" => boolValue ? "Inativar" : "Ativar",
            _ => string.Empty
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (value is string status && status == "Ativo") || 
               (value is string action && action == "Inativar");
    }
}

