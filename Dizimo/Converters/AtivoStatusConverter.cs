using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Dizimo.Converters;

public class AtivoStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value is bool ativo && ativo) ? "Ativo" : "Inativo";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
