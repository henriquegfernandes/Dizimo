using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Application.Ofertas.Commands;
using Dizimo.Application.Ofertas.Handlers;
using Dizimo.Application.Ofertas.Queries;
using Dizimo.Application.Reporting.Services;
using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;

namespace Dizimo.ViewModels;

public partial class OfertaListViewModel : ObservableObject, INavigationAware
{
    private const int TAMANHO_PAGINA = 20;
    private readonly DeleteOfertaHandler _deleteHandler;
    private readonly OfertaExcelService _excelService;
    private readonly IFilterCacheService _filterCacheService;
    private readonly GetOfertaHandlers _getHandlers;
    private readonly INavigationService _navigationService;
    private readonly IUnitOfWork _unitOfWork;
    private bool _carregandoMais;

    private DateTime? _filtroDataFim;

    private DateTime? _filtroDataInicio;

    private string _filtroNome = string.Empty;

    private string _filtroTipoPagamento = "Todos";


    private ObservableCollection<Oferta> _ofertas = [];

    private ObservableCollection<Oferta> _ofertasSelecionadas = [];

    private int _paginaAtual = 1;

    private Oferta? _selectedOferta;

    private bool _temProxima;

    private int _totalOfertas;
    private int _totalPaginas = 1;

    private decimal _valorTotal;

    public OfertaListViewModel(
        GetOfertaHandlers getHandlers,
        DeleteOfertaHandler deleteHandler,
        OfertaExcelService excelService,
        IUnitOfWork unitOfWork,
        INavigationService navigationService,
        IFilterCacheService filterCacheService)
    {
        _getHandlers = getHandlers ?? throw new ArgumentNullException(nameof(getHandlers));
        _deleteHandler = deleteHandler ?? throw new ArgumentNullException(nameof(deleteHandler));
        _excelService = excelService ?? throw new ArgumentNullException(nameof(excelService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _filterCacheService = filterCacheService ?? throw new ArgumentNullException(nameof(filterCacheService));
        Ofertas = [];

        // Conectar o handler de CollectionChanged ao inicializar
        // Isso garante que toda vez que um checkbox for marcado/desmarcado, 
        // o TextoBotaoSelecao será reavaliado
        OfertasSelecionadas = [];

        // NÃO inicializar os filtros aqui - deixar para OnNavigatedTo
        // para que o cache possa ser restaurado corretamente
    }

    public ObservableCollection<Oferta> Ofertas
    {
        get => _ofertas;
        private set => SetProperty(ref _ofertas, value);
    }

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

    public Oferta? SelectedOferta
    {
        get => _selectedOferta;
        set => SetProperty(ref _selectedOferta, value);
    }

    public bool TemProxima
    {
        get => _temProxima;
        private set => SetProperty(ref _temProxima, value);
    }

    public decimal ValorTotal
    {
        get => _valorTotal;
        private set => SetProperty(ref _valorTotal, value);
    }

    public int TotalOfertas
    {
        get => _totalOfertas;
        private set => SetProperty(ref _totalOfertas, value);
    }

    public string TextoResultados => $"{Ofertas.Count} de {TotalOfertas} resultados";

    public List<string> TiposPagamento { get; } = ["Todos", "PIX", "Dinheiro", "Cartão"];

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

    public ObservableCollection<Oferta> OfertasSelecionadas
    {
        get => _ofertasSelecionadas;
        set
        {
            _ofertasSelecionadas?.CollectionChanged -= OfertasSelecionadas_CollectionChanged;
            SetProperty(ref _ofertasSelecionadas, value);
            _ofertasSelecionadas?.CollectionChanged += OfertasSelecionadas_CollectionChanged;
            OnPropertyChanged();
            OnPropertyChanged(nameof(OfertasSelecionadas.Count));
            OnPropertyChanged(nameof(TextoBotaoSelecao));
        }
    }

    public string TextoBotaoSelecao =>
        OfertasSelecionadas.Count == Ofertas.Count ? "Limpar Seleção" : "Selecionar Todos";

    public bool PodeSelecionar => Ofertas.Count > 0;

    /// <summary>
    ///     Implementação de INavigationAware - Recarrega ofertas quando volta para esta página
    ///     Restaura os filtros do cache se não houver parâmetro clearCache
    /// </summary>
    public void OnNavigatedTo(NavigationParameters parameters)
    {
        // Se foi passado clearCache=true, limpar o cache e usar filtros padrão
        if (parameters != null && parameters.TryGetValue("clearCache", out var clearCacheObj) &&
            clearCacheObj is bool clearCache && clearCache)
        {
            _filterCacheService.ClearOfertaListFilters();
            Debug.WriteLine("[Navigation] Cache de filtros foi limpo");

            // Usar filtros padrão
            var hoje = DateTime.Today;
            _filtroDataInicio = hoje;
            _filtroDataFim = hoje;
            _filtroNome = string.Empty;
            _filtroTipoPagamento = "Todos";
        }
        else
        {
            // Tentar restaurar os filtros do cache
            var cachedFilters = _filterCacheService.GetOfertaListFilters();
            if (cachedFilters.HasValue)
            {
                var (dataInicio, dataFim, nome, tipoPagamento) = cachedFilters.Value;

                Debug.WriteLine("[Navigation] Restaurando filtros do cache");

                // Restaurar os filtros
                _filtroDataInicio = dataInicio;
                _filtroDataFim = dataFim;
                _filtroNome = nome;
                _filtroTipoPagamento = tipoPagamento;
            }
            else
            {
                // Se não houver cache, usar filtros padrão
                Debug.WriteLine("[Navigation] Sem cache, usando filtros padrão");
                var hoje = DateTime.Today;
                _filtroDataInicio = hoje;
                _filtroDataFim = hoje;
                _filtroNome = string.Empty;
                _filtroTipoPagamento = "Todos";
            }
        }

        // Notificar que as propriedades mudaram
        OnPropertyChanged(nameof(FiltroDataInicio));
        OnPropertyChanged(nameof(FiltroDataFim));
        OnPropertyChanged(nameof(FiltroNome));
        OnPropertyChanged(nameof(FiltroTipoPagamento));

        // Agora carregar os dados com os filtros restaurados/padrão
        _ = CarregarOfertasAsync();
    }

    /// <summary>
    ///     Implementação de INavigationAware - Salva o estado ao sair da página
    /// </summary>
    public void OnNavigatedFrom()
    {
        // Salvar os filtros atuais no cache
        _filterCacheService.SaveOfertaListFilters(FiltroDataInicio, FiltroDataFim, FiltroNome, FiltroTipoPagamento);
        Debug.WriteLine("[Navigation] Filtros salvos no cache");
    }

    private void OfertasSelecionadas_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(OfertasSelecionadas));
        OnPropertyChanged(nameof(OfertasSelecionadas.Count));
        OnPropertyChanged(nameof(TextoBotaoSelecao));
    }

    private void ResetarPaginacao()
    {
        _paginaAtual = 1;
        TemProxima = false;
        Ofertas.Clear();
        OfertasSelecionadas.Clear();
        OnPropertyChanged(nameof(PodeSelecionar));
        OnPropertyChanged(nameof(TextoBotaoSelecao));
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

            foreach (var oferta in result.Items) Ofertas.Add(oferta);

            OnPropertyChanged(nameof(TextoResultados));
            OnPropertyChanged(nameof(PodeSelecionar));
            OnPropertyChanged(nameof(TextoBotaoSelecao));
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
    public async Task NovaOfertaAsync()
    {
        try
        {
            _navigationService.Navigate("oferta-cadastro");
            Debug.WriteLine("[NAV] Navegado para Nova Oferta");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERRO] Erro ao navegar para nova oferta: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task EditarOfertaAsync(Oferta oferta)
    {
        try
        {
            if (oferta != null)
            {
                var parameters = new NavigationParameters();
                parameters.Add("id", oferta.Id);
                _navigationService.Navigate("oferta-cadastro", parameters);
                Debug.WriteLine($"[NAV] Editando oferta: {oferta.Id}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERRO] Erro ao editar oferta: {ex.Message}");
        }

        await Task.CompletedTask;
    }

    [RelayCommand]
    public async Task ExcluirOfertaAsync(Oferta oferta)
    {
        if (oferta == null) return;

        try
        {
            await _deleteHandler.Handle(new DeleteOfertaCommand(oferta.Id));
            await CarregarOfertasAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERRO] Erro ao excluir: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task ExcluirOfertasSelecionadasAsync()
    {
        if (OfertasSelecionadas.Count == 0) return;

        foreach (var oferta in OfertasSelecionadas.ToList())
            await _deleteHandler.Handle(new DeleteOfertaCommand(oferta.Id));
        await CarregarOfertasAsync();
        OfertasSelecionadas.Clear();
    }

    [RelayCommand]
    public void SelecionarTodos()
    {
        if (OfertasSelecionadas.Count == Ofertas.Count)
        {
            OfertasSelecionadas.Clear();
        }
        else
        {
            OfertasSelecionadas.Clear();
            foreach (var oferta in Ofertas) OfertasSelecionadas.Add(oferta);
        }

        OnPropertyChanged(nameof(TextoBotaoSelecao));
    }

    [RelayCommand]
    public async Task ExportarAsync()
    {
        try
        {
            var todasOfertas = await _unitOfWork.Ofertas.GetAllAsync();
            var excelStream = await _excelService.ExportarAsync(
                [.. todasOfertas],
                FiltroDataInicio,
                FiltroDataFim,
                FiltroTipoPagamento,
                FiltroNome);
            var fileName = $"ofertas_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            if (Avalonia.Application.Current?.ApplicationLifetime is ClassicDesktopStyleApplicationLifetime desktop &&
                desktop.MainWindow != null)
                try
                {
                    var storageProvider = desktop.MainWindow.StorageProvider;
                    if (storageProvider != null)
                    {
                        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                        {
                            Title = "Salvar Planilha de Ofertas",
                            DefaultExtension = "xlsx",
                            FileTypeChoices = new[]
                                { new FilePickerFileType("Arquivo Excel") { Patterns = new[] { "*.xlsx" } } },
                            SuggestedFileName = fileName
                        });

                        if (file != null)
                        {
                            await using var fileStream = await file.OpenWriteAsync();
                            await fileStream.WriteAsync(excelStream.ToArray());
                            Debug.WriteLine($"[INFO] Arquivo exportado para: {file.Path}");

                            try
                            {
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = file.Path.ToString(),
                                    UseShellExecute = true
                                });
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"[AVISO] Não foi possível abrir o arquivo: {ex.Message}");
                            }
                        }
                        else
                        {
                            Debug.WriteLine("[INFO] Exportação cancelada pelo usuário");
                        }

                        return;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ERRO] Erro ao usar file picker: {ex.Message}");
                }

            // Fallback: salvar em Downloads (apenas se file picker não estiver disponível)
            Debug.WriteLine("[INFO] Usando fallback: salvando em Downloads");
            var downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");

            if (!Directory.Exists(downloadsPath))
                Directory.CreateDirectory(downloadsPath);

            var filePath = Path.Combine(downloadsPath, fileName);
            await File.WriteAllBytesAsync(filePath, excelStream.ToArray());

            Debug.WriteLine($"[INFO] Arquivo exportado para: {filePath}");

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
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERRO] Erro ao exportar: {ex.Message}");
        }
    }

    [RelayCommand]
    public static async Task BaixarModeloAsync()
    {
        try
        {
            Debug.WriteLine("[INFO] BaixarModeloAsync iniciado");

            var excelStream = OfertaExcelService.GerarModelo();
            var fileName = "ofertas_modelo.xlsx";

            var downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");

            if (!Directory.Exists(downloadsPath))
                Directory.CreateDirectory(downloadsPath);

            var filePath = Path.Combine(downloadsPath, fileName);
            await File.WriteAllBytesAsync(filePath, excelStream.ToArray());

            Debug.WriteLine($"[INFO] Arquivo modelo salvo em: {filePath}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERRO] Erro ao baixar modelo: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task ImportarAsync()
    {
        try
        {
            Debug.WriteLine("[INFO] ImportarAsync iniciado");

            // This will need to be called from the View or through a service
            // that has access to the window context
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERRO] Erro ao importar: {ex.Message}");
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
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERRO] Erro ao gerar relatório: {ex.Message}");
        }
    }

    /// <summary>
    ///     Carrega TODAS as ofertas com os filtros aplicados, mantendo a mesma ordem da paginação
    /// </summary>
    private async Task<List<Oferta>> CarregarTodasOfertasComFiltrosAsync()
    {
        var todasOfertas = new List<Oferta>();
        var pageNumber = 1;
        var totalPages = 1;

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
                items = items.Where(o =>
                {
                    var dizimista = _unitOfWork.Dizimistas.GetByIdAsync(o.DizimistaId).Result;
                    return dizimista != null && (
                        dizimista.Nome.Contains(FiltroNome, StringComparison.OrdinalIgnoreCase) ||
                        dizimista.NumeroCadastro.ToString().Contains(FiltroNome));
                });

            todasOfertas.AddRange(items);
            totalPages = result.TotalPages;
            pageNumber++;
        }

        return todasOfertas;
    }
}