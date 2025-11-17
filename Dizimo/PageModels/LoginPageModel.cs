using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Core.Services;

namespace Dizimo.PageModels;

public partial class LoginPageModel : ObservableObject
{
    private readonly AuthService _auth;

    [ObservableProperty] private string username = string.Empty;
    [ObservableProperty] private string password = string.Empty;

    public LoginPageModel(AuthService auth)
    {
        _auth = auth;
    }

    [ICommand]
    public async Task LoginAsync()
    {
        var ok = _auth.Authenticate(Username, Password);
        if (ok)
        {
            await Shell.Current.GoToAsync("//main");
        }
        else
        {
            await Shell.Current.DisplayAlert("Erro", "Credenciais inválidas", "OK");
        }
    }
}
