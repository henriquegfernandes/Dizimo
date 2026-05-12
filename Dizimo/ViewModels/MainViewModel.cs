using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Dizimo.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IAuthenticationService _authenticationService;

    private string _usuarioNome = string.Empty;

    private string _usuarioPerfil = string.Empty;

    public MainViewModel(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public static string UsuarioNomeStatic => SessaoService.UsuarioNome ?? "Não logado";
    public static string UsuarioPerfilStatic => SessaoService.Perfil?.ToString() ?? "";
    public static bool IsAdminStatic => SessaoService.IsAdmin;
    public string UsuarioNome => SessaoService.UsuarioNome ?? "Não logado";
    public string UsuarioPerfil => SessaoService.Perfil?.ToString() ?? "";

    public bool IsAdmin => SessaoService.IsAdmin;

    /// <summary>
    ///     Define o callback a ser executado após logout bem-sucedido (DEPRECATED - usar IAuthenticationService)
    /// </summary>
    public void SetOnLogout(Func<Task> onLogout)
    {
        // Este método mantém compatibilidade com código legado
        // A configuração real é feita via IAuthenticationService no AppRootViewModel
        Debug.WriteLine("[AUTH] SetOnLogout (deprecated) chamado - use IAuthenticationService.SetOnLogoutComplete");
    }

    [RelayCommand]
    public async Task LogoutAsync()
    {
        try
        {
            Debug.WriteLine("[AUTH] Logout iniciado");
            // Usa o IAuthenticationService para realizar logout
            await _authenticationService.PerformLogoutAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERRO] Erro ao fazer logout: {ex.Message}");
        }
    }
}