using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Dizimo.Domain.Entities;

namespace Dizimo.Converters;

/// <summary>
///     Converter para verificar se um dizimista está selecionado na coleção de selecionados
///     Recebe como MultiBinding: [0] = Dizimista atual, [1] = DizimistasSelecionados coleção
/// </summary>
public class DizimistaIsSelectedConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2)
            return false;

        // values[0] = Dizimista atual (do DataContext do CheckBox através do DataTemplate)
        // values[1] = DizimistasSelecionados (a coleção do ViewModel)

        // Na verdade, vamos receber o Dizimista através do path DataContext
        var dizimista = values[0] as Dizimista;
        var selecionados = values[1] as ObservableCollection<Dizimista>;

        if (dizimista is null || selecionados is null)
            return false;

        return selecionados.Contains(dizimista);
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        // OneWay converter - não suporta ConvertBack
        return new[] { BindingOperations.DoNothing, BindingOperations.DoNothing };
    }
}