using Avalonia.Controls;
using Avalonia.Markup.Xaml;
namespace Dizimo.Pages;
public partial class UsuarioCadastroPage : UserControl
{
    public UsuarioCadastroPage()
    {
        AvaloniaXamlLoader.Load(this);
        System.Diagnostics.Debug.WriteLine("[INFO] UsuarioCadastroPage inicializado");
    }
}
