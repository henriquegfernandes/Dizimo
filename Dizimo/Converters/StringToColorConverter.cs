using System.Diagnostics;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Dizimo.Converters;

/// <summary>
///     Converter que transforma string hex de cor em Brush do Avalonia
/// </summary>
public class StringToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        if (value is string hexColor)
            try
            {
                // Retorna um Brush em vez de Color para funcionar como Background
                var color = Color.Parse(hexColor);
                return new SolidColorBrush(color);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CONVERTER] Erro ao converter cor '{hexColor}': {ex.Message}");
                return new SolidColorBrush(Colors.Gray);
            }

        return new SolidColorBrush(Colors.Gray);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
///     Converter que calcula a largura relativa baseada na quantidade máxima
///     Retorna uma largura em pixels para a barra do gráfico
/// </summary>
public class RelativeWidthConverter : IValueConverter
{
    private const double MaxPixelWidth = 300; // Largura máxima da barra em pixels
    private static int _maxQuantidade = 1;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        if (value is int quantidade && _maxQuantidade > 0)
        {
            // Calcula largura proporcional em pixels
            var larguraPorcentagem = quantidade / (double)_maxQuantidade * MaxPixelWidth;
            Debug.WriteLine(
                $"[CONVERTER] RelativeWidthConverter: {quantidade}/{_maxQuantidade} = {larguraPorcentagem}px");
            return larguraPorcentagem;
        }

        return 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }

    public static void SetMaxQuantidade(int max)
    {
        _maxQuantidade = max > 0 ? max : 1;
        Debug.WriteLine($"[CONVERTER] RelativeWidthConverter.SetMaxQuantidade({max}) definido");
    }
}