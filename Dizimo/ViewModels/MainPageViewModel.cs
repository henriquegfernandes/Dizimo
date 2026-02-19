using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Application.Dashboard;
using Dizimo.Domain.Entities;
using Dizimo.Services;
using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Maui.Storage;

namespace Dizimo.ViewModels;

public partial class MainPageViewModel(DashboardService dashboardService, AniversariantesExcelService aniversariantesExcelService, AniversariantesPdfService aniversariantesPdfService) : ObservableObject
{
    private readonly DashboardService _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
    private readonly AniversariantesExcelService _aniversariantesExcelService = aniversariantesExcelService ?? throw new ArgumentNullException(nameof(aniversariantesExcelService));
    private readonly AniversariantesPdfService _aniversariantesPdfService = aniversariantesPdfService ?? throw new ArgumentNullException(nameof(aniversariantesPdfService));

    private ObservableCollection<DashboardService.DizimistaPeriodoOfertaData> _dizimistasAgrupadosPeriodo = [];

    private ObservableCollection<Dizimista> _aniversariantes = [];

    private bool _isBusy;

    private string _visualizacaoAtual = "Semana";

    private int _mesSelecionado = DateTime.Now.Month;

    public ObservableCollection<DashboardService.DizimistaPeriodoOfertaData> DizimistasAgrupadosPeriodo
    {
        get => _dizimistasAgrupadosPeriodo;
        set => SetProperty(ref _dizimistasAgrupadosPeriodo, value);
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
            OnPropertyChanged(nameof(VisualizacaoAtual));
            OnPropertyChanged(nameof(TextoBotaoAlternarVisualizacao));
        }
    }

    public int MesSelecionado
    {
        get => _mesSelecionado;
        set
        {
            if (SetProperty(ref _mesSelecionado, value))
            {
                // Recarregar aniversariantes quando o mês mudar
                _ = CarregarAniversariantesAsync();
            }
        }
    }

    public int IndiceMesSelecionado
    {
        get => MesSelecionado - 1;
        set
        {
            MesSelecionado = value + 1;
        }
    }

    public static List<string> MesesDisponiveis =>
    [
        "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
        "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
    ];

    public static string Titulo => $"Dashboard - {DateTime.Now:dddd, dd 'de' MMMM 'de' yyyy}";

    [RelayCommand]
    public async Task CarregarDadosAsync()
    {
        try
        {
            IsBusy = true;

            // Carregar gráfico de dizimistas por período de oferta
            var dados = await _dashboardService.GetDizimistasAgrupadosPorPeriodoAsync();
            DizimistasAgrupadosPeriodo = new ObservableCollection<DashboardService.DizimistaPeriodoOfertaData>(dados);

            // Carregar aniversariantes
            await CarregarAniversariantesAsync();
        }
        catch (Exception ex)
        {
            var mainPage = GetMainPage();
            if (mainPage != null)
            {
                await mainPage.DisplayAlertAsync("Erro", $"Erro ao carregar dashboard: {ex.Message}", "OK");
            }
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
                // Quando em visualização por mês, filtrar pelo mês selecionado
                var lista = await _dashboardService.GetAniversariantesMesAsync(_mesSelecionado);
                var listaFiltrada = lista.Where(d => d.DataNascimento.Month == MesSelecionado).ToList();
                Aniversariantes = new ObservableCollection<Dizimista>(listaFiltrada);
            }
        }
        catch (Exception ex)
        {
            var mainPage = GetMainPage();
            if (mainPage != null)
            {
                await mainPage.DisplayAlertAsync("Erro ao Carregar Aniversariantes", $"Erro: {ex.Message}", "OK");
            }
        }
    }

    [RelayCommand]
    private async Task AlternarVisualizacaoAsync()
    {
        VisualizacaoAtual = VisualizacaoAtual == "Semana" ? "Mês" : "Semana";

        // Resetar mês selecionado para o mês atual quando mudar para visualização por mês
        if (VisualizacaoAtual == "Mês")
        {
            MesSelecionado = DateTime.Now.Month;
            // Notificar que o índice também mudou
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
                var page = GetMainPage();
                if (page != null)
                {
                    await page.DisplayAlertAsync("Aviso", "Nenhum aniversariante para exportar neste período.", "OK");
                }
                return;
            }

            if (_aniversariantesExcelService == null)
            {
                var mainPageNull = GetMainPage();
                if (mainPageNull != null)
                {
                    await mainPageNull.DisplayAlertAsync("Erro", "Serviço de exportação não está disponível.", "OK");
                }
                return;
            }

            var fileName = VisualizacaoAtual == "Semana"
                ? $"aniversariantes_semana_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                : $"aniversariantes_mes_{MesesDisponiveis[MesSelecionado - 1]}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            var excelStream = _aniversariantesExcelService.Exportar(Aniversariantes);

#if WINDOWS
            var result = await FileSaver.Default.SaveAsync(fileName, excelStream, CancellationToken.None);

            if (result.IsSuccessful)
            {
                var mainPage = GetMainPage();
                if (mainPage != null)
                {
                    await mainPage.DisplayAlertAsync("Exportação",
                        $"Aniversariantes exportados com sucesso!", "OK");
                }
            }
#else
            var downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");

            if (!Directory.Exists(downloadsPath))
            {
                Directory.CreateDirectory(downloadsPath);
            }

            var filePath = Path.Combine(downloadsPath, fileName);
            await File.WriteAllBytesAsync(filePath, excelStream.ToArray());

            var mainPageSuccess = GetMainPage();
            if (mainPageSuccess != null)
            {
                await mainPageSuccess.DisplayAlertAsync("Exportação",
                    $"Aniversariantes exportados com sucesso!\n\nLocalização: {filePath}", "OK");
            }
#endif
        }
        catch (Exception ex)
        {
            var mainPageError = GetMainPage();
            if (mainPageError != null)
            {
                await mainPageError.DisplayAlertAsync("Erro", $"Erro ao exportar: {ex.Message}", "OK");
            }
        }
    }

    [RelayCommand]
    public async Task ImprimirAniversariantesAsync()
    {
        try
        {
            if (Aniversariantes.Count == 0)
            {
                var page = GetMainPage();
                if (page != null)
                {
                    await page.DisplayAlertAsync("Aviso", "Nenhum aniversariante para imprimir neste período.", "OK");
                }
                return;
            }

            // Usar a coleção de aniversariantes já carregada e ordenada na página
            var htmlStream = await _aniversariantesPdfService.ImprimirAsync([.. Aniversariantes]);

            // Salvar em arquivo temporário
            var fileName = VisualizacaoAtual == "Semana"
                ? $"aniversariantes_semana_{DateTime.Now:yyyyMMdd_HHmmss}.html"
                : $"aniversariantes_mes_{MesesDisponiveis[MesSelecionado - 1]}_{DateTime.Now:yyyyMMdd_HHmmss}.html";

            var tempPath = Path.Combine(Path.GetTempPath(), fileName);

            await File.WriteAllBytesAsync(tempPath, htmlStream.ToArray());

            // Abrir arquivo automaticamente no navegador padrão
            // O arquivo HTML possui onload="window.print()" que abre o diálogo de impressão automaticamente
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AVISO] Não foi possível abrir o arquivo: {ex.Message}");
                var mainPage = GetMainPage();
                if (mainPage != null)
                    await mainPage.DisplayAlertAsync("Aviso", "Não foi possível abrir o navegador. Verifique se possui um navegador padrão configurado.", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao gerar relatório: {ex.Message}");
            var mainPageError = GetMainPage();
            if (mainPageError != null)
            {
                await mainPageError.DisplayAlertAsync("Erro", $"Erro ao gerar relatório: {ex.Message}", "OK");
            }
        }
    }

    private static Page? GetMainPage()
    {
        var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
        return windows is { Count: > 0 } ? windows[0].Page : null;
    }

    public string TextoBotaoAlternarVisualizacao
    {
        get
        {
            return VisualizacaoAtual == "Semana" ? "Visualizar Mês" : "Visualizar Semana Atual";
        }
    }
}
