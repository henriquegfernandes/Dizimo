using Dizimo.ViewModels;
using Dizimo.Domain.Repositories;
using Dizimo.Application.Ofertas.Handlers;
using Dizimo.Domain.Entities;
using Dizimo.Converters;

namespace Dizimo.Pages;

public partial class OfertaListPage : ContentPage
{
    private readonly SessaoService _sessaoService;
    private readonly IUnitOfWork _unitOfWork;

    public OfertaListPage(SessaoService sessaoService, OfertaListViewModel viewModel, IUnitOfWork unitOfWork)
    {
        InitializeComponent();
        _sessaoService = sessaoService;
        _unitOfWork = unitOfWork;
        BindingContext = viewModel;

        // Registrar UnitOfWork no converter
        var dizimistaConverter = Resources["DizimistaIdToNomeConverter"] as DizimistaIdToNomeConverter;
        dizimistaConverter?.SetUnitOfWork(_unitOfWork);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_sessaoService.IsLogado)
        {
            await DisplayAlertAsync("Acesso negado", "Faça login para acessar o sistema.", "OK");
            await Shell.Current.GoToAsync("login");
        }

        var viewModel = (OfertaListViewModel)BindingContext;
        await viewModel.CarregarOfertasAsync();
    }

    private async void OnNovaOfertaClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("oferta-cadastro");
    }

    private void OnFiltroCompleted(object sender, EventArgs e)
    {
        var viewModel = (OfertaListViewModel)BindingContext;
        viewModel.AplicarFiltrosCommand.Execute(null);
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var viewModel = (OfertaListViewModel)BindingContext;
        viewModel.OfertasSelecionadas.Clear();

        foreach (Oferta item in e.CurrentSelection.Cast<Oferta>())
        {
            viewModel.OfertasSelecionadas.Add(item);
        }
    }

    private void OnSelecionarTodosClicked(object sender, EventArgs e)
    {
        var viewModel = (OfertaListViewModel)BindingContext;
        var collectionView = (CollectionView)FindByName("OfertasCollectionView");

        if (viewModel.OfertasSelecionadas.Count == viewModel.Ofertas.Count)
        {
            // Desseleciona todos
            collectionView.SelectedItems.Clear();
        }
        else
        {
            // Seleciona todos
            collectionView.SelectedItems.Clear();
            foreach (var oferta in viewModel.Ofertas)
            {
                collectionView.SelectedItems.Add(oferta);
            }
        }
    }

    private async void OnEditarOfertaClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var oferta = (Oferta)button.CommandParameter;
        var viewModel = (OfertaListViewModel)BindingContext;
        await viewModel.EditarOfertaCommand.ExecuteAsync(oferta);
    }
}
