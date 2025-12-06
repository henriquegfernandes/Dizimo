using System;
using System.Globalization;

namespace Dizimo.Converters;

/// <summary>
/// Conversor para exibir datas no formato PT-BR (dd/MM/yyyy)
/// Exemplo: 2024-01-15 -> "15/01/2024"
/// </summary>
public class DatePtBrConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime dateValue)
        {
            var ptBr = new CultureInfo("pt-BR");
            return dateValue.ToString("dd/MM/yyyy", ptBr);
        }

        return value?.ToString() ?? string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            var ptBr = new CultureInfo("pt-BR");
            if (DateTime.TryParseExact(stringValue, "dd/MM/yyyy", ptBr, DateTimeStyles.None, out var result))
            {
                return result;
            }
        }
        return DateTime.Now;
    }
}
