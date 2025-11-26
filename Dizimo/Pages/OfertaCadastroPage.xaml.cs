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
}
