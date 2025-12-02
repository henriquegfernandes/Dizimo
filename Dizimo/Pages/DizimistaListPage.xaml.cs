using Dizimo.ViewModels;
using Dizimo.Application.Dizimistas.Handlers;
using Dizimo.Application.Relatorios;
using Dizimo.Domain.Repositories;
using Dizimo.Domain.Entities;

namespace Dizimo.Pages;

public partial class DizimistaListPage : ContentPage
{
    // Remover o modificador 'readonly' do campo _sessaoService para permitir atribuição fora do construtor.
    private SessaoService? _sessaoService;
    private DizimistaListViewModel? _viewModel;

    // Construtor padrão para inicialização automática do ViewModel
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
            if (_viewModel != null)
            {
                if (_viewModel.NovoDizimistaCommand != null)
                    System.Diagnostics.Debug.WriteLine($"[INFO] NovoDizimistaCommand existe na ViewModel!");
                else
                    System.Diagnostics.Debug.WriteLine("[ERRO] NovoDizimistaCommand está nulo no ViewModel!");
            }
        }
        else
        {
            // Inicializa o ViewModel se não estiver presente
            var handlers = (App.Current as App)?.Services.GetService<GetDizimistaHandlers>() ?? throw new InvalidOperationException("GetDizimistaHandlers não está registrado no contêiner de serviços.");
            var deleteHandler = (App.Current as App)?.Services.GetService<DeleteDizimistaHandler>() ?? throw new InvalidOperationException("DeleteDizimistaHandler não está registrado no contêiner de serviços.");
            var inativarHandler = (App.Current as App)?.Services.GetService<InativarDizimistaHandler>() ?? throw new InvalidOperationException("InativarDizimistaHandler não está registrado no contêiner de serviços.");
            var csvService = (App.Current as App)?.Services.GetService<DizimistaCsvService>() ?? throw new InvalidOperationException("DizimistaCsvService não está registrado no contêiner de serviços.");
            var unitOfWork = (App.Current as App)?.Services.GetService<IUnitOfWork>() ?? throw new InvalidOperationException("IUnitOfWork não está registrado no contêiner de serviços.");
            var relatorioService = (App.Current as App)?.Services.GetService<RelatorioAniversariantesService>() ?? throw new InvalidOperationException("RelatorioAniversariantesService não está registrado no contêiner de serviços.");
            var viewModel = new DizimistaListViewModel(handlers, deleteHandler, inativarHandler, csvService, unitOfWork, relatorioService);
            BindingContext = viewModel;
            _viewModel = viewModel;
            System.Diagnostics.Debug.WriteLine("[INFO] DizimistaListPage BindingContext inicializado no OnBindingContextChanged.");
            if (_viewModel.NovoDizimistaCommand != null)
                System.Diagnostics.Debug.WriteLine($"[INFO] NovoDizimistaCommand existe na ViewModel!");
            else
                System.Diagnostics.Debug.WriteLine("[ERRO] NovoDizimistaCommand está nulo no ViewModel!");
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
        // Recarrega sempre a lista ao voltar para a página
        if (_viewModel != null)
            await _viewModel.CarregarDizimistasAsync();
    }

    private async void OnEditarDizimistaClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext != null)
        {
            var dizimista = button.BindingContext;
            if (dizimista != null)
            {
                var dizimistaId = dizimista.GetType().GetProperty("Id")?.GetValue(dizimista);
                var query = $"dizimista-cadastro?id={dizimistaId}";
                await Shell.Current.GoToAsync(query);
            }
        }
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
}
