using Dizimo.ViewModels;
using Dizimo.Application.Usuarios.Handlers;

namespace Dizimo.Pages;

public partial class UsuarioCadastroPage : ContentPage
{
    public UsuarioCadastroPage(
        GetUsuarioHandlers getHandlers,
        UpdateUsuarioHandler updateHandler,
        CreateUsuarioHandler createHandler,
        DeleteUsuarioHandler deleteHandler)
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
        if (BindingContext is UsuarioCadastroViewModel)
        {
            if (!SessaoService.IsAdmin)
            {
                var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
                var mainPage = windows is { Count: > 0 } ? windows[0].Page : null;
                if (mainPage != null)
                    await mainPage.DisplayAlertAsync("Acesso negado", "Apenas administradores podem acessar esta página.", "OK");
                await Shell.Current.GoToAsync("login");
            }
        }
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///usuarios", true);
    }

    private async void OnCancelarClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///usuarios", true);
    }
}
