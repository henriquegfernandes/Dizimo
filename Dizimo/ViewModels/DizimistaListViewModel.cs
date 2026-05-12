using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Application.Dizimistas.Commands;
using Dizimo.Application.Dizimistas.Handlers;
using Dizimo.Application.Dizimistas.Queries;
using Dizimo.Application.Reporting.Services;
using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;

namespace Dizimo.ViewModels;

public partial class DizimistaListViewModel : ObservableObject, INavigationAware
{
    private const int _tamanho_pagina = 20;
    private readonly DeleteDizimistaHandler _deleteHandler;
    private readonly IDialogService _dialogService;
    private readonly DizimistaExcelService _excelService;
    private readonly IFilterCacheService _filterCacheService;
    private readonly GetDizimistaHandlers _handlers;
    private readonly InativarDizimistaHandler _inativarHandler;
    private readonly INavigationService _navigationService;
    private readonly IUnitOfWork _unitOfWork;
    private bool _carregandoMais;


    private ObservableCollection<Dizimista> _dizimistas = [];

    private ObservableCollection<Dizimista> _dizimistasSelecionados = [];

    private string _filtroNome = string.Empty;

    private int _paginaAtual = 1;
    private Dizimista? _selectedDizimista;
    private string _statusSelecionado = "Todos";

    private bool _temProxima;

    private int _totalDizimistas;
    private int _totalPaginas = 1;

    public DizimistaListViewModel(
        GetDizimistaHandlers handlers,
        DeleteDizimistaHandler deleteHandler,
        InativarDizimistaHandler inativarHandler,
        DizimistaExcelService excelService,
        IUnitOfWork unitOfWork,
        INavigationService navigationService,
        IDialogService dialogService,
        IFilterCacheService filterCacheService)
    {
        _handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));
        _deleteHandler = deleteHandler ?? throw new ArgumentNullException(nameof(deleteHandler));
        _inativarHandler = inativarHandler ?? throw new ArgumentNullException(nameof(inativarHandler));
        _excelService = excelService ?? throw new ArgumentNullException(nameof(excelService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _filterCacheService = filterCacheService ?? throw new ArgumentNullException(nameof(filterCacheService));
        Dizimistas = [];
    }

    public ObservableCollection<Dizimista> Dizimistas
    {
        get => _dizimistas;
        private set => SetProperty(ref _dizimistas, value);
    }

    public Dizimista? SelectedDizimista
    {
        get => _selectedDizimista;
        set => SetProperty(ref _selectedDizimista, value);
    }

    public string FiltroNome
    {
        get => _filtroNome;
        set => SetProperty(ref _filtroNome, value);
    }

    public bool TemProxima
    {
        get => _temProxima;
        private set => SetProperty(ref _temProxima, value);
    }

    public int TotalDizimistas
    {
        get => _totalDizimistas;
        private set => SetProperty(ref _totalDizimistas, value);
    }

    public string TextoResultados => $"{Dizimistas.Count} de {TotalDizimistas} resultados";

    public string TextoBotaoSelecao =>
        DizimistasSelecionados.Count > 0 && DizimistasSelecionados.Count == Dizimistas.Count
            ? "Desselecionar Todos"
            : "Selecionar Todos";

    public List<string> StatusOptions { get; } = ["Todos", "Ativos", "Inativos"];

    public string StatusSelecionado
    {
        get => _statusSelecionado;
        set
        {
            if (SetProperty(ref _statusSelecionado, value))
            {
                ResetarPaginacao();
                _ = CarregarDizimistasAsync();
            }
        }
    }

    public ObservableCollection<Dizimista> DizimistasSelecionados
    {
        get => _dizimistasSelecionados;
        set
        {
            _dizimistasSelecionados?.CollectionChanged -= DizimistasSelecionados_CollectionChanged;
            SetProperty(ref _dizimistasSelecionados, value);
            _dizimistasSelecionados?.CollectionChanged += DizimistasSelecionados_CollectionChanged;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DizimistasSelecionados.Count));
            OnPropertyChanged(nameof(TextoBotaoSelecao));
        }
    }

    /// <summary>
    ///     Implementação de INavigationAware - Carrega dizimistas quando navega para esta página
    ///     Restaura os filtros do cache se não houver parâmetro clearCache
    /// </summary>
    public async void OnNavigatedTo(NavigationParameters parameters)
    {
        Debug.WriteLine("[NAV] OnNavigatedTo chamado para DizimistaListViewModel");

        // Se foi passado clearCache=true, limpar o cache e usar filtros padrão
        if (parameters != null && parameters.TryGetValue("clearCache", out var clearCacheObj) &&
            clearCacheObj is bool clearCache && clearCache)
        {
            _filterCacheService.ClearDizimistaListFilters();
            Debug.WriteLine("[Navigation] Cache de filtros foi limpo");

            // Usar filtros padrão
            _statusSelecionado = "Todos";
            _filtroNome = string.Empty;
        }
        else
        {
            // Tentar restaurar os filtros do cache
            var cachedFilters = _filterCacheService.GetDizimistaListFilters();
            if (cachedFilters.HasValue)
            {
                var (status, nome) = cachedFilters.Value;

                Debug.WriteLine("[Navigation] Restaurando filtros do cache");

                // Restaurar os filtros
                _statusSelecionado = status;
                _filtroNome = nome;
            }
            else
            {
                // Se não houver cache, usar filtros padrão
                Debug.WriteLine("[Navigation] Sem cache, usando filtros padrão");
                _statusSelecionado = "Todos";
                _filtroNome = string.Empty;
            }
        }

        // Notificar que as propriedades mudaram
        OnPropertyChanged(nameof(StatusSelecionado));
        OnPropertyChanged(nameof(FiltroNome));

        // Agora carregar os dados com os filtros restaurados/padrão
        await CarregarDizimistasAsync();
    }

    /// <summary>
    ///     Implementação de INavigationAware - Salva o estado ao sair da página
    /// </summary>
    public void OnNavigatedFrom()
    {
        Debug.WriteLine("[NAV] OnNavigatedFrom chamado para DizimistaListViewModel");
        // Salvar os filtros atuais no cache
        _filterCacheService.SaveDizimistaListFilters(StatusSelecionado, FiltroNome);
        Debug.WriteLine("[Navigation] Filtros salvos no cache");
    }

    private void DizimistasSelecionados_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(DizimistasSelecionados));
        OnPropertyChanged(nameof(DizimistasSelecionados.Count));
        OnPropertyChanged(nameof(TextoBotaoSelecao));
    }

    private void ResetarPaginacao()
    {
        _paginaAtual = 1;
        TemProxima = false;
        Dizimistas.Clear();
        OnPropertyChanged(nameof(TextoBotaoSelecao));
    }

    [RelayCommand]
    public async Task CarregarDizimistasAsync()
    {
        ResetarPaginacao();
        await _unitOfWork.ClearDbContextAsync();
        await CarregarProximaPaginaAsync();
        OnPropertyChanged(nameof(TextoBotaoSelecao));
    }

    [RelayCommand]
    public async Task CarregarMaisDizimistas()
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
            var result = await _handlers.Handle(new GetAllDizimistasPaginatedQuery(
                _paginaAtual,
                _tamanho_pagina,
                FiltroNome,
                StatusSelecionado));

            _totalPaginas = result.TotalPages;
            TotalDizimistas = result.TotalCount;

            foreach (var dizimista in result.Items) Dizimistas.Add(dizimista);

            OnPropertyChanged(nameof(TextoResultados));
            OnPropertyChanged(nameof(TextoBotaoSelecao));

            _paginaAtual++;
            TemProxima = _paginaAtual <= _totalPaginas;
        }
        finally
        {
            _carregandoMais = false;
        }
    }

    [RelayCommand]
    public async Task AplicarFiltros()
    {
        ResetarPaginacao();
        await CarregarDizimistasAsync();
    }

    [RelayCommand]
    public async Task LimparFiltros()
    {
        FiltroNome = string.Empty;
        StatusSelecionado = "Todos";
        ResetarPaginacao();
        await CarregarDizimistasAsync();
    }

    [RelayCommand]
    public async Task NovoDizimistaAsync()
    {
        try
        {
            _navigationService.Navigate("dizimista-cadastro");
            Debug.WriteLine("[NAV] Navegado para Novo Dizimista");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERRO] Erro ao navegar para novo dizimista: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task EditarDizimistaAsync(Dizimista dizimista)
    {
        try
        {
            if (dizimista != null)
            {
                var parameters = new NavigationParameters();
                parameters.Add("id", dizimista.Id);
                _navigationService.Navigate("dizimista-cadastro", parameters);
                Debug.WriteLine($"[NAV] Editando dizimista: {dizimista.Id}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERRO] Erro ao editar dizimista: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task VerDetalhesDizimistaAsync(Dizimista dizimista)
    {
        try
        {
            if (dizimista != null)
            {
                var parameters = new NavigationParameters();
                parameters.Add("id", dizimista.Id);
                _navigationService.Navigate("dizimista-detalhes", parameters);
                Debug.WriteLine($"[NAV] Visualizando detalhes de: {dizimista.Id}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERRO] Erro ao carregar detalhes: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task ExportarAsync()
    {
        try
        {
            Debug.WriteLine("[INFO] ExportarAsync iniciado");
            var excelStream = await _excelService.ExportarAsync(FiltroNome, StatusSelecionado);

            var fileName = $"dizimistas_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            string? filePath = null;

            // Se houver uma janela principal disponível, usar file picker
            if (Avalonia.Application.Current?.ApplicationLifetime is ClassicDesktopStyleApplicationLifetime desktop &&
                desktop.MainWindow != null)
                try
                {
                    var storageProvider = desktop.MainWindow.StorageProvider;
                    if (storageProvider != null)
                    {
                        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                        {
                            Title = "Exportar Dizimistas",
                            DefaultExtension = "xlsx",
                            FileTypeChoices = new[]
                                { new FilePickerFileType("Arquivo Excel") { Patterns = new[] { "*.xlsx" } } },
                            SuggestedFileName = fileName
                        });

                        if (file != null)
                        {
                            await using var fileStream = await file.OpenWriteAsync();
                            await fileStream.WriteAsync(excelStream.ToArray());
                            filePath = file.Path.AbsolutePath;
                            Debug.WriteLine($"[INFO] Arquivo exportado com sucesso em: {filePath}");
                        }
                        else
                        {
                            Debug.WriteLine("[INFO] Exportação cancelada pelo usuário");
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[WARN] File picker falhou, usando fallback: {ex.Message}");
                }

            // Fallback: salvar em Downloads
            if (string.IsNullOrEmpty(filePath))
            {
                var downloadsPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Downloads");

                if (!Directory.Exists(downloadsPath))
                    Directory.CreateDirectory(downloadsPath);

                filePath = Path.Combine(downloadsPath, fileName);
                await File.WriteAllBytesAsync(filePath, excelStream.ToArray());
                Debug.WriteLine($"[INFO] Arquivo exportado (fallback) para: {filePath}");
            }

            // Abrir arquivo automaticamente
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
            Debug.WriteLine($"[ERRO] Stack trace: {ex.StackTrace}");
        }
    }

    [RelayCommand]
    public static async Task BaixarModeloAsync()
    {
        try
        {
            Debug.WriteLine("[INFO] BaixarModeloAsync iniciado");

            var excelStream = DizimistaExcelService.GerarModelo();
            var fileName = "dizimistas_modelo.xlsx";

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
            Debug.WriteLine($"[ERRO] Stack trace: {ex.StackTrace}");
        }
    }

    [RelayCommand]
    public async Task ImportarAsync()
    {
        try
        {
            Debug.WriteLine("[INFO] ImportarAsync iniciado");

            // Create a file picker dialog
            // Note: In Avalonia, you need to use Avalonia's file dialog
            // This requires TopLevel or window context
            // For now, we'll implement basic file dialog logic
            Debug.WriteLine("[INFO] Aguardando seleção de arquivo de importação");

            // This will need to be called from the View or through a service
            // that has access to the window context
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERRO] Erro ao importar: {ex.Message}");
            Debug.WriteLine($"[ERRO] Stack trace: {ex.StackTrace}");
        }
    }

    [RelayCommand]
    public async Task GerarRelatorioGeralAsync()
    {
        await CarregarDizimistasAsync();
        Debug.WriteLine($"[INFO] Total de dizimistas: {Dizimistas.Count}");
    }

    [RelayCommand]
    public async Task ExportarRelatorioGeralAsync()
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("Relatório Geral de Dizimistas");
            sb.AppendLine($"Data: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine($"Total de Dizimistas: {Dizimistas.Count}");
            sb.AppendLine();

            Debug.WriteLine(sb.ToString());
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERRO] Erro ao gerar relatório: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task ExcluirDizimistasSelecionadosAsync()
    {
        Debug.WriteLine($"[EXCLUIR] DizimistasSelecionados.Count: {DizimistasSelecionados.Count}");
        foreach (var d in DizimistasSelecionados) Debug.WriteLine($"[EXCLUIR] - Selecionado: {d.Nome} (ID: {d.Id})");

        if (DizimistasSelecionados.Count == 0)
        {
            Debug.WriteLine("[EXCLUIR] Nenhum dizimista selecionado. Operação cancelada.");
            return;
        }

        // Solicitar confirmação
        var confirmar = await _dialogService.ShowConfirmAsync(
            "Confirmação de Exclusão",
            $"Deseja realmente excluir {DizimistasSelecionados.Count} dizimista(s) selecionado(s)? Esta ação não pode ser desfeita.",
            "Sim, Excluir",
            "Cancelar");

        if (!confirmar)
        {
            Debug.WriteLine("[EXCLUIR] Exclusão cancelada pelo usuário.");
            return;
        }

        foreach (var dizimista in DizimistasSelecionados.ToList())
        {
            Debug.WriteLine($"[EXCLUIR] Excluindo: {dizimista.Nome} (ID: {dizimista.Id})");
            await _deleteHandler.Handle(new DeleteDizimistaCommand(dizimista.Id));
        }

        Debug.WriteLine("[EXCLUIR] Recarregando lista de dizimistas...");
        await CarregarDizimistasAsync();
        DizimistasSelecionados.Clear();
        Debug.WriteLine("[EXCLUIR] Exclusão concluída.");
    }

    [RelayCommand]
    public async Task InativarDizimistasSelecionadosAsync()
    {
        Debug.WriteLine($"[INATIVAR] DizimistasSelecionados.Count: {DizimistasSelecionados.Count}");
        foreach (var d in DizimistasSelecionados) Debug.WriteLine($"[INATIVAR] - Selecionado: {d.Nome} (ID: {d.Id})");

        if (DizimistasSelecionados.Count == 0)
        {
            Debug.WriteLine("[INATIVAR] Nenhum dizimista selecionado. Operação cancelada.");
            return;
        }

        // Solicitar confirmação
        var confirmar = await _dialogService.ShowConfirmAsync(
            "Confirmação de Inativação",
            $"Deseja realmente ativar/inativar {DizimistasSelecionados.Count} dizimista(s) selecionado(s)?",
            "Sim, Ativar/Inativar",
            "Cancelar");

        if (!confirmar)
        {
            Debug.WriteLine("[INATIVAR] Inativação cancelada pelo usuário.");
            return;
        }

        foreach (var dizimista in DizimistasSelecionados.ToList())
        {
            Debug.WriteLine($"[INATIVAR] Inativando/Ativando: {dizimista.Nome} (ID: {dizimista.Id})");
            await _inativarHandler.Handle(new InativarDizimistaCommand(dizimista.Id));
        }

        Debug.WriteLine("[INATIVAR] Recarregando lista de dizimistas...");
        await CarregarDizimistasAsync();
        DizimistasSelecionados.Clear();
        Debug.WriteLine("[INATIVAR] Inativação concluída.");
    }

    [RelayCommand]
    public void SelecionarTodos()
    {
        if (DizimistasSelecionados.Count == Dizimistas.Count)
        {
            // Se todos já estão selecionados, desselecionar todos
            DizimistasSelecionados.Clear();
        }
        else
        {
            // Selecionar todos
            DizimistasSelecionados.Clear();
            foreach (var dizimista in Dizimistas) DizimistasSelecionados.Add(dizimista);
        }

        OnPropertyChanged(nameof(TextoBotaoSelecao));
    }

    [RelayCommand]
    public void AlternarSelecaoDizimista(Dizimista dizimista)
    {
        if (dizimista == null)
        {
            Debug.WriteLine("[SELEÇÃO] Dizimista é null!");
            return;
        }

        Debug.WriteLine($"[SELEÇÃO] Alternando seleção do dizimista: {dizimista.Nome} (ID: {dizimista.Id})");
        Debug.WriteLine($"[SELEÇÃO] Antes - Quantidade selecionados: {DizimistasSelecionados.Count}");

        if (DizimistasSelecionados.Contains(dizimista))
        {
            DizimistasSelecionados.Remove(dizimista);
            Debug.WriteLine("[SELEÇÃO] Removido da seleção");
        }
        else
        {
            DizimistasSelecionados.Add(dizimista);
            Debug.WriteLine("[SELEÇÃO] Adicionado à seleção");
        }

        Debug.WriteLine($"[SELEÇÃO] Depois - Quantidade selecionados: {DizimistasSelecionados.Count}");
    }

    [RelayCommand]
    public async Task ImprimirAsync()
    {
        try
        {
            // Carregar TODOS os dizimistas COM OS FILTROS aplicados, mantendo a mesma ordem da página
            var todosDizimistas = await CarregarTodosDizimistasComFiltrosAsync();

            // Determinar se apenas ativos
            bool? apenasAtivos = null;
            if (StatusSelecionado == "Ativos")
                apenasAtivos = true;
            else if (StatusSelecionado == "Inativos")
                apenasAtivos = false;

            var pdfService = new DizimistaPdfService(_unitOfWork);
            var htmlStream = await pdfService.ImprimirAsync(
                todosDizimistas,
                FiltroNome,
                apenasAtivos);

            // Salvar em arquivo temporário
            var fileName = $"dizimistas_{DateTime.Now:yyyyMMdd_HHmmss}.html";
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
    ///     Carrega TODOS os dizimistas com os filtros aplicados, mantendo a mesma ordem da paginação
    /// </summary>
    private async Task<List<Dizimista>> CarregarTodosDizimistasComFiltrosAsync()
    {
        var todosDizimistas = new List<Dizimista>();
        var pageNumber = 1;
        var totalPages = 1;

        while (pageNumber <= totalPages)
        {
            var result = await _handlers.Handle(new GetAllDizimistasPaginatedQuery(
                pageNumber,
                _tamanho_pagina,
                FiltroNome,
                StatusSelecionado));

            todosDizimistas.AddRange(result.Items);
            totalPages = result.TotalPages;
            pageNumber++;
        }

        return todosDizimistas;
    }
}