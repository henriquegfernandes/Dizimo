using Dizimo.ViewModels;
using Dizimo.Application.Usuarios.Handlers;

namespace Dizimo.Pages;

public partial class UsuarioListPage : ContentPage
{
    public UsuarioListPage(
        GetUsuarioHandlers getHandlers,
        CreateUsuarioHandler createHandler,
        UpdateUsuarioHandler updateHandler,
        DeleteUsuarioHandler deleteHandler,
        InativarUsuarioHandler inativarHandler,
        SessaoService sessaoService)
    {
        InitializeComponent();

        BindingContext = new UsuarioListViewModel(
            getHandlers,
            createHandler,
            updateHandler,
            deleteHandler,
            inativarHandler,
            sessaoService
        );
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is UsuarioListViewModel vm)
        {
            var sessaoServiceField = typeof(UsuarioListViewModel)
                .GetField("_sessaoService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var sessaoService = sessaoServiceField?.GetValue(vm) as SessaoService;

            if (sessaoService != null && !sessaoService.IsAdmin)
            {
                var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
                var mainWindow = windows != null && windows.Count > 0 ? windows[0] : null;
                var mainPage = mainWindow?.Page;
                if (mainPage != null)
                    await mainPage.DisplayAlertAsync("Acesso negado", "Apenas administradores podem acessar esta página.", "OK");
                await Shell.Current.GoToAsync("//login");
            }
        }
    }
}
