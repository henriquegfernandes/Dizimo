using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Services;

namespace Dizimo.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IAuthenticationService _authenticationService;

    public static string UsuarioNomeStatic => SessaoService.UsuarioNome ?? "Não logado";
    public static string UsuarioPerfilStatic => SessaoService.Perfil?.ToString() ?? "";
    public static bool IsAdminStatic => SessaoService.IsAdmin;

    private string _usuarioNome = string.Empty;
    public string UsuarioNome
    {
        get => SessaoService.UsuarioNome ?? "Não logado";
    }

    private string _usuarioPerfil = string.Empty;
    public string UsuarioPerfil
    {
        get => SessaoService.Perfil?.ToString() ?? "";
    }

    public bool IsAdmin
    {
        get => SessaoService.IsAdmin;
    }

    public MainViewModel(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    /// <summary>
    /// Define o callback a ser executado após logout bem-sucedido (DEPRECATED - usar IAuthenticationService)
    /// </summary>
    public void SetOnLogout(Func<Task> onLogout)
    {
        // Este método mantém compatibilidade com código legado
        // A configuração real é feita via IAuthenticationService no AppRootViewModel
        System.Diagnostics.Debug.WriteLine("[AUTH] SetOnLogout (deprecated) chamado - use IAuthenticationService.SetOnLogoutComplete");
    }

    [RelayCommand]
    public async Task LogoutAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[AUTH] Logout iniciado");
            // Usa o IAuthenticationService para realizar logout
            await _authenticationService.PerformLogoutAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao fazer logout: {ex.Message}");
        }
    }
}
