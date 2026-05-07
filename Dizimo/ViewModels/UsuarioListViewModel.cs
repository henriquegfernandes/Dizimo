using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Domain.Entities;
using Dizimo.Application.Usuarios.Handlers;
using Dizimo.Application.Usuarios.Commands;
using Dizimo.Application.Usuarios.Queries;
using System.Collections.ObjectModel;

namespace Dizimo.ViewModels;

public partial class UsuarioListViewModel : ObservableObject, INavigationAware
{
    private readonly GetUsuarioHandlers _getHandlers;
    private readonly DeleteUsuarioHandler _deleteHandler;
    private readonly UpdateUsuarioHandler _updateHandler;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    // ...existing code...

    public List<Usuario> TodosUsuarios { get; private set; } = new List<Usuario>();

    private ObservableCollection<Usuario> _usuarios = new();
    public ObservableCollection<Usuario> Usuarios
    {
        get => _usuarios;
        private set => SetProperty(ref _usuarios, value);
    }

    private Usuario? _selectedUsuario;
    public Usuario? SelectedUsuario
    {
        get => _selectedUsuario;
        set => SetProperty(ref _selectedUsuario, value);
    }

    private string _filtroNome = string.Empty;
    public string FiltroNome
    {
        get => _filtroNome;
        set => SetProperty(ref _filtroNome, value);
    }

    private ObservableCollection<Usuario> _usuariosSelecionados = new();
    public ObservableCollection<Usuario> UsuariosSelecionados
    {
        get => _usuariosSelecionados;
        set
        {
            if (_usuariosSelecionados != null)
                _usuariosSelecionados.CollectionChanged -= UsuariosSelecionados_CollectionChanged;
            SetProperty(ref _usuariosSelecionados, value);
            if (_usuariosSelecionados != null)
                _usuariosSelecionados.CollectionChanged += UsuariosSelecionados_CollectionChanged;
            OnPropertyChanged(nameof(UsuariosSelecionados));
            OnPropertyChanged(nameof(UsuariosSelecionados.Count));
            OnPropertyChanged(nameof(TextoBotaoSelecao));
        }
    }

    public string TextoBotaoSelecao => UsuariosSelecionados.Count == Usuarios.Count && Usuarios.Count > 0 ? "Desselecionar Todos" : "Selecionar Todos";

    private void UsuariosSelecionados_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(UsuariosSelecionados));
        OnPropertyChanged(nameof(UsuariosSelecionados.Count));
    }

    public UsuarioListViewModel(
        GetUsuarioHandlers getHandlers,
        DeleteUsuarioHandler deleteHandler,
        UpdateUsuarioHandler updateHandler,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _getHandlers = getHandlers ?? throw new ArgumentNullException(nameof(getHandlers));
        _deleteHandler = deleteHandler ?? throw new ArgumentNullException(nameof(deleteHandler));
        _updateHandler = updateHandler ?? throw new ArgumentNullException(nameof(updateHandler));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    }

    [RelayCommand]
    public async Task CarregarUsuariosAsync()
    {
        var lista = await _getHandlers.Handle(new GetAllUsuariosQuery());
        TodosUsuarios = lista is List<Usuario> usuarioList ? usuarioList : lista.ToList();
        Usuarios = new ObservableCollection<Usuario>(TodosUsuarios);
    }

    [RelayCommand]
    public void AplicarFiltros()
    {
        IEnumerable<Usuario> filtrados = TodosUsuarios;

        if (!string.IsNullOrWhiteSpace(FiltroNome))
        {
            filtrados = filtrados.Where(u => u.Nome.Contains(FiltroNome, StringComparison.OrdinalIgnoreCase) ||
                                             u.Login.Contains(FiltroNome, StringComparison.OrdinalIgnoreCase));
        }

        var filteredList = filtrados is List<Usuario> usuarioList ? usuarioList : filtrados.ToList();
        Usuarios = new ObservableCollection<Usuario>(filteredList);
    }

    [RelayCommand]
    public void LimparFiltros()
    {
        FiltroNome = string.Empty;
        AplicarFiltros();
    }

    [RelayCommand]
    public async Task EditarUsuarioAsync(Usuario usuario)
    {
        try
        {
            if (usuario != null)
            {
                var parameters = new NavigationParameters();
                parameters.Add("id", usuario.Id);
                _navigationService.Navigate("usuario-cadastro", parameters);
                System.Diagnostics.Debug.WriteLine($"[NAV] Editando usuário: {usuario.Id}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao editar usuário: {ex.Message}");
        }
        await Task.CompletedTask;
    }

    [RelayCommand]
    public async Task NovoUsuarioAsync()
    {
        try
        {
            _navigationService.Navigate("usuario-cadastro");
            System.Diagnostics.Debug.WriteLine("[NAV] Navegado para Novo Usuário");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao navegar para novo usuário: {ex.Message}");
        }
        await Task.CompletedTask;
    }

    [RelayCommand]
    public async Task ExcluirUsuarioAsync()
    {
        if (SelectedUsuario != null)
        {
            bool confirm = await _dialogService.ShowConfirmAsync(
                "Confirmação de Exclusão",
                $"Deseja realmente excluir o usuário {SelectedUsuario.Login}? Esta ação não pode ser desfeita.",
                "Sim", "Não");
            
            if (confirm)
            {
                try
                {
                    await _deleteHandler.Handle(new DeleteUsuarioCommand(SelectedUsuario.Id));
                    await CarregarUsuariosAsync();
                    SelectedUsuario = null;
                    await _dialogService.ShowSuccessAsync("Usuário excluído com sucesso!");
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowErrorAsync($"Erro ao excluir: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[ERRO] {ex}");
                }
            }
        }
    }

    [RelayCommand]
    public async Task ExcluirUsuariosSelecionadosAsync()
    {
        if (UsuariosSelecionados.Count == 0) return;
        
        bool confirm = await _dialogService.ShowConfirmAsync(
            "Confirmação de Exclusão",
            $"Deseja excluir {UsuariosSelecionados.Count} usuário(s)? Esta ação não pode ser desfeita.",
            "Sim", "Não");
        
        if (!confirm) return;

        try
        {
            foreach (var usuario in UsuariosSelecionados.ToList())
            {
                await _deleteHandler.Handle(new DeleteUsuarioCommand(usuario.Id));
            }
            await CarregarUsuariosAsync();
            UsuariosSelecionados.Clear();
            await _dialogService.ShowSuccessAsync("Usuários excluídos com sucesso!");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Erro ao excluir: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ERRO] {ex}");
        }
    }

    [RelayCommand]
    public async Task InativarUsuarioAsync()
    {
        if (SelectedUsuario != null)
        {
            string status = SelectedUsuario.Ativo ? "inativar" : "ativar";
            bool confirm = await _dialogService.ShowConfirmAsync(
                "Confirmação",
                $"Deseja {status} o usuário {SelectedUsuario.Login}?",
                "Sim", "Não");
            
            if (confirm)
            {
                try
                {
                    // Buscar usuário atualizado do banco para ter dados completos
                    var usuarioAtualizado = TodosUsuarios.FirstOrDefault(u => u.Id == SelectedUsuario.Id);
                    
                    if (usuarioAtualizado is not null)
                    {
                        // Fazer toggle do status
                        usuarioAtualizado.Ativo = !usuarioAtualizado.Ativo;
                        
                        // Chamar UpdateUsuarioHandler
                        var comando = new UpdateUsuarioCommand(
                            usuarioAtualizado.Id,
                            usuarioAtualizado.Nome,
                            usuarioAtualizado.Login,
                            string.Empty, // Senha vazia mantém a existente
                            usuarioAtualizado.Ativo,
                            usuarioAtualizado.Perfil);
                        
                        await _updateHandler.Handle(comando);
                    }
                    
                    await CarregarUsuariosAsync();
                    SelectedUsuario = null;
                    await _dialogService.ShowSuccessAsync($"Usuário {status}do com sucesso!");
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowErrorAsync($"Erro ao {status}: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[ERRO] {ex}");
                }
            }
        }
    }

     [RelayCommand]
    public async Task InativarUsuariosSelecionadosAsync()
    {
        if (UsuariosSelecionados.Count == 0) return;
        
        bool confirm = await _dialogService.ShowConfirmAsync(
            "Confirmação",
            $"Deseja inativar/ativar {UsuariosSelecionados.Count} usuário(s)?",
            "Sim", "Não");
        
        if (!confirm) return;

        try
        {
            // Executar o toggle para cada usuário selecionado
            foreach (var usuarioSelecionado in UsuariosSelecionados.ToList())
            {
                // Buscar usuário atualizado da lista
                var usuarioAtualizado = TodosUsuarios.FirstOrDefault(u => u.Id == usuarioSelecionado.Id);
                
                if (usuarioAtualizado is not null)
                {
                    // Fazer toggle do status
                    usuarioAtualizado.Ativo = !usuarioAtualizado.Ativo;
                    
                    // Chamar UpdateUsuarioHandler
                    var comando = new UpdateUsuarioCommand(
                        usuarioAtualizado.Id,
                        usuarioAtualizado.Nome,
                        usuarioAtualizado.Login,
                        string.Empty, // Senha vazia mantém a existente
                        usuarioAtualizado.Ativo,
                        usuarioAtualizado.Perfil);
                    
                    await _updateHandler.Handle(comando);
                }
            }
            
            // Recarregar dados APÓS fazer todos os toggles
            await CarregarUsuariosAsync();
            UsuariosSelecionados.Clear();
            await _dialogService.ShowSuccessAsync("Usuários ativados/inativados com sucesso!");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Erro ao ativar/inativar: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ERRO] {ex}");
        }
    }

    [RelayCommand]
    public void SelecionarTodos()
    {
        if (UsuariosSelecionados.Count == Usuarios.Count)
        {
            UsuariosSelecionados.Clear();
        }
        else
        {
            UsuariosSelecionados.Clear();
            foreach (var usuario in Usuarios)
            {
                UsuariosSelecionados.Add(usuario);
            }
        }
    }

    [RelayCommand]
    public void AplicarFiltrosEnter()
    {
        AplicarFiltrosCommand.Execute(null);
    }

    public void OnNavigatedTo(NavigationParameters parameters)
    {
        _ = CarregarUsuariosAsync();
    }

    public void OnNavigatedFrom()
    {
    }
}
