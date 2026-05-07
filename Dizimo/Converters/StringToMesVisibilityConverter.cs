using Avalonia.Data.Converters;
using System.Globalization;

namespace Dizimo.Converters
{
    public class StringToMesVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string visualizacao)
            {
                return visualizacao == "Mês";
            }
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
