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
        await Shell.Current.GoToAsync("///dizimistas", true);
    }
}
