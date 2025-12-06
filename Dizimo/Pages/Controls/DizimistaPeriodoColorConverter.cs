using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.Maui.Graphics;
using Dizimo.Application.Dashboard;

namespace Dizimo.Pages.Controls
{
    public class DizimistaPeriodoColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ObservableCollection<DashboardService.DizimistaPeriodoOfertaData> dados && dados.Count > 0)
            {
                var brushes = new ObservableCollection<Brush>();
                
                // Ordem fixa das cores conforme DashboardService retorna:
                // Verde (#22C55E) - Últimos 2 meses
                // Amarelo (#FBBF24) - 2-6 meses
                // Laranja (#F97316) - 6-12 meses
                // Vermelho (#EF4444) - Mais de 1 ano
                
                var colors = new[] { "#22C55E", "#FBBF24", "#F97316", "#EF4444" };
                
                foreach (var colorHex in colors)
                {
                    if (Color.TryParse(colorHex, out Color parsedColor))
                    {
                        brushes.Add(new SolidColorBrush(parsedColor));
                    }
                }
                
                return brushes;
            }

            return new ObservableCollection<Brush>();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
