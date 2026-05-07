using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dizimo.ViewModels;
using System.Collections.ObjectModel;

namespace Dizimo.Pages;

public partial class MainPage : UserControl
{
    public MainPage()
    {
        AvaloniaXamlLoader.Load(this);
        Loaded += MainPage_Loaded;
        System.Diagnostics.Debug.WriteLine("[INFO] MainPage inicializado");
    }

    private async void MainPage_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[INFO] MainPage_Loaded - DataContext type: {DataContext?.GetType().Name ?? "null"}");
        
        MainPageViewModel? mainPageVm = null;

        // Tentar obter MainPageViewModel do DataContext
        if (DataContext is ShellViewModel shellVm)
        {
            mainPageVm = shellVm.MainPageVm;
            System.Diagnostics.Debug.WriteLine("[INFO] MainPageViewModel obtido via ShellViewModel");
        }
        else if (DataContext is MainPageViewModel directVm)
        {
            mainPageVm = directVm;
            System.Diagnostics.Debug.WriteLine("[INFO] MainPageViewModel setado diretamente");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] DataContext é do tipo inesperado: {DataContext?.GetType().Name}");
            return;
        }

        if (mainPageVm != null)
        {
            System.Diagnostics.Debug.WriteLine("[INFO] Chamando InitializeAsync()...");
            await mainPageVm.InitializeAsync();
            System.Diagnostics.Debug.WriteLine("[INFO] InitializeAsync() completado");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[ERRO] MainPageViewModel é null");
        }
    }
}
