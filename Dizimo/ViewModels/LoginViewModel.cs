using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Application.Usuarios.Queries;
using Dizimo.Application.Usuarios.Handlers;
using Microsoft.Maui.Controls;

namespace Dizimo.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly GetUsuarioHandlers _getUsuarioHandlers;

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

    private bool _isLoginFailed;
    public bool IsLoginFailed
    {
        get => _isLoginFailed; 
        set => SetProperty(ref _isLoginFailed, value);
    }

    public LoginViewModel(GetUsuarioHandlers getUsuarioHandlers)
    {
        _getUsuarioHandlers = getUsuarioHandlers;
        ResetLoginState();
    }

    public void ResetLoginState()
    {
        Login = string.Empty;
        Senha = string.Empty;
        IsLoginFailed = false;
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        var usuario = await _getUsuarioHandlers.Handle(new GetUsuarioByLoginQuery(Login));
        if (usuario != null && usuario.Ativo && usuario.SenhaHash == SessaoService.HashSenha(Senha))
        {
            SessaoService.Login(usuario.Id, usuario.Perfil);
            // Atualiza o BindingContext do AppShell após login
            if (Shell.Current is AppShell appShell)
            {
                var app = Microsoft.Maui.Controls.Application.Current as App;
                var mainVm = app?.Services.GetService<MainViewModel>();
                var backupVm = app?.Services.GetService<LocalBackupViewModel>();
                if (mainVm != null && backupVm != null)
                    appShell.BindingContext = new ShellViewModel(mainVm, backupVm);
            }
            await Shell.Current.GoToAsync("//main");
        }
        else
        {
            IsLoginFailed = true;
            var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
            var mainPage = windows is { Count: > 0 } ? windows[0].Page : null;
            if (mainPage != null)
            {
                await mainPage.DisplayAlertAsync("Erro", "Login ou senha inválidos.", "OK");
            }
        }
    }
}
