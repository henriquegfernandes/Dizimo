using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Services;

namespace Dizimo.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IDialogService _dialogService;

    private string _login = string.Empty;
    public string Login
    {
        get => _login;
        set => SetProperty(ref _login, value);
    }

    private string _senha = string.Empty;
    public string Senha
    {
        get => _senha;
        set => SetProperty(ref _senha, value);
    }

    private bool _isLoginFailed;
    public bool IsLoginFailed
    {
        get => _isLoginFailed; 
        set => SetProperty(ref _isLoginFailed, value);
    }

    public LoginViewModel(IAuthenticationService authenticationService, IDialogService? dialogService = null)
    {
        _authenticationService = authenticationService;
        _dialogService = dialogService ?? new DialogService();
        ResetLoginState();
    }

    public void ResetLoginState()
    {
        Login = string.Empty;
        Senha = string.Empty;
        IsLoginFailed = false;
    }

    /// <summary>
    /// Define o callback a ser executado após login bem-sucedido (DEPRECATED - usar IAuthenticationService)
    /// </summary>
    public void SetOnLoginSuccess(Func<Task> onLoginSuccess)
    {
        // Este método mantém compatibilidade com código legado
        // A configuração real é feita via IAuthenticationService no AppRootViewModel
        System.Diagnostics.Debug.WriteLine("[AUTH] SetOnLoginSuccess (deprecated) chamado - use IAuthenticationService.SetOnLoginSuccess");
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        try
        {
            // Usa o IAuthenticationService para realizar login
            var loginSuccess = await _authenticationService.PerformLoginAsync(Login, Senha);
            
            if (loginSuccess)
            {
                // Login bem-sucedido - o callback será executado pelo serviço
                System.Diagnostics.Debug.WriteLine($"[AUTH] Login bem-sucedido via LoginViewModel");
                IsLoginFailed = false;
            }
            else
            {
                // Login falhou - mostrar erro
                IsLoginFailed = true;
                await _dialogService.ShowErrorAsync("Login ou senha inválidos.");
                System.Diagnostics.Debug.WriteLine($"[AUTH] Login falhou - Usuário inválido ou inativo");
            }
        }
        catch (Exception ex)
        {
            IsLoginFailed = true;
            await _dialogService.ShowErrorAsync($"Erro ao fazer login: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ERRO] Exceção no login: {ex.Message}");
        }
    }
}
