using System.Diagnostics;
using Avalonia.Markup.Xaml;

namespace Dizimo.Pages;

public class DizimistaCadastroPage : UserControl
{
    public DizimistaCadastroPage()
    {
        AvaloniaXamlLoader.Load(this);
        Debug.WriteLine("[INFO] DizimistaCadastroPage inicializado");
    }
}