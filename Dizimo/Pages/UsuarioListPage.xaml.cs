using System.Diagnostics;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Dizimo.ViewModels;

namespace Dizimo.Pages;

public class UsuarioListPage : UserControl
{
    public UsuarioListPage()
    {
        AvaloniaXamlLoader.Load(this);
        Debug.WriteLine("[INFO] UsuarioListPage inicializado");
    }

    public void FilterTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Return)
            if (DataContext is UsuarioListViewModel viewModel)
                viewModel.AplicarFiltrosEnterCommand.Execute(null);
    }

    public void FilterTextBox_LostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is UsuarioListViewModel viewModel) viewModel.AplicarFiltrosEnterCommand.Execute(null);
    }
}