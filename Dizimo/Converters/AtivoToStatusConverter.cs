using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Dizimo.Converters
{
    public class AtivoToStatusConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool ativo)
                return ativo ? "Ativo" : "Inativo";
            return "";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string status)
                return status == "Ativo";
            return false;
        }
    }
}
