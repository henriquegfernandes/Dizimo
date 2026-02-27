using Microsoft.Maui.Controls;

namespace Dizimo.Services;

public class ThemeService
{
    private const string ThemePreferenceKey = "app_theme_preference";

    /// <summary>
    /// Salva a preferência de tema do usuário
    /// </summary>
    public static void SaveThemePreference(AppTheme theme)
    {
        Preferences.Set(ThemePreferenceKey, theme.ToString());
    }

    /// <summary>
    /// Carrega a preferência de tema salva anteriormente
    /// </summary>
    public static AppTheme GetSavedThemePreference()
    {
        var savedTheme = Preferences.Get(ThemePreferenceKey, AppTheme.Light.ToString());
        
        if (Enum.TryParse<AppTheme>(savedTheme, out var theme))
        {
            return theme;
        }

        return AppTheme.Light;
    }

    /// <summary>
    /// Aplica o tema ao aplicativo
    /// </summary>
    public static void ApplyTheme(AppTheme theme)
    {
        Microsoft.Maui.Controls.Application.Current?.UserAppTheme = theme;
    }

    /// <summary>
    /// Obtém o índice do tema para usar em SegmentedControl (0 = Light, 1 = Dark)
    /// </summary>
    public static int GetThemeIndex(AppTheme theme)
    {
        return theme == AppTheme.Light ? 0 : 1;
    }

    /// <summary>
    /// Converte índice de SegmentedControl para AppTheme
    /// </summary>
    public static AppTheme GetThemeFromIndex(int index)
    {
        return index == 0 ? AppTheme.Light : AppTheme.Dark;
    }
}
