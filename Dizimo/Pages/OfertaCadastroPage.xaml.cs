using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Dizimo.ViewModels;

namespace Dizimo.Pages;

/// <summary>
///     Página de cadastro de ofertas com funcionalidades de UX para campos numéricos
/// </summary>
public class OfertaCadastroPage : UserControl
{
    public OfertaCadastroPage()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    ///     Limpa o campo de código quando o usuário clica nele (se contiver valor padrão "0")
    /// </summary>
    private void OnCodigoGotFocus(object? sender, RoutedEventArgs e)
    {
        var textBox = sender as TextBox;
        if (textBox != null && textBox.Text == "0")
        {
            textBox.Text = "";
            textBox.CaretIndex = 0;
        }
    }

    /// <summary>
    ///     Executa a busca de dizimista quando o campo de código perde o foco
    /// </summary>
    private async void OnCodigoLostFocus(object? sender, RoutedEventArgs e)
    {
        var dataContext = DataContext as OfertaCadastroViewModel;
        if (dataContext?.BuscarDizimistaCommand?.CanExecute(null) == true)
            await dataContext.BuscarDizimistaCommand.ExecuteAsync(null);
    }

    /// <summary>
    ///     Limpa o campo de valor quando o usuário clica nele (se contiver valor padrão "0")
    /// </summary>
    private void OnValorGotFocus(object? sender, RoutedEventArgs e)
    {
        var textBox = sender as TextBox;
        if (textBox != null && (textBox.Text == "0" || textBox.Text == "0.00" || textBox.Text == "0,00"))
        {
            textBox.Text = "";
            textBox.CaretIndex = 0;
        }
    }

    /// <summary>
    ///     Formata o valor com 2 casas decimais quando o campo perde o foco
    /// </summary>
    private void OnValorLostFocus(object? sender, RoutedEventArgs e)
    {
        var textBox = sender as TextBox;
        if (textBox != null && decimal.TryParse(textBox.Text, out var valor)) textBox.Text = valor.ToString("F2");
    }
}