using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Globalization;

namespace Dizimo.Converters;

/// <summary>
/// Converter que transforma string hex de cor em Brush do Avalonia
/// </summary>
public class StringToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        if (value is string hexColor)
        {
            try
            {
                // Retorna um Brush em vez de Color para funcionar como Background
                var color = Color.Parse(hexColor);
                return new SolidColorBrush(color);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CONVERTER] Erro ao converter cor '{hexColor}': {ex.Message}");
                return new SolidColorBrush(Colors.Gray);
            }
        }

        return new SolidColorBrush(Colors.Gray);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter que calcula a largura relativa baseada na quantidade máxima
/// Retorna uma largura em pixels para a barra do gráfico
/// </summary>
public class RelativeWidthConverter : IValueConverter
{
    private static int _maxQuantidade = 1;
    private const double MaxPixelWidth = 300; // Largura máxima da barra em pixels

    public static void SetMaxQuantidade(int max)
    {
        _maxQuantidade = max > 0 ? max : 1;
        System.Diagnostics.Debug.WriteLine($"[CONVERTER] RelativeWidthConverter.SetMaxQuantidade({max}) definido");
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        if (value is int quantidade && _maxQuantidade > 0)
        {
            // Calcula largura proporcional em pixels
            var larguraPorcentagem = (quantidade / (double)_maxQuantidade) * MaxPixelWidth;
            System.Diagnostics.Debug.WriteLine($"[CONVERTER] RelativeWidthConverter: {quantidade}/{_maxQuantidade} = {larguraPorcentagem}px");
            return larguraPorcentagem;
        }

        return 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }
}

