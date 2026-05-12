using System.Globalization;
using Avalonia.Data.Converters;
using Dizimo.Domain.Entities;

namespace Dizimo.Converters;

public class MesAnoToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Este converter esperar� receber a entidade Oferta completa
        if (value is Oferta oferta)
        {
            var cultureInfo = new CultureInfo("pt-BR");
            var date = new DateTime(2024, oferta.MesReferencia, 1);
            var mesNome = date.ToString("MMMM", cultureInfo);

            // Converter para Title Case (primeira letra mai�scula)
            mesNome = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(mesNome);

            return $"{mesNome}/{oferta.AnoReferencia}";
        }

        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}