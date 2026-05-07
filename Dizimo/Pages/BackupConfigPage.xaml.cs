using Avalonia.Controls;
using Avalonia.Markup.Xaml;
namespace Dizimo.Pages;
public partial class BackupConfigPage : UserControl
{
    public BackupConfigPage()
    {
        AvaloniaXamlLoader.Load(this);
        System.Diagnostics.Debug.WriteLine("[INFO] BackupConfigPage inicializado");
    }
}
