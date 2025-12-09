using Dizimo.Application.Ofertas.Handlers;
using Dizimo.Domain.Repositories;
using Dizimo.ViewModels;

namespace Dizimo.Pages;

public partial class OfertaCadastroPage : ContentPage
{
    public OfertaCadastroPage(
        CreateOfertaHandler createHandler,
        UpdateOfertaHandler updateHandler,
        GetOfertaHandlers getHandler,
        IUnitOfWork unitOfWork)
    {
        InitializeComponent();
        BindingContext = new OfertaCadastroViewModel(createHandler, updateHandler, getHandler, unitOfWork);
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///ofertas", true);
    }

    private async void OnCodigoDizimistaUnfocused(object sender, FocusEventArgs e)
    {
        var viewModel = (OfertaCadastroViewModel)BindingContext;
        if (viewModel?.BuscarDizimistaCommand.CanExecute(null) == true)
        {
            await viewModel.BuscarDizimistaCommand.ExecuteAsync(null);
        }
    }
}
