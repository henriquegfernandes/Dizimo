using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;
using Microsoft.Maui.Controls;
using System.Security.Cryptography;
using System.Text;

namespace Dizimo.ViewModels;

public partial class SetupViewModel(IUnitOfWork unitOfWork) : ObservableObject
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;


    private string _nomeUsuario = string.Empty;
    public string NomeUsuario
    {
        get => _nomeUsuario;
        set => SetProperty(ref _nomeUsuario, value);
    }

    private string _senha = string.Empty;
    public string Senha
    {
        get => _senha;
        set => SetProperty(ref _senha, value);
    }

    private string _senhaConfirmacao = string.Empty;
    public string SenhaConfirmacao
    {
        get => _senhaConfirmacao;
        set => SetProperty(ref _senhaConfirmacao, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private string _mensagemErro = string.Empty;
    public string MensagemErro
    {
        get => _mensagemErro;
        set => SetProperty(ref _mensagemErro, value);
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
                await Shell.Current.GoToAsync("login");
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
                SenhaHash = HashSenhaBase64(Senha),
                Perfil = PerfilUsuario.Admin,
                Ativo = true
            };

            await _unitOfWork.Usuarios.AddAsync(admin);
            await _unitOfWork.SaveChangesAsync();

            // Navega para a página de login
            await Shell.Current.GoToAsync("login");
        }
        catch (Exception ex)
        {
            MensagemErro = $"Erro ao criar usuário: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static string HashSenhaBase64(string senha)
    {
        return SessaoService.HashSenha(senha);
    }
}
