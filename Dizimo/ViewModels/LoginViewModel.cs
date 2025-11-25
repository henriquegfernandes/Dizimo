using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Domain.Repositories;
using Dizimo.Application.Usuarios.Queries;
using Dizimo.Application.Usuarios.Handlers;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Dizimo.Infrastructure.Services;

namespace Dizimo.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly GetUsuarioHandlers _getUsuarioHandlers;
    private readonly SessaoService _sessaoService;

    [ObservableProperty] private string login = string.Empty;
    [ObservableProperty] private string senha = string.Empty;
    [ObservableProperty] private bool isLoginFailed;

    public LoginViewModel(GetUsuarioHandlers getUsuarioHandlers, SessaoService sessaoService)
    {
        _getUsuarioHandlers = getUsuarioHandlers;
        _sessaoService = sessaoService;
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        var usuario = await _getUsuarioHandlers.Handle(new GetUsuarioByLoginQuery(Login));
        if (usuario != null && usuario.Ativo && usuario.SenhaHash == SessaoService.HashSenha(Senha))
        {
            _sessaoService.Login(usuario.Id, usuario.Perfil);
            await Shell.Current.GoToAsync("//dizimistas");
        }
        else
        {
            IsLoginFailed = true;
            await Application.Current.MainPage.DisplayAlert("Erro", "Login ou senha inválidos.", "OK");
        }
    }
}
