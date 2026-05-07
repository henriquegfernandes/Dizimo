using Avalonia.Controls;
using Avalonia.Markup.Xaml;
namespace Dizimo.Pages;
public partial class DizimistaDetalhesPage : UserControl
{
    public DizimistaDetalhesPage()
    {
        AvaloniaXamlLoader.Load(this);
        System.Diagnostics.Debug.WriteLine("[INFO] DizimistaDetalhesPage inicializado");
    }
}
