using Dizimo.ViewModels;
using Dizimo.Application.Usuarios.Handlers;

namespace Dizimo.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
        var app = Microsoft.Maui.Controls.Application.Current as App;
        var vm = app?.Services.GetService<LoginViewModel>();
        if (vm is not null)
        {
            BindingContext = vm;
        }
        else
        {
            // Corrigido: obtendo as dependências do contêiner de serviços
            var getUsuarioHandlers = app?.Services.GetService<GetUsuarioHandlers>();
            var sessaoService = app?.Services.GetService<SessaoService>();
            if (getUsuarioHandlers != null && sessaoService != null)
            {
                BindingContext = new LoginViewModel(getUsuarioHandlers, sessaoService);
            }
            else
            {
                // Opcional: tratamento de erro ou fallback
                BindingContext = null;
            }
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is LoginViewModel vm)
            vm.ResetLoginState();
    }
}
