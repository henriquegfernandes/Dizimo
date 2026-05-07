using System.Windows.Input;
using Dizimo.ViewModels;
using Dizimo.Services;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Dizimo.ViewModels;

public partial class ShellViewModel : ObservableObject, IDisposable
{
    private readonly MainViewModel _mainVm;
    private readonly LocalBackupViewModel _backupVm;
    private readonly MainPageViewModel _mainPageVm;
    private readonly INavigationService _navigationService;
    private Func<Task>? _onLogout;
    private bool _disposed;

    public MainViewModel MainVm => _mainVm;
    public LocalBackupViewModel BackupVm => _backupVm;
    public MainPageViewModel MainPageVm => _mainPageVm;

    private bool _isAdmin;
    public bool IsAdmin
    {
        get => _isAdmin;
        set => SetProperty(ref _isAdmin, value);
    }

    private string _usuarioNome = string.Empty;
    public string UsuarioNome
    {
        get => _usuarioNome;
        set => SetProperty(ref _usuarioNome, value);
    }

    private string _usuarioPerfil = string.Empty;
    public string UsuarioPerfil
    {
        get => _usuarioPerfil;
        set => SetProperty(ref _usuarioPerfil, value);
    }

    private object? _currentPage;
    public object? CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }

    private bool _isMenuExpanded = true;
    public bool IsMenuExpanded
    {
        get => _isMenuExpanded;
        set => SetProperty(ref _isMenuExpanded, value);
    }

    private AppTheme _selectedTheme = AppTheme.Light;
    public AppTheme SelectedTheme
    {
        get => _selectedTheme;
        set 
        { 
            if (SetProperty(ref _selectedTheme, value))
            {
                ThemeService.ApplyTheme(value);
            }
        }
    }

    private bool _isDarkTheme = false;
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

    public ShellViewModel(
        MainViewModel mainVm, 
        LocalBackupViewModel backupVm, 
        MainPageViewModel mainPageVm,
        INavigationService navigationService)
    {
        _mainVm = mainVm;
        _backupVm = backupVm;
        _mainPageVm = mainPageVm;
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

    private void OnNavigationChanged(object? sender, string route)
    {
        // Quando o NavigationService navega, atualiza o CurrentPage
        if (_navigationService.CurrentContent is not null)
        {
            CurrentPage = _navigationService.CurrentContent;
            System.Diagnostics.Debug.WriteLine($"[NAV] CurrentPage atualizado para rota: {route}");
        }
    }

    private void UpdateValues()
    {
        IsAdmin = MainViewModel.IsAdminStatic;
        UsuarioNome = MainViewModel.UsuarioNomeStatic;
        UsuarioPerfil = MainViewModel.UsuarioPerfilStatic;
    }

    /// <summary>
    /// Versão pública de UpdateValues para ser chamada após autenticação
    /// </summary>
    public void UpdateValuesPublic()
    {
        UpdateValues();
        System.Diagnostics.Debug.WriteLine($"[AUTH] ShellViewModel atualizado: IsAdmin={IsAdmin}, Usuario={UsuarioNome} ({UsuarioPerfil})");
    }

    /// <summary>
    /// Define o callback a ser executado após logout (DEPRECATED)
    /// A configuração real agora é feita via IAuthenticationService no AppRootViewModel
    /// </summary>
    public void SetOnLogout(Func<Task> onLogout)
    {
        _onLogout = onLogout;
        _mainVm.SetOnLogout(onLogout);
        _backupVm.SetOnRestoreSuccess(onLogout);
        System.Diagnostics.Debug.WriteLine("[AUTH] SetOnLogout (deprecated) chamado no ShellViewModel");
    }

    /// <summary>
    /// Configura o AppRootViewModel para o LocalBackupViewModel (DEPRECATED)
    /// Não mais necessário com o novo IAuthenticationService
    /// </summary>
    public void SetAppRootViewModel(AppRootViewModel appRootViewModel)
    {
        System.Diagnostics.Debug.WriteLine("[AUTH] SetAppRootViewModel (deprecated) chamado - use IAuthenticationService");
    }

    [RelayCommand]
    public void ToggleMenu()
    {
        IsMenuExpanded = !IsMenuExpanded;
        System.Diagnostics.Debug.WriteLine($"[NAV] Menu toggled - IsMenuExpanded: {IsMenuExpanded}");
    }

    [RelayCommand]
    public void ToggleTheme()
    {
        IsDarkTheme = !IsDarkTheme;
        System.Diagnostics.Debug.WriteLine($"[TEMA] Tema alternado - IsDarkTheme: {IsDarkTheme}");
    }

    [RelayCommand]
    public async Task NavigateToDashboardAsync()
    {
        try
        {
            _navigationService.Navigate("main");
            System.Diagnostics.Debug.WriteLine("[NAV] Navegado para Dashboard via NavigationService");
            // Dispara o carregamento de dados de forma assíncrona
            _ = _mainPageVm.CarregarDadosCommand?.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao carregar dashboard: {ex.Message}");
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
            System.Diagnostics.Debug.WriteLine("[NAV] Navegado para Dizimistas via NavigationService (cache limpo)");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao navegar para Dizimistas: {ex.Message}");
        }
    }

    [RelayCommand]
    public void NavigateToNovoDizimista()
    {
        try
        {
            _navigationService.Navigate("dizimista-cadastro");
            System.Diagnostics.Debug.WriteLine("[NAV] Navegado para Novo Dizimista via NavigationService");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao navegar para novo dizimista: {ex.Message}");
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
            System.Diagnostics.Debug.WriteLine("[NAV] Navegado para Ofertas via NavigationService (cache limpo)");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao navegar para Ofertas: {ex.Message}");
        }
    }

    [RelayCommand]
    public void NavigateToNovaOferta()
    {
        try
        {
            _navigationService.Navigate("oferta-cadastro");
            System.Diagnostics.Debug.WriteLine("[NAV] Navegado para Nova Oferta via NavigationService");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao navegar para nova oferta: {ex.Message}");
        }
    }

    [RelayCommand]
    public void NavigateToUsuarios()
    {
        try
        {
            _navigationService.Navigate("usuarios");
            System.Diagnostics.Debug.WriteLine("[NAV] Navegado para Usuários via NavigationService");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao navegar para usuários: {ex.Message}");
        }
    }

    [RelayCommand]
    public void NavigateToBackup()
    {
        try
        {
            _navigationService.Navigate("backupconfig");
            System.Diagnostics.Debug.WriteLine("[NAV] Navegado para Backup via NavigationService");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao navegar para backup: {ex.Message}");
        }
    }

    public ICommand LogoutCommand => _mainVm.LogoutCommand;
    public ICommand EscolherPastaCommand => _backupVm.EscolherPastaCommand;
    public ICommand BackupCommand => _backupVm.BackupCommand;
    public ICommand RestoreCommand => _backupVm.RestoreCommand;
    public string BackupFolderPath => _backupVm.BackupFolderPath ?? string.Empty;

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _navigationService.NavigationChanged -= OnNavigationChanged;
            GC.SuppressFinalize(this);
        }
    }
}
