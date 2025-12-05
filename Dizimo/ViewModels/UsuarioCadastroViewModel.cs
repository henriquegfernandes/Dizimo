using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Application.Usuarios.Commands;
using Dizimo.Application.Usuarios.Handlers;
using Dizimo.Application.Usuarios.Queries;
using Dizimo.Domain.Entities;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;

namespace Dizimo.ViewModels;

public partial class UsuarioCadastroViewModel : ObservableObject, IQueryAttributable
{
    private readonly GetUsuarioHandlers _getHandlers;
    private readonly UpdateUsuarioHandler _updateHandler;
    private readonly CreateUsuarioHandler _createHandler;
    private readonly DeleteUsuarioHandler _deleteHandler;

    private string _nome = string.Empty;
    public string Nome
    {
        get => _nome;
        set => SetProperty(ref _nome, value);
    }

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

    private bool _ativo = true;
    public bool Ativo
    {
        get => _ativo;
        set => SetProperty(ref _ativo, value);
    }

    private Guid _id;
    public Guid Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    private bool _isEditMode;
    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    private ObservableCollection<string> _perfilOptions = new();
    public ObservableCollection<string> PerfilOptions
    {
        get => _perfilOptions;
        set => SetProperty(ref _perfilOptions, value);
    }

    private string _perfilSelecionado = string.Empty;
    public string PerfilSelecionado
    {
        get => _perfilSelecionado;
        set => SetProperty(ref _perfilSelecionado, value);
    }

    public UsuarioCadastroViewModel(
        GetUsuarioHandlers getHandlers,
        UpdateUsuarioHandler updateHandler,
        CreateUsuarioHandler createHandler,
        DeleteUsuarioHandler deleteHandler)
    {
        _getHandlers = getHandlers ?? throw new ArgumentNullException(nameof(getHandlers));
        _updateHandler = updateHandler ?? throw new ArgumentNullException(nameof(updateHandler));
        _createHandler = createHandler ?? throw new ArgumentNullException(nameof(createHandler));
        _deleteHandler = deleteHandler ?? throw new ArgumentNullException(nameof(deleteHandler));

        InitializePerfilOptions();
    }

    private void InitializePerfilOptions()
    {
        PerfilOptions = new ObservableCollection<string>
        {
            PerfilUsuario.Padrao.ToString(),
            PerfilUsuario.Admin.ToString()
        };
        PerfilSelecionado = PerfilUsuario.Padrao.ToString();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var idObj) && Guid.TryParse(idObj?.ToString(), out var usuarioId))
        {
            CarregarUsuarioCommand.Execute(usuarioId);
        }
        else
        {
            LimparCampos();
        }
    }

    public IAsyncRelayCommand<Guid> CarregarUsuarioCommand => new AsyncRelayCommand<Guid>(CarregarUsuarioAsync);

    private async Task CarregarUsuarioAsync(Guid usuarioId)
    {
        try
        {
            var usuarios = await _getHandlers.Handle(new GetAllUsuariosQuery());
            var usuario = usuarios.FirstOrDefault(u => u.Id == usuarioId);
            
            if (usuario != null)
            {
                Id = usuario.Id;
                Nome = usuario.Nome;
                Login = usuario.Login;
                Senha = string.Empty; // Não carrega a hash, força o usuário a inserir nova senha
                Ativo = usuario.Ativo;
                PerfilSelecionado = usuario.Perfil.ToString();
                IsEditMode = true;
            }
        }
        catch (Exception ex)
        {
            await GetMainPage()?.DisplayAlertAsync("Erro", $"Erro ao carregar usuário: {ex.Message}", "OK")!;
        }
    }

    public IAsyncRelayCommand SalvarCommand => new AsyncRelayCommand(SalvarAsync);

    private async Task SalvarAsync()
    {
        var mainPage = GetMainPage();
        if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Senha))
        {
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Validação", "Todos os campos são obrigatórios.", "OK");
            return;
        }

        try
        {
            if (IsEditMode)
            {
                var perfilEnum = (PerfilUsuario)Enum.Parse(typeof(PerfilUsuario), PerfilSelecionado);
                await _updateHandler.Handle(new UpdateUsuarioCommand(Id, Nome, Login, SessaoService.HashSenha(Senha), Ativo, perfilEnum));
                if (mainPage != null)
                    await mainPage.DisplayAlertAsync("Sucesso", "Usuário atualizado com sucesso.", "OK");
            }
            else
            {
                await _createHandler.Handle(new CreateUsuarioCommand(Nome, Login, SessaoService.HashSenha(Senha)));
                if (mainPage != null)
                    await mainPage.DisplayAlertAsync("Sucesso", "Usuário criado com sucesso.", "OK");
            }

            await Shell.Current.GoToAsync("///usuarios", true);
        }
        catch (Exception ex)
        {
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Erro", $"Erro ao salvar usuário: {ex.Message}", "OK");
        }
    }

    public IAsyncRelayCommand ExcluirCommand => new AsyncRelayCommand(ExcluirAsync);

    private async Task ExcluirAsync()
    {
        if (IsEditMode && Id != Guid.Empty)
        {
            var mainPage = GetMainPage();
            if (mainPage != null)
            {
                bool confirm = await mainPage.DisplayAlertAsync("Confirmação", $"Deseja excluir o usuário '{Nome}'?", "Sim", "Não");
                if (confirm)
                {
                    try
                    {
                        await _deleteHandler.Handle(new DeleteUsuarioCommand(Id));
                        await mainPage.DisplayAlertAsync("Sucesso", "Usuário excluído com sucesso.", "OK");
                        await Shell.Current.GoToAsync("../");
                    }
                    catch (Exception ex)
                    {
                        await mainPage.DisplayAlertAsync("Erro", $"Erro ao excluir usuário: {ex.Message}", "OK");
                    }
                }
            }
        }
    }

    private void LimparCampos()
    {
        Id = Guid.Empty;
        Nome = string.Empty;
        Login = string.Empty;
        Senha = string.Empty;
        Ativo = true;
        PerfilSelecionado = PerfilUsuario.Padrao.ToString();
        IsEditMode = false;
    }

    private static Page? GetMainPage()
    {
        return Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
    }
}
