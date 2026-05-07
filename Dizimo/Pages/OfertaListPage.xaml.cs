using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Dizimo.ViewModels;

namespace Dizimo.Pages;

public class OfertaListPage : UserControl
{
    public OfertaListPage()
    {
        AvaloniaXamlLoader.Load(this);
        System.Diagnostics.Debug.WriteLine("[INFO] OfertaListPage inicializado");
    }

    /// <summary>
    /// Aplica filtro ao pressionar Enter no campo de texto
    /// </summary>
    private void OnFiltroKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Return)
        {
            var dataContext = this.DataContext as OfertaListViewModel;
            if (dataContext?.AplicarFiltrosCommand.CanExecute(null) == true)
            {
                dataContext.AplicarFiltrosCommand.Execute(null);
            }
        }
    }

    /// <summary>
    /// Aplica filtro ao tirar foco do campo de texto
    /// </summary>
    private void OnFiltroLostFocus(object? sender, RoutedEventArgs e)
    {
        var dataContext = this.DataContext as OfertaListViewModel;
        if (dataContext?.AplicarFiltrosCommand.CanExecute(null) == true)
        {
            dataContext.AplicarFiltrosCommand.Execute(null);
        }
    }
}
