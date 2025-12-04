using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Dizimo.Converters;

public class DecimalToRealConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
        {
            var brasilCulture = new CultureInfo("pt-BR");
            return decimalValue.ToString("C", brasilCulture);
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
