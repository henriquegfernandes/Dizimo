using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Dizimo.ViewModels;

namespace Dizimo.Pages;

public partial class DizimistaListPage : ContentPage
{
    private readonly SessaoService? _sessaoService;

    public DizimistaListPage(DizimistaListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
        var mainPage = windows != null && windows.Count > 0 ? windows[0].Page : null;
        _sessaoService = mainPage?.BindingContext as SessaoService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_sessaoService != null && !_sessaoService.IsLogado)
        {
            var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
            var mainPage = windows != null && windows.Count > 0 ? windows[0].Page : null;
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Acesso negado", "Faça login para acessar o sistema.", "OK");
            await Shell.Current.GoToAsync("//login");
        }
    }

    private async void OnNovoDizimistaClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("dizimista-cadastro");
    }
}
