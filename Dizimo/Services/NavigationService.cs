using Dizimo.ViewModels;

namespace Dizimo.Services
{
    /// <summary>
    /// Modelo para parâmetros de navegação em Avalonia
    /// </summary>
    public class NavigationParameters
    {
        private readonly Dictionary<string, object?> _parameters = new();

        public void Add(string key, object? value) => _parameters[key] = value;
        public T? Get<T>(string key) where T : class => _parameters.TryGetValue(key, out var value) ? value as T : null;
        public bool TryGetValue(string key, out object? value) => _parameters.TryGetValue(key, out value);
        public void Clear() => _parameters.Clear();
    }

    /// <summary>
    /// Interface de serviço de navegação otimizado para Avalonia
    /// </summary>
    public partial interface INavigationService
    {
        event EventHandler<NavigationParameters>? ParametersChanged;
        event EventHandler<string>? NavigationChanged;
        
        void Navigate(string route, NavigationParameters? parameters = null);
        void Navigate<TViewModel>(string route, NavigationParameters? parameters = null) where TViewModel : class;
        bool CanGoBack { get; }
        void GoBack();
        
        string CurrentRoute { get; }
        object? CurrentContent { get; }
        NavigationParameters CurrentParameters { get; }
    }

    /// <summary>
    /// Serviço de navegação otimizado para Avalonia com suporte a histórico e parâmetros
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Type> _routeMap = new();
        private readonly Stack<(string route, NavigationParameters parameters)> _navigationHistory = new();
        private readonly object _navigationLock = new();
        
        private string _currentRoute = "main";
        private object? _currentContent;
        private NavigationParameters _currentParameters = new();

        public event EventHandler<string>? NavigationChanged;
        public event EventHandler<NavigationParameters>? ParametersChanged;

        public string CurrentRoute 
        { 
            get 
            { 
                lock (_navigationLock) 
                { 
                    return _currentRoute; 
                } 
            } 
        }
        
        public object? CurrentContent 
        { 
            get 
            { 
                lock (_navigationLock) 
                { 
                    return _currentContent; 
                } 
            } 
        }
        
        public NavigationParameters CurrentParameters 
        { 
            get 
            { 
                lock (_navigationLock) 
                { 
                    return _currentParameters; 
                } 
            } 
        }
        
        public bool CanGoBack 
        { 
            get 
            { 
                lock (_navigationLock) 
                { 
                    return _navigationHistory.Count > 0; 
                } 
            } 
        }

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            RegisterRoutes();
        }

        private void RegisterRoutes()
        {
            lock (_navigationLock)
            {
                _routeMap["main"] = typeof(MainPageViewModel);
                _routeMap["login"] = typeof(LoginViewModel);
                _routeMap["setup"] = typeof(SetupViewModel);
                _routeMap["dizimistas"] = typeof(DizimistaListViewModel);
                _routeMap["dizimista-cadastro"] = typeof(DizimistaCadastroViewModel);
                _routeMap["dizimista-detalhes"] = typeof(DizimistaDetalhesViewModel);
                _routeMap["ofertas"] = typeof(OfertaListViewModel);
                _routeMap["oferta-cadastro"] = typeof(OfertaCadastroViewModel);
                _routeMap["usuarios"] = typeof(UsuarioListViewModel);
                _routeMap["usuario-cadastro"] = typeof(UsuarioCadastroViewModel);
                _routeMap["backupconfig"] = typeof(LocalBackupViewModel);
            }
        }

        public void Navigate(string route, NavigationParameters? parameters = null)
        {
            var baseRoute = ExtractBaseRoute(route);
            System.Diagnostics.Debug.WriteLine($"[NAV] Navigate chamado com rota: {route} (rota base: {baseRoute})");
            if (_routeMap.TryGetValue(baseRoute, out _))
            {
                NavigateInternal(route, parameters ?? new NavigationParameters(), true);
            }
            else
            {
                LogError($"Rota não mapeada: {route}");
            }
        }

        public void Navigate<TViewModel>(string route, NavigationParameters? parameters = null) where TViewModel : class
        {
            NavigateInternal(route, parameters ?? new NavigationParameters(), true);
        }

        public void GoBack()
        {
            lock (_navigationLock)
            {
                System.Diagnostics.Debug.WriteLine($"[NAV] GoBack chamado. Histórico atual tem {_navigationHistory.Count} entradas");
                
                if (_navigationHistory.Count > 0)
                {
                    var (previousRoute, previousParams) = _navigationHistory.Pop();
                    System.Diagnostics.Debug.WriteLine($"[NAV] Voltando para rota: {previousRoute} (histórico agora tem {_navigationHistory.Count} entradas)");
                    NavigateInternal(previousRoute, previousParams, false);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[NAV] Nenhuma entrada no histórico para voltar!");
                }
            }
        }

        private void NavigateInternal(string route, NavigationParameters parameters, bool saveToHistory)
        {
            lock (_navigationLock)
            {
                try
                {
                    var baseRoute = ExtractBaseRoute(route);
                    
                    if (!_routeMap.TryGetValue(baseRoute, out var viewModelType))
                    {
                        LogError($"Tipo de ViewModel não encontrado para rota: {route}");
                        return;
                    }

                    // Chamar OnNavigatedFrom do ViewModel atual ANTES de navegar
                    if (_currentContent is INavigationAware navigationAwareFrom && route != _currentRoute)
                    {
                        navigationAwareFrom.OnNavigatedFrom();
                        System.Diagnostics.Debug.WriteLine($"[NAV] OnNavigatedFrom chamado para ViewModel atual antes de navegar");
                    }

                    // Salvar estado atual no histórico se a rota for diferente
                    // MAS remover parâmetros especiais como "clearCache" que não devem ser restaurados
                    if (saveToHistory && route != _currentRoute)
                    {
                        var parametersForHistory = new NavigationParameters();
                        // Copiar todos os parâmetros EXCETO os especiais
                        if (_currentParameters.TryGetValue("id", out var id))
                        {
                            parametersForHistory.Add("id", id);
                        }
                        // Não salvar "clearCache" no histórico!
                        
                        _navigationHistory.Push((_currentRoute, parametersForHistory));
                        System.Diagnostics.Debug.WriteLine($"[NAV] Histórico atualizado: adicionada rota '{_currentRoute}' -> navegando para '{route}' (histórico agora tem {_navigationHistory.Count} entradas)");
                    }
                    else if (saveToHistory && route == _currentRoute)
                    {
                        System.Diagnostics.Debug.WriteLine($"[NAV] Navegação para a mesma rota '{route}', histórico não foi atualizado");
                    }

                    var viewModel = _serviceProvider.GetService(viewModelType);
                    if (viewModel == null)
                    {
                        LogError($"Falha ao resolver ViewModel: {viewModelType.Name}");
                        return;
                    }

                    // Injetar parâmetros se o ViewModel suportar
                    if (viewModel is INavigationAware navigationAware)
                    {
                        navigationAware.OnNavigatedTo(parameters);
                    }

                    _currentContent = viewModel;
                    _currentRoute = route;
                    _currentParameters = parameters;

                    NavigationChanged?.Invoke(this, route);
                    ParametersChanged?.Invoke(this, parameters);
                    System.Diagnostics.Debug.WriteLine($"[NAV] Navegação concluída para: {route}");
                }
                catch (Exception ex)
                {
                    LogError($"Erro ao navegar para {route}: {ex.Message}");
                }
            }
        }

        private void LogError(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[NavigationService] {message}");
        }

        private string ExtractBaseRoute(string route)
        {
            // Extrair rota base removendo parâmetros (ex: "dizimista-detalhes/123" -> "dizimista-detalhes")
            var parts = route.Split('/');
            return parts[0];
        }

        // Implementação de métodos async se definidos na interface parcial
        public async Task NavigateToAsync(string route, bool isAbsoluteRoute = true)
        {
            Navigate(route);
            await Task.CompletedTask;
        }

        public async Task NavigateBackAsync(bool useModalNavigation = true)
        {
            GoBack();
            await Task.CompletedTask;
        }

        public async Task NavigateToRootAsync(string route, bool isAbsoluteRoute = true)
        {
            _navigationHistory.Clear();
            Navigate(route);
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Interface para ViewModels que precisam reagir a eventos de navegação
    /// </summary>
    public interface INavigationAware
    {
        void OnNavigatedTo(NavigationParameters parameters);
        void OnNavigatedFrom();
    }
}

