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
            if (getUsuarioHandlers != null)
            {
                BindingContext = new LoginViewModel(getUsuarioHandlers);
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

    private void OnLoginEntryCompleted(object sender, EventArgs e)
    {
        SenhaEntry.Focus();
    }

    private void OnSenhaEntryCompleted(object sender, EventArgs e)
    {
        if (BindingContext is LoginViewModel vm && vm.LoginCommand.CanExecute(null))
        {
            vm.LoginCommand.Execute(null);
        }
    }
}
