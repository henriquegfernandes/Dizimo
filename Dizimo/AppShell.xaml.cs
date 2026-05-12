using System.Diagnostics;
using Avalonia.Markup.Xaml;

namespace Dizimo;

public class AppShell : UserControl
{
    public AppShell()
    {
        AvaloniaXamlLoader.Load(this);
        Debug.WriteLine("[INFO] AppShell inicializado");
    }
}