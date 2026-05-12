using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Application.Usuarios.Commands;
using Dizimo.Application.Usuarios.Handlers;
using Dizimo.Application.Usuarios.Queries;
using Dizimo.Domain.Entities;

namespace Dizimo.ViewModels;

public partial class UsuarioCadastroViewModel : ObservableObject, INavigationAware
{
    private readonly CreateUsuarioHandler _createHandler;
    private readonly DeleteUsuarioHandler _deleteHandler;
    private readonly IDialogService _dialogService;
    private readonly GetUsuarioHandlers _getHandlers;
    private readonly INavigationService _navigationService;
    private readonly UpdateUsuarioHandler _updateHandler;

    private bool _ativo = true;

    private Guid _id;

    private bool _isEditMode;

    private string _login = string.Empty;

    private string _nome = string.Empty;

    private ObservableCollection<string> _perfilOptions = new();

    private string _perfilSelecionado = string.Empty;

    private string _senha = string.Empty;

    public UsuarioCadastroViewModel(
        GetUsuarioHandlers getHandlers,
        UpdateUsuarioHandler updateHandler,
        CreateUsuarioHandler createHandler,
        DeleteUsuarioHandler deleteHandler,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _getHandlers = getHandlers ?? throw new ArgumentNullException(nameof(getHandlers));
        _updateHandler = updateHandler ?? throw new ArgumentNullException(nameof(updateHandler));
        _createHandler = createHandler ?? throw new ArgumentNullException(nameof(createHandler));
        _deleteHandler = deleteHandler ?? throw new ArgumentNullException(nameof(deleteHandler));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

        InitializePerfilOptions();
    }

    public string Nome
    {
        get => _nome;
        set => SetProperty(ref _nome, value);
    }

    public string Login
    {
        get => _login;
        set => SetProperty(ref _login, value);
    }

    public string Senha
    {
        get => _senha;
        set => SetProperty(ref _senha, value);
    }

    public bool Ativo
    {
        get => _ativo;
        set => SetProperty(ref _ativo, value);
    }

    public Guid Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    public ObservableCollection<string> PerfilOptions
    {
        get => _perfilOptions;
        set => SetProperty(ref _perfilOptions, value);
    }

    public string PerfilSelecionado
    {
        get => _perfilSelecionado;
        set => SetProperty(ref _perfilSelecionado, value);
    }

    public IAsyncRelayCommand SalvarCommand => new AsyncRelayCommand(SalvarAsync);

    public IAsyncRelayCommand ExcluirCommand => new AsyncRelayCommand(ExcluirAsync);

    public void OnNavigatedTo(NavigationParameters parameters)
    {
        // Tenta extrair o ID do parâmetro de navegação
        Guid? usuarioId = null;

        if (parameters != null && parameters.TryGetValue("id", out var idObj))
        {
            if (idObj is Guid guidId)
                usuarioId = guidId;
            else if (Guid.TryParse(idObj?.ToString(), out var parsedId))
                usuarioId = parsedId;
        }

        if (usuarioId.HasValue && usuarioId.Value != Guid.Empty)
            _ = CarregarUsuarioAsync(usuarioId.Value);
        else
            LimparCampos();
    }

    public void OnNavigatedFrom()
    {
        // Lógica ao sair da página se necessário
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
            await _dialogService.ShowAlertAsync("Erro", $"Erro ao carregar usuário: {ex.Message}");
        }
    }

    private async Task SalvarAsync()
    {
        if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(Login))
        {
            await _dialogService.ShowAlertAsync("Validação", "Por favor, preencha Nome e Login");
            return;
        }

        // Em modo de criação, senha é obrigatória
        if (!IsEditMode && string.IsNullOrWhiteSpace(Senha))
        {
            await _dialogService.ShowAlertAsync("Validação", "Por favor, preencha a Senha");
            return;
        }

        try
        {
            if (IsEditMode)
            {
                var perfilEnum = (PerfilUsuario)Enum.Parse(typeof(PerfilUsuario), PerfilSelecionado);
                // Se senha vazia em modo edição, usa um valor especial indicando para não alterar
                var senhaHash = string.IsNullOrWhiteSpace(Senha) ? string.Empty : SessaoService.HashSenha(Senha);
                await _updateHandler.Handle(new UpdateUsuarioCommand(Id, Nome, Login, senhaHash, Ativo, perfilEnum));
                await _dialogService.ShowAlertAsync("Sucesso", "Usuário atualizado com sucesso");
            }
            else
            {
                await _createHandler.Handle(new CreateUsuarioCommand(Nome, Login, SessaoService.HashSenha(Senha)));
                await _dialogService.ShowAlertAsync("Sucesso", "Usuário criado com sucesso");
            }

            LimparCampos();
            _navigationService.GoBack();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erro", $"Erro ao salvar usuário: {ex.Message}");
        }
    }

    private async Task ExcluirAsync()
    {
        if (IsEditMode && Id != Guid.Empty)
        {
            var confirm =
                await _dialogService.ShowConfirmAsync("Confirmar", "Tem certeza que deseja excluir este usuário?");
            if (confirm)
                try
                {
                    await _deleteHandler.Handle(new DeleteUsuarioCommand(Id));
                    await _dialogService.ShowAlertAsync("Sucesso", "Usuário excluído com sucesso");
                    LimparCampos();
                    _navigationService.GoBack();
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowAlertAsync("Erro", $"Erro ao excluir usuário: {ex.Message}");
                }
        }
    }

    [RelayCommand]
    public void Voltar()
    {
        _navigationService.GoBack();
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
}