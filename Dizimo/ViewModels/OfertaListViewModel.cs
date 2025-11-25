using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Domain.Entities;
using Dizimo.Infrastructure.Services;
using Dizimo.Domain.Repositories;
using Dizimo.Application.Ofertas.Commands;
using Dizimo.Application.Ofertas.Handlers;
using Dizimo.Application.Relatorios;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Linq;
using System.IO;

namespace Dizimo.ViewModels;

public partial class OfertaListViewModel : ObservableObject
{
    private readonly OfertaCsvService _csvService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateOfertaHandler _updateHandler;
    private readonly DeleteOfertaHandler _deleteHandler;
    private readonly RelatorioOfertasService _relatorioOfertasService;

    [ObservableProperty]
    private ObservableCollection<Oferta> ofertas = new();

    [ObservableProperty]
    private Oferta? selectedOferta;

    [ObservableProperty]
    private DateTime filtroData = DateTime.Today;

    [ObservableProperty]
    private string filtroCodigoDizimista = string.Empty;

    private List<Oferta> todasOfertas = new();

    public OfertaListViewModel(
        OfertaCsvService csvService,
        IUnitOfWork unitOfWork,
        UpdateOfertaHandler updateHandler,
        DeleteOfertaHandler deleteHandler,
        RelatorioOfertasService relatorioOfertasService)
    {
        _csvService = csvService;
        _unitOfWork = unitOfWork;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _relatorioOfertasService = relatorioOfertasService;
    }

    [RelayCommand]
    public async Task CarregarOfertasAsync()
    {
        var lista = await _unitOfWork.Ofertas.GetAllAsync();
        todasOfertas = lista.ToList();
        AplicarFiltros();
    }

    [RelayCommand]
    public void AplicarFiltros()
    {
        var filtrados = todasOfertas.AsEnumerable();
        if (FiltroData != default)
            filtrados = filtrados.Where(o => o.Data.Date == FiltroData.Date);
        if (int.TryParse(FiltroCodigoDizimista, out var codigo))
            filtrados = filtrados.Where(o => {
                var dizimista = _unitOfWork.Dizimistas.GetByIdAsync(o.DizimistaId).Result;
                return dizimista != null && dizimista.NumeroCadastro == codigo;
            });
        Ofertas = new ObservableCollection<Oferta>(filtrados);
    }

    [RelayCommand]
    public async Task EditarOfertaAsync()
    {
        if (SelectedOferta != null)
        {
            var query = $"oferta-cadastro?id={SelectedOferta.Id}";
            await Shell.Current.GoToAsync(query);
        }
    }

    [RelayCommand]
    public async Task ExcluirOfertaAsync()
    {
        if (SelectedOferta != null)
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert("Confirmação", $"Deseja excluir a oferta de valor {SelectedOferta.Valor:C} em {SelectedOferta.Data:dd/MM/yyyy}?", "Sim", "Não");
            if (confirm)
            {
                try
                {
                    await _deleteHandler.Handle(new DeleteOfertaCommand(SelectedOferta.Id));
                    await CarregarOfertasAsync();
                    await Application.Current.MainPage.DisplayAlert("Sucesso", "Oferta excluída com sucesso.", "OK");
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Erro", $"Erro ao excluir: {ex.Message}", "OK");
                }
            }
        }
    }

    [RelayCommand]
    public async Task ExportarAsync()
    {
        try
        {
            var csv = await _csvService.ExportarAsync();
            var filePath = Path.Combine(FileSystem.Current.AppDataDirectory, "ofertas_export.csv");
            File.WriteAllText(filePath, csv);
            await Application.Current.MainPage.DisplayAlert("Exportação", $"Arquivo exportado para: {filePath}", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Erro", $"Erro ao exportar: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task ImportarAsync()
    {
        try
        {
            var filePath = Path.Combine(FileSystem.Current.AppDataDirectory, "ofertas_import.csv");
            if (!File.Exists(filePath))
            {
                await Application.Current.MainPage.DisplayAlert("Importação", $"Coloque o arquivo 'ofertas_import.csv' em: {filePath}", "OK");
                return;
            }
            var csv = File.ReadAllText(filePath);
            var ofertas = await _csvService.ImportarAsync(csv);
            foreach (var o in ofertas)
            {
                await _unitOfWork.Ofertas.AddAsync(o);
            }
            await _unitOfWork.SaveChangesAsync();
            await CarregarOfertasAsync();
            await Application.Current.MainPage.DisplayAlert("Importação", $"Importação concluída com sucesso.", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Erro", $"Erro ao importar: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task BaixarModeloAsync()
    {
        try
        {
            var modeloPath = Path.Combine(FileSystem.Current.AppDataDirectory, "ofertas_modelo.csv");
            var modeloOrigem = Path.Combine(AppContext.BaseDirectory, "ofertas_modelo.csv");
            if (File.Exists(modeloOrigem))
            {
                File.Copy(modeloOrigem, modeloPath, true);
                await Application.Current.MainPage.DisplayAlert("Modelo", $"Arquivo modelo salvo em: {modeloPath}", "OK");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Modelo", "Arquivo modelo não encontrado.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Erro", $"Erro ao baixar modelo: {ex.Message}", "OK");
        }
    }

    [ObservableProperty]
    private decimal totalOfertasPorData;

    [RelayCommand]
    public async Task GerarRelatorioPorDataAsync()
    {
        TotalOfertasPorData = await _relatorioOfertasService.GetTotalOfertasPorDataAsync(FiltroData);
        await Application.Current.MainPage.DisplayAlert("Relatório Ofertas", $"Total de ofertas em {FiltroData:dd/MM/yyyy}: {TotalOfertasPorData:C}", "OK");
    }

    [RelayCommand]
    public async Task ExportarRelatorioPorDataAsync()
    {
        try
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("DizimistaId,Valor,Data");
            var ofertas = todasOfertas.Where(o => o.Data.Date == FiltroData.Date);
            foreach (var o in ofertas)
            {
                sb.AppendLine($"{o.DizimistaId},{o.Valor.ToString(System.Globalization.CultureInfo.InvariantCulture)},{o.Data:yyyy-MM-dd}");
            }
            var filePath = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, "relatorio_ofertas_por_data.csv");
            System.IO.File.WriteAllText(filePath, sb.ToString());
            await Application.Current.MainPage.DisplayAlert("Exportação", $"Relatório de ofertas exportado para: {filePath}", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Erro", $"Erro ao exportar relatório: {ex.Message}", "OK");
        }
    }
}
