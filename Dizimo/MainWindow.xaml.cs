using System.Diagnostics;
using Avalonia.Markup.Xaml;

namespace Dizimo;

/// <summary>
///     Janela principal da aplicação - Avalonia UI
///     Funciona como container para exibir diferentes Views
/// </summary>
public class MainWindow : Window
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
        Debug.WriteLine("[INFO] MainWindow inicializado");
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        Debug.WriteLine("[INFO] Aplicação finalizada");
    }
}