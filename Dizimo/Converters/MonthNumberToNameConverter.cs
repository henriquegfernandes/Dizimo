using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Dizimo.Converters;

public class MonthNumberToNameConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int month && month >= 1 && month <= 12)
        {
            var cultureInfo = new CultureInfo("pt-BR");
            var date = new DateTime(2024, month, 1);
            string mesNome = date.ToString("MMMM", cultureInfo);
            
            // Converter para Title Case (primeira letra maiúscula)
            mesNome = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(mesNome);
            
            return mesNome;
        }
        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
