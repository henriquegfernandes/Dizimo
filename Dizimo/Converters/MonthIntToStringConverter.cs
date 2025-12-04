using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Dizimo.Converters;

public class MonthIntToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int month && month >= 1 && month <= 12)
        {
            var meses = new[]
            {
                "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
                "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
            };
            return meses[month - 1];
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string mesNome)
        {
            var meses = new[]
            {
                "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
                "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
            };
            
            for (int i = 0; i < meses.Length; i++)
            {
                if (meses[i].Equals(mesNome, StringComparison.OrdinalIgnoreCase))
                {
                    return i + 1;
                }
            }
        }
        return 1;
    }
}
