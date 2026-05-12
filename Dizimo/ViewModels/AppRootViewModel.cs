using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Dizimo.Infrastructure.Persistence;

namespace Dizimo.ViewModels;

/// <summary>
///     ViewModel raiz que controla o fluxo da aplicação
///     Determina se mostra Login, Setup ou Dashboard
/// </summary>
public partial class AppRootViewModel : ObservableObject
{
    private readonly IAuthenticationService _authenticationService;
    private readonly DizimoDbContext _dbContext;
    private readonly LoginViewModel _loginViewModel;
    private readonly INavigationService _navigationService;
    private readonly SetupViewModel _setupViewModel;
    private readonly ShellViewModel _shellViewModel;

    [ObservableProperty] private object? _currentView;

    public AppRootViewModel(
        INavigationService navigationService,
        DizimoDbContext dbContext,
        LoginViewModel loginViewModel,
        SetupViewModel setupViewModel,
        ShellViewModel shellViewModel,
        IAuthenticationService authenticationService)
    {
        _navigationService = navigationService;
        _dbContext = dbContext;
        _loginViewModel = loginViewModel;
        _setupViewModel = setupViewModel;
        _shellViewModel = shellViewModel;
        _authenticationService = authenticationService;

        // Configura os ViewModels com a referência para navegação
        ConfigureViewModels();

        // Determina qual view deve ser exibido inicialmente
        DetermineInitialView();
    }

    /// <summary>
    ///     Configura os ViewModels filhos com referência para o AppRootViewModel
    ///     Necessário para que eles possam acionar navegações
    /// </summary>
    private void ConfigureViewModels()
    {
        // Configura callbacks de navegação para LoginViewModel
        _loginViewModel.SetOnLoginSuccess(NavigateToDashboardAsync);

        // Configura callbacks de navegação para SetupViewModel
        _setupViewModel.SetOnSetupComplete(NavigateToLoginAsync);

        // Configura o AuthenticationService com callbacks de login e logout
        _authenticationService.SetOnLoginSuccess(NavigateToDashboardAsync);
        _authenticationService.SetOnLogoutComplete(NavigateToLoginAsync);

        Debug.WriteLine("[NAV] ViewModels configurados com callbacks de navegação");
    }

    private void DetermineInitialView()
    {
        try
        {
            Debug.WriteLine("[NAV] Iniciando determinação da view inicial...");

            // Verifica o tema carregado
            var currentTheme = ThemeService.GetSavedThemePreference();
            Debug.WriteLine($"[TEMA] Tema carregado: {currentTheme}");

            // Verifica se há usuários cadastrados no banco
            var usuariosCount = _dbContext.Usuarios.Count();
            Debug.WriteLine($"[NAV] Total de usuários no banco: {usuariosCount}");

            if (usuariosCount == 0)
            {
                // Se não há usuários, exibe tela de Setup
                CurrentView = _setupViewModel;
                Debug.WriteLine("[NAV] ✓ Nenhum usuário cadastrado - exibindo SetupPage");
            }
            else if (SessaoService.IsLogado)
            {
                // Se usuário já está logado, exibe Dashboard

                _shellViewModel.UpdateValuesPublic();
                CurrentView = _shellViewModel;
                Debug.WriteLine("[NAV] ✓ Usuário logado - exibindo Shell (Dashboard)");
            }
            else
            {
                // Caso padrão: exibe tela de Login
                CurrentView = _loginViewModel;
                Debug.WriteLine("[NAV] ✓ Exibindo LoginPage");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERRO] Erro ao determinar view inicial: {ex.Message}");
            Debug.WriteLine($"[ERRO] StackTrace: {ex.StackTrace}");
            // Fallback para Login em caso de erro
            CurrentView = _loginViewModel;
        }
    }

    /// <summary>
    ///     Navega para a página de login
    ///     Chamado após setup bem-sucedido ou quando usuário faz logout
    /// </summary>
    public void NavigateToLogin()
    {
        _loginViewModel.ResetLoginState();
        CurrentView = _loginViewModel;
        Debug.WriteLine("[NAV] ✓ Navegado para LoginPage");
    }

    /// <summary>
    ///     Navega para a página de login (async)
    /// </summary>
    public async Task NavigateToLoginAsync()
    {
        await Task.Run(() => NavigateToLogin());
    }

    /// <summary>
    ///     Navega para o dashboard
    ///     Chamado após login bem-sucedido
    /// </summary>
    public void NavigateToDashboard()
    {
        // Atualiza os dados do usuário no ShellViewModel ANTES de exibir
        _shellViewModel.UpdateValuesPublic();

        CurrentView = _shellViewModel;
        Debug.WriteLine("[NAV] ✓ Navegado para Dashboard (ShellViewModel)");
    }

    /// <summary>
    ///     Navega para o dashboard (async)
    /// </summary>
    public async Task NavigateToDashboardAsync()
    {
        await Task.Run(() => NavigateToDashboard());
    }

    /// <summary>
    ///     Navega para a página de setup
    /// </summary>
    public void NavigateToSetup()
    {
        CurrentView = _setupViewModel;
        Debug.WriteLine("[NAV] ✓ Navegado para SetupPage");
    }
}