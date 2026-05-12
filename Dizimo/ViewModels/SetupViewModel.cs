using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;

namespace Dizimo.ViewModels;

public partial class SetupViewModel : ObservableObject
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IUnitOfWork _unitOfWork;

    private bool _isLoading;

    private string _mensagemErro = string.Empty;

    // ...existing code...
    private string _nomeUsuario = string.Empty;
    private Func<Task>? _onSetupComplete;

    private string _senha = string.Empty;

    private string _senhaConfirmacao = string.Empty;

    public SetupViewModel(IUnitOfWork unitOfWork, IAuthenticationService authenticationService)
    {
        _unitOfWork = unitOfWork;
        _authenticationService = authenticationService;
    }

    public string NomeUsuario
    {
        get => _nomeUsuario;
        set => SetProperty(ref _nomeUsuario, value);
    }

    public string Senha
    {
        get => _senha;
        set => SetProperty(ref _senha, value);
    }

    public string SenhaConfirmacao
    {
        get => _senhaConfirmacao;
        set => SetProperty(ref _senhaConfirmacao, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string MensagemErro
    {
        get => _mensagemErro;
        set => SetProperty(ref _mensagemErro, value);
    }

    /// <summary>
    ///     Define o callback a ser executado após setup bem-sucedido (DEPRECATED - usar IAuthenticationService)
    /// </summary>
    public void SetOnSetupComplete(Func<Task> onSetupComplete)
    {
        _onSetupComplete = onSetupComplete;
        Debug.WriteLine(
            "[AUTH] SetOnSetupComplete (deprecated) chamado - use IAuthenticationService.SetOnLoginSuccess");
    }

    [RelayCommand]
    public async Task CriarPrimeiroUsuarioAsync()
    {
        MensagemErro = string.Empty;

        // Validações
        if (string.IsNullOrWhiteSpace(NomeUsuario))
        {
            MensagemErro = "O nome de usuário é obrigatório.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Senha))
        {
            MensagemErro = "A senha é obrigatória.";
            return;
        }

        if (Senha.Length < 6)
        {
            MensagemErro = "A senha deve ter no mínimo 6 caracteres.";
            return;
        }

        if (Senha != SenhaConfirmacao)
        {
            MensagemErro = "As senhas não correspondem.";
            return;
        }

        IsLoading = true;

        try
        {
            // Verifica se já existe qualquer usuário cadastrado; se houver, bloqueia o setup e redireciona para login
            var usuariosExistentes = await _unitOfWork.Usuarios.GetAllAsync();
            if (usuariosExistentes.Any())
            {
                MensagemErro = "Já existe um usuário cadastrado. Faça login para continuar.";
                Debug.WriteLine("[NAV] Redirecionando para login (usuários já existem)");
                return;
            }

            // Verifica se já existe um usuário com esse login usando busca direta por login
            var usuarioComMesmoLogin = await _unitOfWork.Usuarios.GetByLoginAsync(NomeUsuario);
            if (usuarioComMesmoLogin is not null)
            {
                MensagemErro = "Já existe um usuário com esse login.";
                return;
            }

            // Cria o primeiro usuário como administrador
            var admin = new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = "Administrador",
                Login = NomeUsuario,
                SenhaHash = SessaoService.HashSenha(Senha),
                Perfil = PerfilUsuario.Admin,
                Ativo = true
            };

            await _unitOfWork.Usuarios.AddAsync(admin);
            await _unitOfWork.SaveChangesAsync();
            Debug.WriteLine("[AUTH] Primeiro usuário criado com sucesso");

            // Faz login automático com as credenciais digitadas
            // Usa o IAuthenticationService para manter consistência
            var loginSuccess = await _authenticationService.PerformLoginAsync(NomeUsuario, Senha);

            if (loginSuccess)
            {
                // Login bem-sucedido - o callback será executado pelo serviço (NavigateToDashboard)
                Debug.WriteLine("[AUTH] Setup concluído - login automático realizado");
            }
            else
            {
                // Login falhou (não deveria acontecer neste contexto)
                MensagemErro = "Erro ao fazer login automático após setup.";
                Debug.WriteLine("[ERROR] Erro ao fazer login automático após setup");
            }
        }
        catch (Exception ex)
        {
            MensagemErro = $"Erro ao criar usuário: {ex.Message}";
            Debug.WriteLine($"[ERROR] Erro ao criar primeiro usuário: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}