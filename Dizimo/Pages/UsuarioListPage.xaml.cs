using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Dizimo.ViewModels;
using Avalonia.Controls;

namespace Dizimo.Pages;

public partial class UsuarioListPage : UserControl
{
    public UsuarioListPage()
    {
        AvaloniaXamlLoader.Load(this);
        System.Diagnostics.Debug.WriteLine("[INFO] UsuarioListPage inicializado");
    }

    public void FilterTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Return)
        {
            if (this.DataContext is UsuarioListViewModel viewModel)
            {
                viewModel.AplicarFiltrosEnterCommand.Execute(null);
            }
        }
    }

    public void FilterTextBox_LostFocus(object? sender, RoutedEventArgs e)
    {
        if (this.DataContext is UsuarioListViewModel viewModel)
        {
            viewModel.AplicarFiltrosEnterCommand.Execute(null);
        }
    }
}
