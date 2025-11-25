using Microsoft.Maui.Controls;
using Dizimo.Infrastructure.Services;

namespace Dizimo.Pages;

public partial class UsuarioListPage : ContentPage
{
    private readonly SessaoService _sessaoService;

    public UsuarioListPage()
    {
        InitializeComponent();
        _sessaoService = Application.Current.Services.GetService<SessaoService>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (!_sessaoService.IsAdmin)
        {
            DisplayAlert("Acesso negado", "Apenas administradores podem acessar esta página.", "OK");
            Shell.Current.GoToAsync("//login");
        }
    }
}
