using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Dizimo
{
    /// <summary>
    /// Janela principal da aplicação - Avalonia UI
    /// Funciona como container para exibir diferentes Views
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            AvaloniaXamlLoader.Load(this);
            System.Diagnostics.Debug.WriteLine("[INFO] MainWindow inicializado");
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            System.Diagnostics.Debug.WriteLine("[INFO] Aplicação finalizada");
        }
    }
}
