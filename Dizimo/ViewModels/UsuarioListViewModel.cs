using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Domain.Entities;
using Dizimo.Application.Usuarios.Handlers;
using Dizimo.Application.Usuarios.Commands;
using Dizimo.Application.Usuarios.Queries;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Dizimo.ViewModels;

public partial class UsuarioListViewModel : ObservableObject
{
    private readonly GetUsuarioHandlers _getHandlers;
    private readonly CreateUsuarioHandler _createHandler;
    private readonly UpdateUsuarioHandler _updateHandler;
    private readonly DeleteUsuarioHandler _deleteHandler;
    private readonly InativarUsuarioHandler _inativarHandler;

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
        }
    }

    private void UsuariosSelecionados_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(UsuariosSelecionados));
        OnPropertyChanged(nameof(UsuariosSelecionados.Count));
    }

    public UsuarioListViewModel(
        GetUsuarioHandlers getHandlers,
        CreateUsuarioHandler createHandler,
        UpdateUsuarioHandler updateHandler,
        DeleteUsuarioHandler deleteHandler,
        InativarUsuarioHandler inativarHandler)
    {
        _getHandlers = getHandlers ?? throw new ArgumentNullException(nameof(getHandlers));
        _createHandler = createHandler ?? throw new ArgumentNullException(nameof(createHandler));
        _updateHandler = updateHandler ?? throw new ArgumentNullException(nameof(updateHandler));
        _deleteHandler = deleteHandler ?? throw new ArgumentNullException(nameof(deleteHandler));
        _inativarHandler = inativarHandler ?? throw new ArgumentNullException(nameof(inativarHandler));
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
    public async Task ExcluirUsuarioAsync()
    {
        if (SelectedUsuario != null)
        {
            var mainPage = GetMainPage();
            if (mainPage != null)
            {
                bool confirm = await mainPage.DisplayAlertAsync("Confirmação", $"Deseja excluir o usuário '{SelectedUsuario.Nome}'?", "Sim", "Não");
                if (confirm)
                {
                    try
                    {
                        await _deleteHandler.Handle(new DeleteUsuarioCommand(SelectedUsuario.Id));
                        await CarregarUsuariosAsync();
                        SelectedUsuario = null;
                        await mainPage.DisplayAlertAsync("Sucesso", "Usuário excluído com sucesso.", "OK");
                    }
                    catch (Exception ex)
                    {
                        await mainPage.DisplayAlertAsync("Erro", $"Erro ao excluir: {ex.Message}", "OK");
                    }
                }
            }
        }
    }

    [RelayCommand]
    public async Task ExcluirUsuariosSelecionadosAsync()
    {
        if (UsuariosSelecionados.Count == 0) return;
        var mainPage = GetMainPage();
        if (mainPage != null)
        {
            bool confirm = await mainPage.DisplayAlertAsync("Confirmação", $"Deseja excluir {UsuariosSelecionados.Count} usuário(s)?", "Sim", "Não");
            if (!confirm) return;
        }
        foreach (var usuario in UsuariosSelecionados.ToList())
        {
            await _deleteHandler.Handle(new DeleteUsuarioCommand(usuario.Id));
        }
        await CarregarUsuariosAsync();
        UsuariosSelecionados.Clear();
    }

    [RelayCommand]
    public async Task InativarUsuarioAsync()
    {
        if (SelectedUsuario != null)
        {
            var mainPage = GetMainPage();
            if (mainPage != null)
            {
                string status = SelectedUsuario.Ativo ? "inativar" : "ativar";
                bool confirm = await mainPage.DisplayAlertAsync("Confirmação", $"Deseja {status} o usuário '{SelectedUsuario.Nome}'?", "Sim", "Não");
                if (confirm)
                {
                    try
                    {
                        await _inativarHandler.Handle(new InativarUsuarioCommand(SelectedUsuario.Id));
                        await CarregarUsuariosAsync();
                        SelectedUsuario = null;
                        await mainPage.DisplayAlertAsync("Sucesso", $"Usuário {status}o com sucesso.", "OK");
                    }
                    catch (Exception ex)
                    {
                        await mainPage.DisplayAlertAsync("Erro", $"Erro ao {status}ar: {ex.Message}", "OK");
                    }
                }
            }
        }
    }

    [RelayCommand]
    public async Task InativarUsuariosSelecionadosAsync()
    {
        if (UsuariosSelecionados.Count == 0) return;
        var mainPage = GetMainPage();
        if (mainPage != null)
        {
            bool confirm = await mainPage.DisplayAlertAsync("Confirmação", $"Deseja inativar/ativar {UsuariosSelecionados.Count} usuário(s)?", "Sim", "Não");
            if (!confirm) return;
        }
        foreach (var usuario in UsuariosSelecionados.ToList())
        {
            await _inativarHandler.Handle(new InativarUsuarioCommand(usuario.Id));
        }
        await CarregarUsuariosAsync();
        UsuariosSelecionados.Clear();
    }

    [RelayCommand]
    public async Task NovoUsuarioAsync()
    {
        await Shell.Current.GoToAsync("usuario-cadastro");
    }

    private static Page? GetMainPage()
    {
        return Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
    }
}
