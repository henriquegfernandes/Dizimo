using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;
using Dizimo.Application.Ofertas.Commands;
using Dizimo.Application.Ofertas.Handlers;
using Dizimo.Application.Ofertas.Queries;
using Dizimo.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using CommunityToolkit.Maui.Storage;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;
using Dizimo.Domain.Models;

namespace Dizimo.ViewModels;

public partial class OfertaListViewModel : ObservableObject
{
    private readonly GetOfertaHandlers _getHandlers;
    private readonly DeleteOfertaHandler _deleteHandler;
    private readonly OfertaExcelService _excelService;
    private readonly IUnitOfWork _unitOfWork;

    private static readonly FilePickerFileType ExcelFileType = new(new Dictionary<DevicePlatform, IEnumerable<string>>
    {
        { DevicePlatform.WinUI, new[] { ".xlsx" } },
        { DevicePlatform.macOS, new[] { ".xlsx" } },
        { DevicePlatform.iOS, new[] { "com.microsoft.excel.xlsx" } },
        { DevicePlatform.Android, new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" } },
    });

    private ObservableCollection<Oferta> _ofertas = [];
    public ObservableCollection<Oferta> Ofertas
    {
        get => _ofertas;
        private set => SetProperty(ref _ofertas, value);
    }

    private string _filtroNome = string.Empty;
    public string FiltroNome
    {
        get => _filtroNome;
        set
        {
            if (SetProperty(ref _filtroNome, value))
            {
                ResetarPaginacao();
                _ = CarregarOfertasAsync();
            }
        }
    }

    private DateTime? _filtroDataInicio = DateTime.Today;
    public DateTime? FiltroDataInicio
    {
        get => _filtroDataInicio;
        set 
        { 
            if (SetProperty(ref _filtroDataInicio, value))
            {
                ResetarPaginacao();
                _ = CarregarOfertasAsync();
            }
        }
    }

    private DateTime? _filtroDataFim = DateTime.Today;
    public DateTime? FiltroDataFim
    {
        get => _filtroDataFim;
        set 
        { 
            if (SetProperty(ref _filtroDataFim, value))
            {
                ResetarPaginacao();
                _ = CarregarOfertasAsync();
            }
        }
    }

    private Oferta? _selectedOferta;
    public Oferta? SelectedOferta
    {
        get => _selectedOferta;
        set => SetProperty(ref _selectedOferta, value);
    }

    private int _paginaAtual = 1;
    private const int TAMANHO_PAGINA = 20;
    private bool _carregandoMais = false;
    private int _totalPaginas = 1;

    private bool _temProxima = false;
    public bool TemProxima
    {
        get => _temProxima;
        private set => SetProperty(ref _temProxima, value);
    }

    private decimal _valorTotal;
    public decimal ValorTotal
    {
        get => _valorTotal;
        private set => SetProperty(ref _valorTotal, value);
    }

    private int _totalOfertas = 0;
    public int TotalOfertas
    {
        get => _totalOfertas;
        private set => SetProperty(ref _totalOfertas, value);
    }

    public string TextoResultados
    {
        get => $"{Ofertas.Count} de {TotalOfertas} resultados";
    }

    public List<string> TiposPagamento { get; } = [ "Todos", "PIX", "Dinheiro", "Cartao" ];

    private string _filtroTipoPagamento = "Todos";
    public string FiltroTipoPagamento
    {
        get => _filtroTipoPagamento;
        set
        {
            if (SetProperty(ref _filtroTipoPagamento, value))
            {
                ResetarPaginacao();
                _ = CarregarOfertasAsync();
            }
        }
    }

    private ObservableCollection<Oferta> _ofertasSelecionadas = [];
    public ObservableCollection<Oferta> OfertasSelecionadas
    {
        get => _ofertasSelecionadas;
        set
        {
            _ofertasSelecionadas?.CollectionChanged -= OfertasSelecionadas_CollectionChanged;
            SetProperty(ref _ofertasSelecionadas, value);
            _ofertasSelecionadas?.CollectionChanged += OfertasSelecionadas_CollectionChanged;
            OnPropertyChanged(nameof(OfertasSelecionadas));
            OnPropertyChanged(nameof(OfertasSelecionadas.Count));
        }
    }

    private void OfertasSelecionadas_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(OfertasSelecionadas));
        OnPropertyChanged(nameof(OfertasSelecionadas.Count));
    }

    public OfertaListViewModel(
        GetOfertaHandlers getHandlers,
        DeleteOfertaHandler deleteHandler,
        OfertaExcelService excelService,
        IUnitOfWork unitOfWork)
    {
        _getHandlers = getHandlers ?? throw new ArgumentNullException(nameof(getHandlers));
        _deleteHandler = deleteHandler ?? throw new ArgumentNullException(nameof(deleteHandler));
        _excelService = excelService ?? throw new ArgumentNullException(nameof(excelService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        Ofertas = [];
    }

    private void ResetarPaginacao()
    {
        _paginaAtual = 1;
        TemProxima = false;
        Ofertas.Clear();
    }

    [RelayCommand]
    public async Task CarregarOfertasAsync()
    {
        ResetarPaginacao();
        await _unitOfWork.ClearDbContextAsync();
        await CarregarProximaPaginaAsync();
    }

    [RelayCommand]
    public async Task CarregarMaisOfertas()
    {
        if (_carregandoMais || !TemProxima) return;
        await CarregarProximaPaginaAsync();
    }

    private async Task CarregarProximaPaginaAsync()
    {
        if (_carregandoMais) return;
        _carregandoMais = true;

        try
        {
            var result = await _getHandlers.Handle(new GetAllOfertasPaginatedQuery(
                _paginaAtual, 
                TAMANHO_PAGINA,
                FiltroDataInicio,
                FiltroDataFim,
                FiltroTipoPagamento,
                FiltroNome));

            _totalPaginas = result.TotalPages;
            TotalOfertas = result.TotalCount;

            foreach (var oferta in result.Items)
            {
                Ofertas.Add(oferta);
            }

            OnPropertyChanged(nameof(TextoResultados));
            _paginaAtual++;
            TemProxima = _paginaAtual <= _totalPaginas;
            await AtualizarValorTotal();
        }
        finally
        {
            _carregandoMais = false;
        }
    }

    private async Task AtualizarValorTotal()
    {
        // Calcular o total de TODAS as ofertas filtradas (não apenas as carregadas)
        ValorTotal = await _unitOfWork.Ofertas.GetTotalValorAsync(
            FiltroDataInicio,
            FiltroDataFim,
            FiltroTipoPagamento != "Todos" ? FiltroTipoPagamento : null,
            FiltroNome);
        OnPropertyChanged(nameof(TextoResultados));
    }

    [RelayCommand]
    public async Task AplicarFiltros()
    {
        ResetarPaginacao();
        await CarregarOfertasAsync();
    }

    [RelayCommand]
    public async Task LimparFiltros()
    {
        FiltroNome = string.Empty;
        FiltroDataInicio = null;
        FiltroDataFim = null;
        FiltroTipoPagamento = "Todos";
        ResetarPaginacao();
        await CarregarOfertasAsync();
    }

    [RelayCommand]
    public static async Task NovaOfertaAsync()
    {
        await Shell.Current.GoToAsync("oferta-cadastro");
    }

    [RelayCommand]
    public static async Task EditarOfertaAsync(Oferta oferta)
    {
        if (oferta != null)
        {
            var navigationParameter = new Dictionary<string, object>
            {
                { "id", oferta.Id.ToString() }
            };
            await Shell.Current.GoToAsync("oferta-cadastro", navigationParameter);
        }
    }

    [RelayCommand]
    public async Task ExcluirOfertaAsync(Oferta oferta)
    {
        if (oferta == null) return;

        var mainPage = GetMainPage();
        if (mainPage == null) return;

        bool confirm = await mainPage.DisplayAlertAsync(
            "Confirmação",
            $"Deseja excluir a oferta de valor {oferta.Valor:C} em {oferta.Data:dd/MM/yyyy}?",
            "Sim",
            "Não"
        );

        if (confirm)
        {
            try
            {
                await _deleteHandler.Handle(new DeleteOfertaCommand(oferta.Id));
                await CarregarOfertasAsync();
                await mainPage.DisplayAlertAsync("Sucesso", "Oferta excluída com sucesso.", "OK");
            }
            catch (Exception ex)
            {
                await mainPage.DisplayAlertAsync("Erro", $"Erro ao excluir: {ex.Message}", "OK");
            }
        }
    }

    [RelayCommand]
    public async Task ExcluirOfertasSelecionadasAsync()
    {
        if (OfertasSelecionadas.Count == 0) return;
        var mainPage = GetMainPage();
        if (mainPage != null)
        {
            bool confirm = await mainPage.DisplayAlertAsync("Confirmação", $"Deseja excluir {OfertasSelecionadas.Count} oferta(s)?", "Sim", "Não");
            if (!confirm) return;
        }
        foreach (var oferta in OfertasSelecionadas.ToList())
        {
            await _deleteHandler.Handle(new DeleteOfertaCommand(oferta.Id));
        }
        await CarregarOfertasAsync();
        OfertasSelecionadas.Clear();
    }

    [RelayCommand]
    public async Task ExportarAsync()
    {
        try
        {
            // Trazer TODAS as ofertas do banco COM OS FILTROS aplicados
            var todasOfertas = await _unitOfWork.Ofertas.GetAllAsync();
            var excelStream = await _excelService.ExportarAsync(
                [..todasOfertas],
                FiltroDataInicio,
                FiltroDataFim,
                FiltroTipoPagamento,
                FiltroNome);
            var fileName = $"ofertas_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

#if WINDOWS
            var result = await FileSaver.Default.SaveAsync(fileName, excelStream, CancellationToken.None);

            if (result.IsSuccessful)
            {
                System.Diagnostics.Debug.WriteLine($"[INFO] Arquivo exportado para: {result.FilePath}");
                
                // Abrir arquivo automaticamente
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = result.FilePath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[AVISO] Não foi possível abrir o arquivo: {ex.Message}");
                }
                
                var mainPage = GetMainPage();
                if (mainPage != null)
                    await mainPage.DisplayAlertAsync("Exportação", $"Planilha de ofertas exportada com sucesso!", "OK");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[INFO] Exportação cancelada pelo usuário");
            }

#else
            var downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");

            if (!Directory.Exists(downloadsPath))
                Directory.CreateDirectory(downloadsPath);

            var filePath = Path.Combine(downloadsPath, fileName);
            await File.WriteAllBytesAsync(filePath, excelStream.ToArray());

            // Abrir arquivo automaticamente
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AVISO] Não foi possível abrir o arquivo: {ex.Message}");
            }

            var mainPage = GetMainPage();
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Exportação", $"Planilha de ofertas exportada com sucesso!\n\nLocalização: {filePath}", "OK");
#endif
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao exportar: {ex.Message}");
            var mainPage = GetMainPage();
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Erro", $"Erro ao exportar: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public static async Task BaixarModeloAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[INFO] BaixarModeloAsync iniciado");

            var excelStream = OfertaExcelService.GerarModelo();
            var fileName = "ofertas_modelo.xlsx";

#if WINDOWS
            var result = await FileSaver.Default.SaveAsync(fileName, excelStream, CancellationToken.None);

            if (result.IsSuccessful)
            {
                System.Diagnostics.Debug.WriteLine($"[INFO] Arquivo modelo salvo em: {result.FilePath}");
                var mainPage = GetMainPage();
                if (mainPage != null)
                    await mainPage.DisplayAlertAsync("Modelo Baixado", $"Planilha modelo baixada com sucesso!", "OK");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[INFO] Download do modelo cancelado pelo usuário");
            }
#else
            var downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");

            if (!Directory.Exists(downloadsPath))
                Directory.CreateDirectory(downloadsPath);

            var filePath = Path.Combine(downloadsPath, fileName);
            await File.WriteAllBytesAsync(filePath, excelStream.ToArray());

            var mainPage = GetMainPage();
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Modelo Baixado", $"Planilha modelo baixada com sucesso!\n\nLocalização: {filePath}", "OK");
#endif
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao baixar modelo: {ex.Message}");
            var mainPage = GetMainPage();
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Erro", $"Erro ao baixar modelo: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task ImportarAsync()
    {
        try
        {
            var mainPage = GetMainPage();
            
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Selecione um arquivo Excel",
                FileTypes = ExcelFileType
            });

            if (result == null)
            {
                System.Diagnostics.Debug.WriteLine("[INFO] Importação cancelada pelo usuário");
                return;
            }

            var excelBytes = await File.ReadAllBytesAsync(result.FullPath);
            var importResult = await _excelService.ImportarAsync(excelBytes);
            
            foreach (var o in importResult.OfertasImportadas)
            {
                await _unitOfWork.Ofertas.AddAsync(o);
            }
            await _unitOfWork.SaveChangesAsync();
            await CarregarOfertasAsync();
            
            var mensagem = $"Importação concluída!\n\n";
            mensagem += $"? Ofertas importadas: {importResult.OfertasImportadas.Count}\n";
            
            if (importResult.Erros.Count > 0)
            {
                mensagem += $"\n? Ofertas não importadas: {importResult.Erros.Count}\n\n";
                mensagem += "Erros:\n";
                
                var errosExibir = importResult.Erros.Take(10).ToList();
                foreach (var erro in errosExibir)
                {
                    mensagem += $"• {erro}\n";
                }
                
                if (importResult.Erros.Count > 10)
                {
                    mensagem += $"\n... e mais {importResult.Erros.Count - 10} erro(s)";
                }
            }
            
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Resultado da Importação", mensagem, "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao importar: {ex.Message}");
            var mainPage = GetMainPage();
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Erro", $"Erro ao importar: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task ImprimirAsync()
    {
        try
        {
            // Carregar TODAS as ofertas COM OS FILTROS aplicados, mantendo a mesma ordem da página
            var todasOfertas = await CarregarTodasOfertasComFiltrosAsync();

            var pdfService = new OfertaPdfService(_unitOfWork);
            var htmlStream = await pdfService.ImprimirAsync(
                todasOfertas,
                FiltroDataInicio,
                FiltroDataFim,
                FiltroTipoPagamento,
                FiltroNome);

            // Salvar em arquivo temporário
            var fileName = $"ofertas_{DateTime.Now:yyyyMMdd_HHmmss}.html";
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
            var mainPage = GetMainPage();
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Erro", $"Erro ao gerar relatório: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Carrega TODAS as ofertas com os filtros aplicados, mantendo a mesma ordem da paginação
    /// </summary>
    private async Task<List<Oferta>> CarregarTodasOfertasComFiltrosAsync()
    {
        var todasOfertas = new List<Oferta>();
        int pageNumber = 1;
        int totalPages = 1;

        while (pageNumber <= totalPages)
        {
            var result = await _getHandlers.Handle(new GetAllOfertasPaginatedQuery(
                pageNumber,
                TAMANHO_PAGINA,
                FiltroDataInicio,
                FiltroDataFim,
                FiltroTipoPagamento,
                FiltroNome));

            // Aplicar filtro de nome client-side se necessário
            var items = result.Items.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(FiltroNome))
            {
                items = items.Where(o =>
                {
                    var dizimista = _unitOfWork.Dizimistas.GetByIdAsync(o.DizimistaId).Result;
                    return dizimista != null && (
                        dizimista.Nome.Contains(FiltroNome, StringComparison.OrdinalIgnoreCase) ||
                        dizimista.NumeroCadastro.ToString().Contains(FiltroNome));
                });
            }

            todasOfertas.AddRange(items);
            totalPages = result.TotalPages;
            pageNumber++;
        }

        return todasOfertas;
    }

    private static Page? GetMainPage()
    {
        var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
        return windows is { Count: > 0 } ? windows[0].Page : null;
    }
}
