using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Domain.Entities;
using Dizimo.Application.Dizimistas.Queries;
using Dizimo.Application.Dizimistas.Handlers;
using Dizimo.Application.Dizimistas.Commands;
using Dizimo.Infrastructure.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Linq;
using System.IO;
using Dizimo.Application.Relatorios;

namespace Dizimo.ViewModels;

public partial class DizimistaListViewModel : ObservableObject
{
    private readonly GetDizimistaHandlers _handlers;
    private readonly DeleteDizimistaHandler _deleteHandler;
    private readonly InativarDizimistaHandler _inativarHandler;
    private readonly DizimistaCsvService _csvService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RelatorioAniversariantesService _relatorioAniversariantesService;

    [ObservableProperty]
    private ObservableCollection<Dizimista> dizimistas = new();

    [ObservableProperty]
    private Dizimista? selectedDizimista;

    [ObservableProperty]
    private string filtroNome = string.Empty;

    [ObservableProperty]
    private string filtroNumeroCadastro = string.Empty;

    private List<Dizimista> todosDizimistas = new();

    [ObservableProperty]
    private ObservableCollection<Dizimista> aniversariantes = new();

    [ObservableProperty]
    private int filtroMesAniversario = DateTime.Today.Month;

    public DizimistaListViewModel(GetDizimistaHandlers handlers, DeleteDizimistaHandler deleteHandler, InativarDizimistaHandler inativarHandler, DizimistaCsvService csvService, IUnitOfWork unitOfWork, RelatorioAniversariantesService relatorioAniversariantesService)
    {
        _handlers = handlers;
        _deleteHandler = deleteHandler;
        _inativarHandler = inativarHandler;
        _csvService = csvService;
        _unitOfWork = unitOfWork;
        _relatorioAniversariantesService = relatorioAniversariantesService;
    }

    [RelayCommand]
    public async Task CarregarDizimistasAsync()
    {
        var lista = await _handlers.Handle(new GetAllDizimistasQuery());
        todosDizimistas = lista.ToList();
        AplicarFiltros();
    }

    [RelayCommand]
    public void AplicarFiltros()
    {
        var filtrados = todosDizimistas.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(FiltroNome))
            filtrados = filtrados.Where(d => d.Nome.Contains(FiltroNome, StringComparison.OrdinalIgnoreCase));
        if (int.TryParse(FiltroNumeroCadastro, out var num))
            filtrados = filtrados.Where(d => d.NumeroCadastro == num);
        Dizimistas = new ObservableCollection<Dizimista>(filtrados);
    }

    [RelayCommand]
    public async Task EditarDizimistaAsync()
    {
        if (SelectedDizimista != null)
        {
            var query = $"dizimista-cadastro?id={SelectedDizimista.Id}";
            await Shell.Current.GoToAsync(query);
        }
    }

    [RelayCommand]
    public async Task ExcluirDizimistaAsync()
    {
        if (SelectedDizimista != null)
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert("Confirmação", $"Deseja excluir o dizimista '{SelectedDizimista.Nome}'?", "Sim", "Não");
            if (confirm)
            {
                try
                {
                    await _deleteHandler.Handle(new DeleteDizimistaCommand(SelectedDizimista.Id));
                    await CarregarDizimistasAsync();
                    await Application.Current.MainPage.DisplayAlert("Sucesso", "Dizimista excluído com sucesso.", "OK");
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Erro", $"Erro ao excluir: {ex.Message}", "OK");
                }
            }
        }
    }

    [RelayCommand]
    public async Task InativarDizimistaAsync()
    {
        if (SelectedDizimista != null)
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert("Confirmação", $"Deseja inativar o dizimista '{SelectedDizimista.Nome}'?", "Sim", "Não");
            if (confirm)
            {
                try
                {
                    await _inativarHandler.Handle(new InativarDizimistaCommand(SelectedDizimista.Id));
                    await CarregarDizimistasAsync();
                    await Application.Current.MainPage.DisplayAlert("Sucesso", "Dizimista inativado com sucesso.", "OK");
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Erro", $"Erro ao inativar: {ex.Message}", "OK");
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
            var filePath = Path.Combine(FileSystem.Current.AppDataDirectory, "dizimistas_export.csv");
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
            var filePath = Path.Combine(FileSystem.Current.AppDataDirectory, "dizimistas_import.csv");
            if (!File.Exists(filePath))
            {
                await Application.Current.MainPage.DisplayAlert("Importação", $"Coloque o arquivo 'dizimistas_import.csv' em: {filePath}", "OK");
                return;
            }
            var csv = File.ReadAllText(filePath);
            var dizimistas = await _csvService.ImportarAsync(csv);
            foreach (var d in dizimistas)
            {
                await _unitOfWork.Dizimistas.AddAsync(d);
            }
            await _unitOfWork.SaveChangesAsync();
            await CarregarDizimistasAsync();
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
            var modeloPath = Path.Combine(FileSystem.Current.AppDataDirectory, "dizimistas_modelo.csv");
            var modeloOrigem = Path.Combine(AppContext.BaseDirectory, "dizimistas_modelo.csv");
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

    [RelayCommand]
    public async Task GerarRelatorioAniversariantesAsync()
    {
        var lista = await _relatorioAniversariantesService.GetAniversariantesAsync(FiltroMesAniversario);
        Aniversariantes = new ObservableCollection<Dizimista>(lista);
        await Application.Current.MainPage.DisplayAlert("Relatório Aniversariantes", $"{Aniversariantes.Count} aniversariantes encontrados para o mês {FiltroMesAniversario}", "OK");
    }

    [RelayCommand]
    public async Task GerarRelatorioGeralAsync()
    {
        await CarregarDizimistasAsync();
        await Application.Current.MainPage.DisplayAlert("Relatório Geral", $"Total de dizimistas: {Dizimistas.Count}", "OK");
    }

    [RelayCommand]
    public async Task ExportarRelatorioGeralAsync()
    {
        try
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("NumeroCadastro,Nome,DataNascimento,Ativo");
            foreach (var d in Dizimistas)
            {
                sb.AppendLine($"{d.NumeroCadastro},\"{d.Nome}\",{d.DataNascimento:yyyy-MM-dd},{d.Ativo}");
            }
            var filePath = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, "relatorio_dizimistas.csv");
            System.IO.File.WriteAllText(filePath, sb.ToString());
            await Application.Current.MainPage.DisplayAlert("Exportação", $"Relatório geral exportado para: {filePath}", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Erro", $"Erro ao exportar relatório: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task ExportarRelatorioAniversariantesAsync()
    {
        try
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("NumeroCadastro,Nome,DataNascimento,Ativo");
            foreach (var d in Aniversariantes)
            {
                sb.AppendLine($"{d.NumeroCadastro},\"{d.Nome}\",{d.DataNascimento:yyyy-MM-dd},{d.Ativo}");
            }
            var filePath = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, "relatorio_aniversariantes.csv");
            System.IO.File.WriteAllText(filePath, sb.ToString());
            await Application.Current.MainPage.DisplayAlert("Exportação", $"Relatório de aniversariantes exportado para: {filePath}", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Erro", $"Erro ao exportar relatório: {ex.Message}", "OK");
        }
    }
}
