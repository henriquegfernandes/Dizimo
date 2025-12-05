using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.Maui.Graphics;
using Dizimo.Application.Dashboard;

namespace Dizimo.Pages.Controls
{
    public class ColorListConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ObservableCollection<DashboardService.DizimistaPeriodoOfertaData> dados && dados.Count > 0)
            {
                var colors = new ObservableCollection<Brush>();
                foreach (var item in dados)
                {
                    if (Color.TryParse(item.Cor, out Color parsedColor))
                    {
                        colors.Add(new SolidColorBrush(parsedColor));
                    }
                }
                return colors;
            }

            return new ObservableCollection<Brush>();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
