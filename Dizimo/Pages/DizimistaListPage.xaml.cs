using Dizimo.ViewModels;
using Dizimo.Application.Dizimistas.Handlers;
using Dizimo.Domain.Repositories;
using Dizimo.Domain.Entities;

namespace Dizimo.Pages;

public partial class DizimistaListPage : ContentPage
{
    private SessaoService? _sessaoService;
    private DizimistaListViewModel? _viewModel;

    public DizimistaListPage()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (BindingContext is DizimistaListViewModel vm)
        {
            _viewModel = vm;
            System.Diagnostics.Debug.WriteLine("[INFO] DizimistaListPage BindingContext é DizimistaListViewModel.");
        }
        else
        {
            var handlers = (App.Current as App)?.Services.GetService<GetDizimistaHandlers>() ?? throw new InvalidOperationException("GetDizimistaHandlers não está registrado no contêiner de serviços.");
            var deleteHandler = (App.Current as App)?.Services.GetService<DeleteDizimistaHandler>() ?? throw new InvalidOperationException("DeleteDizimistaHandler não está registrado no contêiner de serviços.");
            var inativarHandler = (App.Current as App)?.Services.GetService<InativarDizimistaHandler>() ?? throw new InvalidOperationException("InativarDizimistaHandler não está registrado no contêiner de serviços.");
            var excelService = (App.Current as App)?.Services.GetService<DizimistaExcelService>() ?? throw new InvalidOperationException("DizimistaExcelService não está registrado no contêiner de serviços.");
            var unitOfWork = (App.Current as App)?.Services.GetService<IUnitOfWork>() ?? throw new InvalidOperationException("IUnitOfWork não está registrado no contêiner de serviços.");
            var viewModel = new DizimistaListViewModel(handlers, deleteHandler, inativarHandler, excelService, unitOfWork);
            BindingContext = viewModel;
            _viewModel = viewModel;
            System.Diagnostics.Debug.WriteLine("[INFO] DizimistaListPage BindingContext inicializado no OnBindingContextChanged.");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
        var mainPage = windows != null && windows.Count > 0 ? windows[0].Page : null;
        _sessaoService = mainPage?.BindingContext as SessaoService;
        if (_sessaoService != null && !_sessaoService.IsLogado)
        {
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Acesso negado", "Faça login para acessar o sistema.", "OK");
            await Shell.Current.GoToAsync("//login");
            return;
        }
        if (_viewModel != null)
            await _viewModel.CarregarDizimistasAsync();
    }

    private async void OnNovoDizimistaClicked(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("[INFO] Comando NovoDizimista executado!");
        await Shell.Current.GoToAsync("dizimista-cadastro");
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BindingContext is DizimistaListViewModel vm)
        {
            vm.DizimistasSelecionados.Clear();
            foreach (var item in e.CurrentSelection.OfType<Dizimista>())
                vm.DizimistasSelecionados.Add(item);
        }
    }

    private void OnFiltroCompleted(object sender, EventArgs e)
    {
        if (BindingContext is DizimistaListViewModel vm && vm.AplicarFiltrosCommand.CanExecute(null))
            vm.AplicarFiltrosCommand.Execute(null);
    }

    private async void OnVerDetalhesDizimistaClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Dizimista dizimista)
        {
            System.Diagnostics.Debug.WriteLine($"[INFO] Ver detalhes do dizimista: {dizimista.Nome} (ID: {dizimista.Id})");
            
            var navigationParameter = new Dictionary<string, object>
            {
                { "id", dizimista.Id.ToString() }
            };
            
            System.Diagnostics.Debug.WriteLine($"[INFO] Navegando para dizimista-detalhes com ID: {dizimista.Id}");
            await Shell.Current.GoToAsync("dizimista-detalhes", navigationParameter);
        }
    }
}
