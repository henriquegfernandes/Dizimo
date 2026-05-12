using System.Diagnostics;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Dizimo.ViewModels;

namespace Dizimo.Pages;

public class MainPage : UserControl
{
    public MainPage()
    {
        AvaloniaXamlLoader.Load(this);
        Loaded += MainPage_Loaded;
        Debug.WriteLine("[INFO] MainPage inicializado");
    }

    private async void MainPage_Loaded(object? sender, RoutedEventArgs e)
    {
        Debug.WriteLine($"[INFO] MainPage_Loaded - DataContext type: {DataContext?.GetType().Name ?? "null"}");

        MainPageViewModel? mainPageVm = null;

        // Tentar obter MainPageViewModel do DataContext
        if (DataContext is ShellViewModel shellVm)
        {
            mainPageVm = shellVm.MainPageVm;
            Debug.WriteLine("[INFO] MainPageViewModel obtido via ShellViewModel");
        }
        else if (DataContext is MainPageViewModel directVm)
        {
            mainPageVm = directVm;
            Debug.WriteLine("[INFO] MainPageViewModel setado diretamente");
        }
        else
        {
            Debug.WriteLine($"[ERRO] DataContext é do tipo inesperado: {DataContext?.GetType().Name}");
            return;
        }

        if (mainPageVm != null)
        {
            Debug.WriteLine("[INFO] Chamando InitializeAsync()...");
            await mainPageVm.InitializeAsync();
            Debug.WriteLine("[INFO] InitializeAsync() completado");
        }
        else
        {
            Debug.WriteLine("[ERRO] MainPageViewModel é null");
        }
    }
}