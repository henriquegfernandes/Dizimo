using System.Diagnostics;
using Avalonia.Markup.Xaml;

namespace Dizimo.Pages;

public class BackupConfigPage : UserControl
{
    public BackupConfigPage()
    {
        AvaloniaXamlLoader.Load(this);
        Debug.WriteLine("[INFO] BackupConfigPage inicializado");
    }
}