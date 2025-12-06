using Dizimo.ViewModels;
using Dizimo.Application.Usuarios.Handlers;

namespace Dizimo.Pages;

public partial class UsuarioCadastroPage : ContentPage
{
    public UsuarioCadastroPage(
        GetUsuarioHandlers getHandlers,
        UpdateUsuarioHandler updateHandler,
        CreateUsuarioHandler createHandler,
        DeleteUsuarioHandler deleteHandler,
        SessaoService sessaoService)
    {
        InitializeComponent();

        BindingContext = new UsuarioCadastroViewModel(
            getHandlers,
            updateHandler,
            createHandler,
            deleteHandler
        );
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is UsuarioCadastroViewModel vm)
        {
            var sessaoService = new SessaoService();

            if (!sessaoService.IsAdmin)
            {
                var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage != null)
                    await mainPage.DisplayAlertAsync("Acesso negado", "Apenas administradores podem acessar esta página.", "OK");
                await Shell.Current.GoToAsync("//login");
            }
        }
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("../");
    }

    private async void OnCancelarClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("../");
    }
}
