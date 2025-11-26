namespace Dizimo.Pages;

public partial class RelatorioPage : ContentPage
{
    public RelatorioPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (!IsUsuarioLogado())
        {
            DisplayAlert("Acesso negado", "Faça login para acessar os relatórios.", "OK");
            Shell.Current.GoToAsync("//login");
        }
    }

    private bool IsUsuarioLogado()
    {
        return Preferences.ContainsKey("UsuarioId");
    }
}
