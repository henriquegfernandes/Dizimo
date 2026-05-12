using System.Diagnostics;
using Avalonia.Markup.Xaml;

namespace Dizimo.Pages;

public class DizimistaDetalhesPage : UserControl
{
    public DizimistaDetalhesPage()
    {
        AvaloniaXamlLoader.Load(this);
        Debug.WriteLine("[INFO] DizimistaDetalhesPage inicializado");
    }
}