namespace Dizimo.Services;

/// <summary>
/// Serviço de navegação agnostic para MAUI
/// </summary>
public partial interface INavigationService
{
    Task NavigateToAsync(string route, bool animated = true);
    Task NavigateBackAsync(bool animated = true);
    Task NavigateToRootAsync(string route, bool animated = true);
}

