using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Dizimo.Converters
{
    public class GreaterThanZeroToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int intValue)
                return intValue > 0;
            return false;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // OneWay converter - não suporta ConvertBack
            return Avalonia.Data.BindingOperations.DoNothing;
        }
    }
}
