using Dizimo.ViewModels;

namespace Dizimo.Pages;

public partial class DizimistaCadastroPage : ContentPage
{
    public DizimistaCadastroPage(DizimistaCadastroViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
