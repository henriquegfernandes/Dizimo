using Dizimo.ViewModels;
using Dizimo.Application.Usuarios.Handlers;
using Dizimo.Domain.Entities;

namespace Dizimo.Pages;

public partial class UsuarioListPage : ContentPage
{

    public UsuarioListPage(
        GetUsuarioHandlers getHandlers,
        CreateUsuarioHandler createHandler,
        UpdateUsuarioHandler updateHandler,
        DeleteUsuarioHandler deleteHandler,
        InativarUsuarioHandler inativarHandler)
    {
        InitializeComponent();

        BindingContext = new UsuarioListViewModel(
            getHandlers,
            createHandler,
            updateHandler,
            deleteHandler,
            inativarHandler
        );
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (!SessaoService.IsAdmin)
        {
            var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
            var mainPage = windows is { Count: > 0 } ? windows[0].Page : null;
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Acesso negado", "Apenas administradores podem acessar esta página.", "OK");
            await Shell.Current.GoToAsync("login");
            return;
        }

        if (BindingContext is UsuarioListViewModel viewModel)
        {
            await viewModel.CarregarUsuariosCommand.ExecuteAsync(null);
        }
    }

    private void OnFiltroCompleted(object sender, EventArgs e)
    {
        if (BindingContext is UsuarioListViewModel vm)
        {
            vm.AplicarFiltrosCommand?.Execute(null);
        }
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BindingContext is UsuarioListViewModel vm)
        {
            vm.UsuariosSelecionados.Clear();
            
            foreach (Usuario item in e.CurrentSelection.Cast<Usuario>())
            {
                vm.UsuariosSelecionados.Add(item);
            }
        }
    }

    private async void OnNovoUsuarioClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("usuario-cadastro");
    }

    private async void OnEditarUsuarioClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Usuario usuario)
        {
            await Shell.Current.GoToAsync($"usuario-cadastro?id={usuario.Id}");
        }
    }
}
