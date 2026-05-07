using Avalonia.Controls;
using Avalonia.Markup.Xaml;
namespace Dizimo;
public partial class AppShell : UserControl
{
    public AppShell()
    {
        AvaloniaXamlLoader.Load(this);
        System.Diagnostics.Debug.WriteLine("[INFO] AppShell inicializado");
    }
}
