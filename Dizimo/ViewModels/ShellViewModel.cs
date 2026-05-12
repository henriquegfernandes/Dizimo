using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Dizimo.ViewModels;

public partial class ShellViewModel : ObservableObject, IDisposable
{
    private readonly INavigationService _navigationService;

    private object? _currentPage;
    private bool _disposed;

    private bool _isAdmin;

    private bool _isDarkTheme;

    private bool _isMenuExpanded = true;
    private Func<Task>? _onLogout;

    private AppTheme _selectedTheme = AppTheme.Light;

    private string _usuarioNome = string.Empty;

    private string _usuarioPerfil = string.Empty;

    public ShellViewModel(
        MainViewModel mainVm,
        LocalBackupViewModel backupVm,
        MainPageViewModel mainPageVm,
        INavigationService navigationService)
    {
        MainVm = mainVm;
        BackupVm = backupVm;
        MainPageVm = mainPageVm;
        _navigationService = navigationService;

        // Ouve eventos de navegação do NavigationService
        _navigationService.NavigationChanged += OnNavigationChanged;

        // Carrega o tema salvo
        var savedTheme = ThemeService.GetSavedThemePreference();
        _isDarkTheme = savedTheme == AppTheme.Dark;
        _selectedTheme = savedTheme;

        UpdateValues();

        // Carrega dashboard por padrão
        _ = NavigateToDashboardAsync();
    }

    public MainViewModel MainVm { get; }

    public LocalBackupViewModel BackupVm { get; }

    public MainPageViewModel MainPageVm { get; }

    public bool IsAdmin
    {
        get => _isAdmin;
        set => SetProperty(ref _isAdmin, value);
    }

    public string UsuarioNome
    {
        get => _usuarioNome;
        set => SetProperty(ref _usuarioNome, value);
    }

    public string UsuarioPerfil
    {
        get => _usuarioPerfil;
        set => SetProperty(ref _usuarioPerfil, value);
    }

    public object? CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }

    public bool IsMenuExpanded
    {
        get => _isMenuExpanded;
        set => SetProperty(ref _isMenuExpanded, value);
    }

    public AppTheme SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (SetProperty(ref _selectedTheme, value)) ThemeService.ApplyTheme(value);
        }
    }

    public bool IsDarkTheme
    {
        get => _isDarkTheme;
        set
        {
            if (SetProperty(ref _isDarkTheme, value))
            {
                var newTheme = value ? AppTheme.Dark : AppTheme.Light;
                SelectedTheme = newTheme;
                ThemeService.SaveThemePreference(newTheme);
            }
        }
    }

    public ICommand LogoutCommand => MainVm.LogoutCommand;
    public ICommand EscolherPastaCommand => BackupVm.EscolherPastaCommand;
    public ICommand BackupCommand => BackupVm.BackupCommand;
    public ICommand RestoreCommand => BackupVm.RestoreCommand;
    public string BackupFolderPath => BackupVm.BackupFolderPath ?? string.Empty;

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _navigationService.NavigationChanged -= OnNavigationChanged;
            GC.SuppressFinalize(this);
        }
    }

    private void OnNavigationChanged(object? sender, string route)
    {
        // Quando o NavigationService navega, atualiza o CurrentPage
        if (_navigationService.CurrentContent is not null)
        {
            CurrentPage = _navigationService.CurrentContent;
            Debug.WriteLine($"[NAV] CurrentPage atualizado para rota: {route}");
        }
    }

    private void UpdateValues()
    {
        IsAdmin = MainViewModel.IsAdminStatic;
        UsuarioNome = MainViewModel.UsuarioNomeStatic;
        UsuarioPerfil = MainViewModel.UsuarioPerfilStatic;
    }

    /// <summary>
    ///     Versão pública de UpdateValues para ser chamada após autenticação
    /// </summary>
    public void UpdateValuesPublic()
    {
        UpdateValues();
        Debug.WriteLine(
            $"[AUTH] ShellViewModel atualizado: IsAdmin={IsAdmin}, Usuario={UsuarioNome} ({UsuarioPerfil})");
    }

    /// <summary>
    ///     Define o callback a ser executado após logout (DEPRECATED)
    ///     A configuração real agora é feita via IAuthenticationService no AppRootViewModel
    /// </summary>
    public void SetOnLogout(Func<Task> onLogout)
    {
        _onLogout = onLogout;
        MainVm.SetOnLogout(onLogout);
        BackupVm.SetOnRestoreSuccess(onLogout);
        Debug.WriteLine("[AUTH] SetOnLogout (deprecated) chamado no ShellViewModel");
    }

    /// <summary>
    ///     Configura o AppRootViewModel para o LocalBackupViewModel (DEPRECATED)
    ///     Não mais necessário com o novo IAuthenticationService
    /// </summary>
    public void SetAppRootViewModel(AppRootViewModel appRootViewModel)
    {
        Debug.WriteLine("[AUTH] SetAppRootViewModel (deprecated) chamado - use IAuthenticationService");
    }

    [RelayCommand]
    public void ToggleMenu()
    {
        IsMenuExpanded = !IsMenuExpanded;
        Debug.WriteLine($"[NAV] Menu toggled - IsMenuExpanded: {IsMenuExpanded}");
    }

    [RelayCommand]
    public void ToggleTheme()
    {
        IsDarkTheme = !IsDarkTheme;
        Debug.WriteLine($"[TEMA] Tema alternado - IsDarkTheme: {IsDarkTheme}");
    }

    [RelayCommand]
    public async Task NavigateToDashboardAsync()
    {
        try
        {
            _navigationService.Navigate("main");
            Debug.WriteLine("[NAV] Navegado para Dashboard via NavigationService");
            // Dispara o carregamento de dados de forma assíncrona
            _ = MainPageVm.CarregarDadosCommand?.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERRO] Erro ao carregar dashboard: {ex.Message}");
        }
    }

    [RelayCommand]
    public void NavigateToDizimistas()
    {
        try
        {
            var parameters = new NavigationParameters();
            parameters.Add("clearCache", true);
            _navigationService.Navigate("dizimistas", parameters);
            Debug.WriteLine("[NAV] Navegado para Dizimistas via NavigationService (cache limpo)");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERRO] Erro ao navegar para Dizimistas: {ex.Message}");
        }
    }

    [RelayCommand]
    public void NavigateToNovoDizimista()
    {
        try
        {
            _navigationService.Navigate("dizimista-cadastro");
            Debug.WriteLine("[NAV] Navegado para Novo Dizimista via NavigationService");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERRO] Erro ao navegar para novo dizimista: {ex.Message}");
        }
    }

    [RelayCommand]
    public void NavigateToOfertas()
    {
        try
        {
            var parameters = new NavigationParameters();
            parameters.Add("clearCache", true);
            _navigationService.Navigate("ofertas", parameters);
            Debug.WriteLine("[NAV] Navegado para Ofertas via NavigationService (cache limpo)");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERRO] Erro ao navegar para Ofertas: {ex.Message}");
        }
    }

    [RelayCommand]
    public void NavigateToNovaOferta()
    {
        try
        {
            _navigationService.Navigate("oferta-cadastro");
            Debug.WriteLine("[NAV] Navegado para Nova Oferta via NavigationService");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERRO] Erro ao navegar para nova oferta: {ex.Message}");
        }
    }

    [RelayCommand]
    public void NavigateToUsuarios()
    {
        try
        {
            _navigationService.Navigate("usuarios");
            Debug.WriteLine("[NAV] Navegado para Usuários via NavigationService");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERRO] Erro ao navegar para usuários: {ex.Message}");
        }
    }

    [RelayCommand]
    public void NavigateToBackup()
    {
        try
        {
            _navigationService.Navigate("backupconfig");
            Debug.WriteLine("[NAV] Navegado para Backup via NavigationService");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERRO] Erro ao navegar para backup: {ex.Message}");
        }
    }
}