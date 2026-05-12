using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Application.Dashboard;
using Dizimo.Application.Reporting.Services;
using Dizimo.Converters;
using Dizimo.Domain.Entities;

namespace Dizimo.ViewModels;

/// <summary>
///     DTO para dados do gráfico - permite binding de propriedades nomeadas em Avalonia UI
///     (ValueTuple não suporta binding de Item1, Item2, etc em Avalonia)
/// </summary>
public class GraficoData
{
    public required string Periodo { get; set; }
    public required int Quantidade { get; set; }
    public required string CorHex { get; set; }
}

public partial class MainPageViewModel(
    DashboardService dashboardService,
    AniversariantesExcelService aniversariantesExcelService,
    AniversariantesPdfService aniversariantesPdfService,
    IDialogService dialogService) : ObservableObject
{
    private readonly AniversariantesExcelService _aniversariantesExcelService = aniversariantesExcelService ??
        throw new ArgumentNullException(nameof(aniversariantesExcelService));

    private readonly AniversariantesPdfService _aniversariantesPdfService =
        aniversariantesPdfService ?? throw new ArgumentNullException(nameof(aniversariantesPdfService));

    private readonly DashboardService _dashboardService =
        dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));

    private readonly IDialogService _dialogService =
        dialogService ?? throw new ArgumentNullException(nameof(dialogService));

    private ObservableCollection<Dizimista> _aniversariantes = [];

    private ObservableCollection<GraficoData> _dadosGrafico = [];

    // ...existing code...

    private ObservableCollection<DashboardService.DizimistaPeriodoOfertaData> _dizimistasAgrupadosPeriodo = [];

    private bool _isBusy;

    private bool _isInitialized;

    private int _mesSelecionado = DateTime.Now.Month;

    private string _visualizacaoAtual = "Semana";

    public ObservableCollection<DashboardService.DizimistaPeriodoOfertaData> DizimistasAgrupadosPeriodo
    {
        get => _dizimistasAgrupadosPeriodo;
        set => SetProperty(ref _dizimistasAgrupadosPeriodo, value);
    }

    public ObservableCollection<GraficoData> DadosGrafico
    {
        get => _dadosGrafico;
        set => SetProperty(ref _dadosGrafico, value);
    }


    public ObservableCollection<Dizimista> Aniversariantes
    {
        get => _aniversariantes;
        set => SetProperty(ref _aniversariantes, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string VisualizacaoAtual
    {
        get => _visualizacaoAtual;
        set
        {
            SetProperty(ref _visualizacaoAtual, value);
            OnPropertyChanged();
            OnPropertyChanged(nameof(TextoBotaoAlternarVisualizacao));
        }
    }

    public int MesSelecionado
    {
        get => _mesSelecionado;
        set
        {
            if (SetProperty(ref _mesSelecionado, value))
                // Recarregar aniversariantes quando o m�s mudar
                _ = CarregarAniversariantesAsync();
        }
    }

    public int IndiceMesSelecionado
    {
        get => MesSelecionado - 1;
        set => MesSelecionado = value + 1;
    }

    public ObservableCollection<string> MesesDisponiveis { get; } =
    [
        "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
        "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
    ];

    public static string Titulo => $"Dashboard - {DateTime.Now:dddd, dd 'de' MMMM 'de' yyyy}";

    public string TextoBotaoAlternarVisualizacao =>
        VisualizacaoAtual == "Semana" ? "Visualizar Mês" : "Visualizar Semana Atual";

    /// <summary>
    ///     Inicializa o ViewModel e carrega os dados. Deve ser chamado uma vez quando a view estiver pronta.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized) return;
        _isInitialized = true;

        Debug.WriteLine("[INFO] MainPageViewModel.InitializeAsync() chamado");
        await CarregarDadosAsync();
    }

    [RelayCommand]
    public async Task CarregarDadosAsync()
    {
        try
        {
            IsBusy = true;

            var dados = await _dashboardService.GetDizimistasAgrupadosPorPeriodoAsync();
            DizimistasAgrupadosPeriodo = new ObservableCollection<DashboardService.DizimistaPeriodoOfertaData>(dados);

            // Mapear dados para gráfico com cores - usando GraficoData ao invés de ValueTuple
            var dadosGraficoDtos = dados
                .Select(d => new GraficoData
                {
                    Periodo = d.Periodo,
                    Quantidade = d.Quantidade,
                    CorHex = d.Cor
                })
                .ToList();

            // Define a quantidade máxima para o converter de largura relativa
            if (dadosGraficoDtos.Any())
            {
                var maxQtd = dadosGraficoDtos.Max(d => d.Quantidade);
                RelativeWidthConverter.SetMaxQuantidade(maxQtd);
                Debug.WriteLine($"[GRAFICO] Quantidade máxima definida: {maxQtd}");
            }

            DadosGrafico = new ObservableCollection<GraficoData>(dadosGraficoDtos);
            Debug.WriteLine($"[GRAFICO] Dados do gráfico carregados: {DadosGrafico.Count} períodos");

            await CarregarAniversariantesAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Erro ao carregar dados: {ex.Message}");
            Debug.WriteLine($"[ERRO] {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }


    [RelayCommand]
    private async Task CarregarAniversariantesAsync()
    {
        try
        {
            if (VisualizacaoAtual == "Semana")
            {
                var lista = await _dashboardService.GetAniversariantesSemanasAsync();
                Aniversariantes = new ObservableCollection<Dizimista>(lista);
            }
            else
            {
                var lista = await _dashboardService.GetAniversariantesMesAsync(_mesSelecionado);
                var listaFiltrada = lista.Where(d => d.DataNascimento.Month == MesSelecionado).ToList();
                Aniversariantes = new ObservableCollection<Dizimista>(listaFiltrada);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Erro ao carregar aniversariantes: {ex.Message}");
            Debug.WriteLine($"[ERRO] {ex}");
        }
    }

    [RelayCommand]
    private async Task AlternarVisualizacaoAsync()
    {
        VisualizacaoAtual = VisualizacaoAtual == "Semana" ? "Mês" : "Semana";

        if (VisualizacaoAtual == "Mês")
        {
            MesSelecionado = DateTime.Now.Month;
            OnPropertyChanged(nameof(IndiceMesSelecionado));
        }

        await CarregarAniversariantesAsync();
    }

    [RelayCommand]
    private async Task ExportarAniversariantesAsync()
    {
        try
        {
            if (Aniversariantes.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Exportação", "Não há aniversariantes para exportar");
                return;
            }

            if (_aniversariantesExcelService == null)
            {
                await _dialogService.ShowErrorAsync("Serviço de Excel não está disponível");
                return;
            }

            var fileName = VisualizacaoAtual == "Semana"
                ? $"aniversariantes_semana_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                : $"aniversariantes_mes_{MesesDisponiveis[MesSelecionado - 1]}_{DateTime.Now:yyyyMMdd_HHmms}.xlsx";

            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
                desktop.MainWindow != null)
                try
                {
                    var storageProvider = desktop.MainWindow.StorageProvider;

                    if (storageProvider != null)
                    {
                        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                        {
                            Title = "Salvar Aniversariantes",
                            DefaultExtension = "xlsx",
                            FileTypeChoices = new[]
                                { new FilePickerFileType("Arquivo Excel") { Patterns = new[] { "*.xlsx" } } },
                            SuggestedFileName = fileName
                        });

                        if (file != null)
                        {
                            var excelStream = _aniversariantesExcelService.Exportar(Aniversariantes);
                            await using var fileStream = await file.OpenWriteAsync();
                            await fileStream.WriteAsync(excelStream.ToArray());

                            var filePath = file.Path.LocalPath;
                            await _dialogService.ShowSuccessAsync(
                                $"Aniversariantes exportados com sucesso!\n\nLocalização: {filePath}");

                            try
                            {
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = filePath,
                                    UseShellExecute = true
                                });
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"[AVISO] Não foi possível abrir o arquivo: {ex.Message}");
                            }

                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ERRO] Erro ao abrir file picker: {ex.Message}");
                    await _dialogService.ShowErrorAsync($"Erro ao abrir file picker: {ex.Message}");
                    return;
                }

            await _dialogService.ShowErrorAsync("StorageProvider não disponível");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Erro ao exportar: {ex.Message}");
            Debug.WriteLine($"[ERRO] {ex}");
        }
    }


    [RelayCommand]
    public async Task ImprimirAniversariantesAsync()
    {
        try
        {
            if (Aniversariantes.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Impressão", "Não há aniversariantes para imprimir");
                return;
            }

            var htmlStream = await _aniversariantesPdfService.ImprimirAsync([.. Aniversariantes]);

            var fileName = VisualizacaoAtual == "Semana"
                ? $"aniversariantes_semana_{DateTime.Now:yyyyMMdd_HHmmss}.html"
                : $"aniversariantes_mes_{MesesDisponiveis[MesSelecionado - 1]}_{DateTime.Now:yyyyMMdd_HHmms}.html";

            var tempPath = Path.Combine(Path.GetTempPath(), fileName);

            await File.WriteAllBytesAsync(tempPath, htmlStream.ToArray());

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AVISO] Não foi possível abrir o arquivo: {ex.Message}");
                await _dialogService.ShowErrorAsync($"Erro ao abrir arquivo: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Erro ao gerar relatório: {ex.Message}");
            Debug.WriteLine($"[ERRO] {ex}");
        }
    }
}