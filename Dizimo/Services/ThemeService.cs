using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using System.Linq;
using Avalonia.VisualTree;

namespace Dizimo.Services;

/// <summary>
/// Enumeração de temas suportados pela aplicação
/// </summary>
public enum AppTheme
{
    /// <summary>
    /// Tema claro - cores claras com texto escuro
    /// </summary>
    Light,
    
    /// <summary>
    /// Tema escuro - cores escuras com texto claro
    /// </summary>
    Dark,
    
    /// <summary>
    /// Tema automático - segue a preferência do sistema
    /// </summary>
    Auto
}

/// <summary>
/// Serviço responsável por gerenciar os temas da aplicação
/// Suporta alternância entre temas claro e escuro com SimpleTheme
/// 
/// Para usar: ThemeService.Initialize(this, preferencesService);
/// Para mudar tema: ThemeService.SaveThemePreference(AppTheme.Dark);
/// </summary>
public class ThemeService
{
    private static AppTheme _currentTheme = AppTheme.Light;
    private static global::Avalonia.Application? _application;
    private static IPreferencesService? _preferencesService;
    private const string ThemePreferenceKey = "AppTheme";

    /// <summary>
    /// Inicializa o serviço de temas
    /// </summary>
    public static void Initialize(global::Avalonia.Application app, IPreferencesService? preferencesService = null)
    {
        _application = app;
        _preferencesService = preferencesService;
        
        // Carrega o tema salvo anteriormente
        if (_preferencesService != null)
        {
            var savedTheme = _preferencesService.Get(ThemePreferenceKey, AppTheme.Light.ToString());
            if (Enum.TryParse<AppTheme>(savedTheme, out var theme))
            {
                _currentTheme = theme;
            }
        }
        
        ApplyTheme(_currentTheme);
        System.Diagnostics.Debug.WriteLine($"[THEME] Serviço de temas inicializado com tema: {_currentTheme}");
    }

    /// <summary>
    /// Salva a preferência de tema do usuário e aplica o tema
    /// </summary>
    public static void SaveThemePreference(AppTheme theme)
    {
        _currentTheme = theme;
        
        // Persiste em disco se o IPreferencesService estiver disponível
        if (_preferencesService != null)
        {
            _preferencesService.Set(ThemePreferenceKey, theme.ToString());
            System.Diagnostics.Debug.WriteLine($"[THEME] Tema salvo em preferências: {theme}");
        }
        
        ApplyTheme(theme);
        System.Diagnostics.Debug.WriteLine($"[THEME] Tema salvo e aplicado: {theme}");
    }

    /// <summary>
    /// Retorna a preferência de tema salva
    /// </summary>
    public static AppTheme GetSavedThemePreference()
    {
        if (_preferencesService != null)
        {
            var savedTheme = _preferencesService.Get(ThemePreferenceKey, AppTheme.Light.ToString());
            if (Enum.TryParse<AppTheme>(savedTheme, out var theme))
            {
                return theme;
            }
        }
        
        return _currentTheme;
    }

    /// <summary>
    /// Aplica o tema selecionado ao aplicativo
    /// Atualiza os recursos de aplicação de acordo com o tema
    /// </summary>
    public static void ApplyTheme(AppTheme theme)
    {
        if (_application == null)
        {
            System.Diagnostics.Debug.WriteLine("[THEME] Aviso: Aplicação não inicializada");
            return;
        }

        try
        {
            _currentTheme = theme;
            
            // Determina qual tema será aplicado
            AppTheme effectiveTheme = theme;
            if (theme == AppTheme.Auto)
            {
                effectiveTheme = DetectSystemTheme();
            }

            // Define as cores dos brushes conforme o tema
            if (effectiveTheme == AppTheme.Dark)
            {
                // TEMA ESCURO
                _application.Resources["BrushTextPrimary"] = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)); // Branco
                _application.Resources["BrushTextSecondary"] = new SolidColorBrush(Color.FromArgb(255, 179, 179, 179)); // Cinza claro
                _application.Resources["BrushTextTertiary"] = new SolidColorBrush(Color.FromArgb(255, 128, 128, 128)); // Cinza
                
                _application.Resources["BrushBackgroundPrimary"] = new SolidColorBrush(Color.FromArgb(255, 18, 18, 18)); // #121212
                _application.Resources["BrushBackgroundSecondary"] = new SolidColorBrush(Color.FromArgb(255, 30, 30, 30)); // #1E1E1E
                _application.Resources["BrushBackgroundTertiary"] = new SolidColorBrush(Color.FromArgb(255, 44, 44, 44)); // #2C2C2C
                
                _application.Resources["BrushBorder"] = new SolidColorBrush(Color.FromArgb(255, 56, 56, 56)); // #383838
                _application.Resources["BrushDivider"] = new SolidColorBrush(Color.FromArgb(255, 44, 44, 44)); // #2C2C2C
            }
            else
            {
                // TEMA CLARO
                _application.Resources["BrushTextPrimary"] = new SolidColorBrush(Color.FromArgb(255, 33, 33, 33)); // #212121 Preto
                _application.Resources["BrushTextSecondary"] = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100)); // #646464 Cinza
                _application.Resources["BrushTextTertiary"] = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150)); // #969696 Cinza claro
                
                _application.Resources["BrushBackgroundPrimary"] = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)); // Branco
                _application.Resources["BrushBackgroundSecondary"] = new SolidColorBrush(Color.FromArgb(255, 242, 242, 242)); // #F2F2F2 Cinza bem claro
                _application.Resources["BrushBackgroundTertiary"] = new SolidColorBrush(Color.FromArgb(255, 230, 230, 230)); // #E6E6E6 Cinza
                
                _application.Resources["BrushBorder"] = new SolidColorBrush(Color.FromArgb(255, 220, 220, 220)); // #DCDCDC
                _application.Resources["BrushDivider"] = new SolidColorBrush(Color.FromArgb(255, 230, 230, 230)); // #E6E6E6
            }

            // Aplica o tema no Avalonia
            _application.RequestedThemeVariant = effectiveTheme == AppTheme.Dark 
                ? global::Avalonia.Styling.ThemeVariant.Dark 
                : global::Avalonia.Styling.ThemeVariant.Light;

            System.Diagnostics.Debug.WriteLine($"[THEME] Tema aplicado com sucesso: {effectiveTheme}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[THEME] Erro ao aplicar tema: {ex.Message}");
        }
    }

    /// <summary>
    /// Detecta o tema do sistema operacional
    /// </summary>
    private static AppTheme DetectSystemTheme()
    {
        // TODO: Implementar detecção real de tema do sistema quando disponível
        // Por enquanto, retorna Light como padrão
        return AppTheme.Light;
    }

    /// <summary>
    /// Mapeia índice (0/1/2) para AppTheme
    /// Usado para compatibilidade com ComboBox ou similares
    /// </summary>
    public static int GetThemeIndex(AppTheme theme)
    {
        return theme switch
        {
            AppTheme.Light => 0,
            AppTheme.Dark => 1,
            AppTheme.Auto => 2,
            _ => 0
        };
    }

    /// <summary>
    /// Mapeia índice para AppTheme
    /// </summary>
    public static AppTheme GetThemeFromIndex(int index)
    {
        return index switch
        {
            0 => AppTheme.Light,
            1 => AppTheme.Dark,
            2 => AppTheme.Auto,
            _ => AppTheme.Light
        };
    }

    /// <summary>
    /// Retorna o tema atual em uso
    /// </summary>
    public static AppTheme GetCurrentTheme()
    {
        return _currentTheme;
    }
}

