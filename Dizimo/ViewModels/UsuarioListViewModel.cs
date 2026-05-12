using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Application.Usuarios.Commands;
using Dizimo.Application.Usuarios.Handlers;
using Dizimo.Application.Usuarios.Queries;
using Dizimo.Domain.Entities;

namespace Dizimo.ViewModels;

public partial class UsuarioListViewModel : ObservableObject, INavigationAware
{
    private readonly DeleteUsuarioHandler _deleteHandler;
    private readonly IDialogService _dialogService;
    private readonly GetUsuarioHandlers _getHandlers;
    private readonly INavigationService _navigationService;
    private readonly UpdateUsuarioHandler _updateHandler;

    private string _filtroNome = string.Empty;

    private Usuario? _selectedUsuario;

    private ObservableCollection<Usuario> _usuarios = new();

    private ObservableCollection<Usuario> _usuariosSelecionados = new();

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

    // ...existing code...

    public List<Usuario> TodosUsuarios { get; private set; } = new();

    public ObservableCollection<Usuario> Usuarios
    {
        get => _usuarios;
        private set => SetProperty(ref _usuarios, value);
    }

    public Usuario? SelectedUsuario
    {
        get => _selectedUsuario;
        set => SetProperty(ref _selectedUsuario, value);
    }

    public string FiltroNome
    {
        get => _filtroNome;
        set => SetProperty(ref _filtroNome, value);
    }

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
            OnPropertyChanged();
            OnPropertyChanged(nameof(UsuariosSelecionados.Count));
            OnPropertyChanged(nameof(TextoBotaoSelecao));
        }
    }

    public string TextoBotaoSelecao => UsuariosSelecionados.Count == Usuarios.Count && Usuarios.Count > 0
        ? "Desselecionar Todos"
        : "Selecionar Todos";

    public void OnNavigatedTo(NavigationParameters parameters)
    {
        _ = CarregarUsuariosAsync();
    }

    public void OnNavigatedFrom()
    {
    }

    private void UsuariosSelecionados_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(UsuariosSelecionados));
        OnPropertyChanged(nameof(UsuariosSelecionados.Count));
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
            filtrados = filtrados.Where(u => u.Nome.Contains(FiltroNome, StringComparison.OrdinalIgnoreCase) ||
                                             u.Login.Contains(FiltroNome, StringComparison.OrdinalIgnoreCase));

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
                Debug.WriteLine($"[NAV] Editando usuário: {usuario.Id}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERRO] Erro ao editar usuário: {ex.Message}");
        }

        await Task.CompletedTask;
    }

    [RelayCommand]
    public async Task NovoUsuarioAsync()
    {
        try
        {
            _navigationService.Navigate("usuario-cadastro");
            Debug.WriteLine("[NAV] Navegado para Novo Usuário");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERRO] Erro ao navegar para novo usuário: {ex.Message}");
        }

        await Task.CompletedTask;
    }

    [RelayCommand]
    public async Task ExcluirUsuarioAsync()
    {
        if (SelectedUsuario != null)
        {
            var confirm = await _dialogService.ShowConfirmAsync(
                "Confirmação de Exclusão",
                $"Deseja realmente excluir o usuário {SelectedUsuario.Login}? Esta ação não pode ser desfeita.");

            if (confirm)
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
                    Debug.WriteLine($"[ERRO] {ex}");
                }
        }
    }

    [RelayCommand]
    public async Task ExcluirUsuariosSelecionadosAsync()
    {
        if (UsuariosSelecionados.Count == 0) return;

        var confirm = await _dialogService.ShowConfirmAsync(
            "Confirmação de Exclusão",
            $"Deseja excluir {UsuariosSelecionados.Count} usuário(s)? Esta ação não pode ser desfeita.");

        if (!confirm) return;

        try
        {
            foreach (var usuario in UsuariosSelecionados.ToList())
                await _deleteHandler.Handle(new DeleteUsuarioCommand(usuario.Id));
            await CarregarUsuariosAsync();
            UsuariosSelecionados.Clear();
            await _dialogService.ShowSuccessAsync("Usuários excluídos com sucesso!");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Erro ao excluir: {ex.Message}");
            Debug.WriteLine($"[ERRO] {ex}");
        }
    }

    [RelayCommand]
    public async Task InativarUsuarioAsync()
    {
        if (SelectedUsuario != null)
        {
            var status = SelectedUsuario.Ativo ? "inativar" : "ativar";
            var confirm = await _dialogService.ShowConfirmAsync(
                "Confirmação",
                $"Deseja {status} o usuário {SelectedUsuario.Login}?");

            if (confirm)
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
                    Debug.WriteLine($"[ERRO] {ex}");
                }
        }
    }

    [RelayCommand]
    public async Task InativarUsuariosSelecionadosAsync()
    {
        if (UsuariosSelecionados.Count == 0) return;

        var confirm = await _dialogService.ShowConfirmAsync(
            "Confirmação",
            $"Deseja inativar/ativar {UsuariosSelecionados.Count} usuário(s)?");

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
            Debug.WriteLine($"[ERRO] {ex}");
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
            foreach (var usuario in Usuarios) UsuariosSelecionados.Add(usuario);
        }
    }

    [RelayCommand]
    public void AplicarFiltrosEnter()
    {
        AplicarFiltrosCommand.Execute(null);
    }
}