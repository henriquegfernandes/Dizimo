using Dizimo.ViewModels;

namespace Dizimo.Pages;

public partial class DizimistaCadastroPage : ContentPage
{
    public DizimistaCadastroPage(DizimistaCadastroViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("[INFO] Botão voltar clicado na página de cadastro");
        await Shell.Current.GoToAsync("..", true);
    }
}
