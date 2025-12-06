using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Application.Usuarios.Queries;
using Dizimo.Application.Usuarios.Handlers;
using Microsoft.Maui.Controls;

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
            _sessaoService.Login(usuario.Id, usuario.Perfil);
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
            var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
            if (mainPage != null)
            {
                await mainPage.DisplayAlertAsync("Erro", "Login ou senha inválidos.", "OK");
            }
        }
    }
}
