using Avalonia.Markup.Xaml;
namespace Dizimo.Pages;
public class DizimistaCadastroPage : UserControl
{
    public DizimistaCadastroPage()
    {
        AvaloniaXamlLoader.Load(this);
        System.Diagnostics.Debug.WriteLine("[INFO] DizimistaCadastroPage inicializado");
    }
}
