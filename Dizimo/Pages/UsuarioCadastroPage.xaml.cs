using System.Diagnostics;
using Avalonia.Markup.Xaml;

namespace Dizimo.Pages;

public class UsuarioCadastroPage : UserControl
{
    public UsuarioCadastroPage()
    {
        AvaloniaXamlLoader.Load(this);
        Debug.WriteLine("[INFO] UsuarioCadastroPage inicializado");
    }
}