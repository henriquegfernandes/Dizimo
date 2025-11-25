using Microsoft.Maui.Controls;
using Dizimo.Infrastructure.Services;

namespace Dizimo.Pages;

public partial class OfertaListPage : ContentPage
{
    private readonly SessaoService _sessaoService;

    public OfertaListPage()
    {
        InitializeComponent();
        _sessaoService = Application.Current.Services.GetService<SessaoService>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (!_sessaoService.IsLogado)
        {
            DisplayAlert("Acesso negado", "Faça login para acessar o sistema.", "OK");
            Shell.Current.GoToAsync("//login");
        }
    }

    private async void OnNovaOfertaClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("oferta-cadastro");
    }
}
