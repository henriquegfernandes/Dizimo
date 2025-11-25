using Dizimo.Infrastructure.Services;
using Dizimo.Pages;
using Microsoft.Maui.Controls;

namespace Dizimo.Pages;

public partial class DizimistaListPage : ContentPage
{
    private readonly SessaoService _sessaoService;

    public DizimistaListPage()
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

    private async void OnNovoDizimistaClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("dizimista-cadastro");
    }
}
