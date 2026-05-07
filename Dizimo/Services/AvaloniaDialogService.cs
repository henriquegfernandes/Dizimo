using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Controls.Primitives;
using Avalonia.Platform.Storage;
namespace Dizimo.Services;
/// <summary>
/// Implementação otimizada de diálogos para Avalonia UI
/// Fornece diálogos nativos responsivos
/// </summary>
public class AvaloniaDialogService : IDialogService
{
    private readonly Lazy<Window?> _mainWindow = new(() => GetMainWindow());
    private static Window? GetMainWindow()
        => Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
    public async Task ShowAlertAsync(string title, string message, string buttonText = "OK")
    {
        var window = _mainWindow.Value ?? GetMainWindow();
        if (window is null)
        {
            System.Diagnostics.Debug.WriteLine($"[DIALOG] Alerta - {title}: {message}");
            return;
        }
        await MessageBoxHelper.Show(window, message, title, buttonText);
    }
    public async Task<bool> ShowConfirmAsync(string title, string message, string acceptText = "Sim", string cancelText = "Não")
    {
        var window = _mainWindow.Value ?? GetMainWindow();
        if (window is null)
        {
            System.Diagnostics.Debug.WriteLine($"[DIALOG] Confirmação - {title}: {message}");
            return false;
        }
        var result = await MessageBoxHelper.Show(window, message, title, acceptText, cancelText);
        return result == 0;
    }
    public async Task ShowErrorAsync(string message)
    {
        var window = _mainWindow.Value ?? GetMainWindow();
        if (window is null)
        {
            System.Diagnostics.Debug.WriteLine($"[DIALOG] Erro: {message}");
            return;
        }
        await MessageBoxHelper.Show(window, message, "❌ Erro", "OK");
    }
    public async Task ShowSuccessAsync(string message)
    {
        var window = _mainWindow.Value ?? GetMainWindow();
        if (window is null)
        {
            System.Diagnostics.Debug.WriteLine($"[DIALOG] Sucesso: {message}");
            return;
        }
        await MessageBoxHelper.Show(window, message, "✓ Sucesso", "OK");
    }
     public async Task ShowInfoAsync(string title, string message)
     {
         var window = _mainWindow.Value ?? GetMainWindow();
         if (window is null)
         {
             System.Diagnostics.Debug.WriteLine($"[DIALOG] Informação - {title}: {message}");
             return;
         }
         await MessageBoxHelper.Show(window, message, title, "OK");
     }

     public async Task<string?> ShowFolderPickerAsync(string title, string? initialPath = null)
     {
         var window = _mainWindow.Value ?? GetMainWindow();
         if (window is null)
         {
             System.Diagnostics.Debug.WriteLine($"[DIALOG] Folder Picker - {title}");
             return null;
         }

         var topLevel = TopLevel.GetTopLevel(window);
         if (topLevel?.StorageProvider is null)
         {
             System.Diagnostics.Debug.WriteLine("[DIALOG] StorageProvider indisponível");
             return null;
         }

         try
         {
             var suggestedPath = string.IsNullOrEmpty(initialPath) 
                 ? null 
                 : new Uri(initialPath);

             var result = await topLevel.StorageProvider.OpenFolderPickerAsync(
                 new FolderPickerOpenOptions
                 {
                     Title = title,
                     AllowMultiple = false,
                     SuggestedStartLocation = suggestedPath != null ? await topLevel.StorageProvider.TryGetFolderFromPathAsync(suggestedPath) : null
                 }
             );

             return result.Count > 0 ? result[0].Path.LocalPath : null;
         }
         catch (Exception ex)
         {
             System.Diagnostics.Debug.WriteLine($"[DIALOG] Erro ao abrir folder picker: {ex.Message}");
             return null;
         }
     }

     public async Task<string?> ShowFilePickerAsync(string title, string? initialPath = null, string[]? filters = null)
     {
         var window = _mainWindow.Value ?? GetMainWindow();
         if (window is null)
         {
             System.Diagnostics.Debug.WriteLine($"[DIALOG] File Picker - {title}");
             return null;
         }

         var topLevel = TopLevel.GetTopLevel(window);
         if (topLevel?.StorageProvider is null)
         {
             System.Diagnostics.Debug.WriteLine("[DIALOG] StorageProvider indisponível");
             return null;
         }

         try
         {
             var suggestedPath = string.IsNullOrEmpty(initialPath) 
                 ? null 
                 : new Uri(initialPath);

             var fileTypeFilters = new List<FilePickerFileType>();
             
             if (filters != null && filters.Length > 0)
             {
                 foreach (var filter in filters)
                 {
                     fileTypeFilters.Add(new FilePickerFileType(filter) { Patterns = new[] { $"*.{filter}" } });
                 }
             }
             else
             {
                 // Padrão: mostrar todos os arquivos
                 fileTypeFilters.Add(new FilePickerFileType("Todos os arquivos") { Patterns = new[] { "*.*" } });
             }

             var result = await topLevel.StorageProvider.OpenFilePickerAsync(
                 new FilePickerOpenOptions
                 {
                     Title = title,
                     AllowMultiple = false,
                     FileTypeFilter = fileTypeFilters,
                     SuggestedStartLocation = suggestedPath != null ? await topLevel.StorageProvider.TryGetFolderFromPathAsync(suggestedPath) : null
                 }
             );

             return result.Count > 0 ? result[0].Path.LocalPath : null;
         }
         catch (Exception ex)
         {
             System.Diagnostics.Debug.WriteLine($"[DIALOG] Erro ao abrir file picker: {ex.Message}");
             return null;
         }
     }
}
/// <summary>
/// Classe auxiliar para gerenciar caixas de mensagem
/// </summary>
internal static class MessageBoxHelper
{
    public static async Task Show(Window owner, string message, string title, string buttonText)
    {
        var dialog = new MessageBoxDialog { Title = title, Message = message, Buttons = new[] { buttonText } };
        await dialog.ShowDialog(owner);
    }
    public static async Task<int> Show(Window owner, string message, string title, string button1, string button2)
    {
        var dialog = new MessageBoxDialog { Title = title, Message = message, Buttons = new[] { button1, button2 } };
        return await dialog.ShowDialogAsync(owner);
    }
}
/// <summary>
/// Diálogo personalizado otimizado para Avalonia
/// </summary>
internal class MessageBoxDialog : Window
{
    private int _result = 1;
    public string Message { get; set; } = string.Empty;
    public string[] Buttons { get; set; } = Array.Empty<string>();
    
    public MessageBoxDialog()
    {
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Topmost = true;
        Width = 500;
        Height = 320;
        CanResize = false;
        ShowInTaskbar = false;
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        
        if (Content == null)
        {
            BuildContent();
        }
    }

    private void BuildContent()
    {
        // Usar temas da aplicação
        var app = Avalonia.Application.Current;
        object? bgObj = null;
        object? textObj = null;
        object? primaryObj = null;
        
        app?.TryGetResource("BrushBackgroundPrimary", out bgObj);
        app?.TryGetResource("BrushTextPrimary", out textObj);
        app?.TryGetResource("BrushPrimary", out primaryObj);
        
        var bgBrush = (bgObj as Brush) ?? new SolidColorBrush(Colors.White);
        var textBrush = (textObj as Brush) ?? new SolidColorBrush(Colors.Black);
        var primaryBrush = (primaryObj as Brush) ?? new SolidColorBrush(Colors.Red);
        
        // Main container com Grid para distribuição de altura
        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitions
            {
                new RowDefinition(new GridLength(4, GridUnitType.Star)),  // 80% - Content
                new RowDefinition(GridLength.Auto),                        // 1px - Separator
                new RowDefinition(new GridLength(1, GridUnitType.Star))   // 20% - Footer
            }
        };
        
        // Content area (mensagem) - Row 0
        var textBlock = new TextBlock
        {
            Text = Message,
            TextWrapping = TextWrapping.Wrap,
            Foreground = textBrush,
            FontSize = 13,
            LineHeight = 22,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            MinHeight = 220
        };
        
        var scrollViewer = new ScrollViewer
        {
            Content = textBlock,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Padding = new Avalonia.Thickness(20, 20, 20, 15),
            Background = bgBrush
        };
        
        Grid.SetRow(scrollViewer, 0);
        mainGrid.Children.Add(scrollViewer);
        
        // Separator (linha divisória) - Row 1
        var separator = new Border
        {
            Background = new SolidColorBrush(new Color(32, 0, 0, 0)),
            Height = 1
        };
        Grid.SetRow(separator, 1);
        mainGrid.Children.Add(separator);
        
        // Footer area (botões) - Row 2
        var footerBorder = new Border
        {
            Padding = new Avalonia.Thickness(20, 12, 20, 12),
            Background = bgBrush,
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            }
        };
        Grid.SetRow(footerBorder, 2);
        
        var buttonStack = (StackPanel)footerBorder.Child!;
        
        for (int i = 0; i < Buttons.Length; i++)
        {
            var index = i;
            var btn = new Button
            {
                Content = Buttons[i],
                MinWidth = 90,
                Padding = new Avalonia.Thickness(12, 8),
                FontSize = 12
            };
            
            if (i == 0)
            {
                // Botão Primary: fundo vermelho, texto branco
                btn.Classes.Add("Primary");
                btn.Foreground = new SolidColorBrush(Colors.White);
            }
            else
            {
                // Botão Secondary: texto vermelho (cor primária)
                btn.Classes.Add("Secondary");
                btn.Foreground = primaryBrush;
            }
            
            btn.Click += (_, _) => { _result = index; Close(); };
            buttonStack.Children.Add(btn);
        }
        mainGrid.Children.Add(footerBorder);
        
        Content = mainGrid;
        Background = bgBrush;
    }

    public async Task<int> ShowDialogAsync(Window owner)
    {
        await ShowDialog(owner);
        return _result;
    }
}
