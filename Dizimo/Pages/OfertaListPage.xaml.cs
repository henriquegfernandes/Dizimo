using Dizimo.ViewModels;
using Dizimo.Domain.Repositories;
using Dizimo.Application.Ofertas.Handlers;
using Dizimo.Application.Relatorios;

namespace Dizimo.Pages;

public partial class OfertaListPage : ContentPage
{
    private readonly SessaoService _sessaoService;

    public OfertaListPage(SessaoService sessaoService, OfertaListViewModel viewModel)
    {
        InitializeComponent();
        _sessaoService = sessaoService;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_sessaoService.IsLogado)
        {
            await DisplayAlertAsync("Acesso negado", "Faça login para acessar o sistema.", "OK");
            await Shell.Current.GoToAsync("login");
        }
    }

    private async void OnNovaOfertaClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("oferta-cadastro");
    }
}
