using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;
using Dizimo.Application.Ofertas.Commands;
using Dizimo.Application.Ofertas.Handlers;
using Dizimo.Application.Ofertas.Queries;
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

    private ObservableCollection<Oferta> _ofertas = new ObservableCollection<Oferta>();
    public ObservableCollection<Oferta> Ofertas
    {
        get => _ofertas;
        private set => SetProperty(ref _ofertas, value);
    }

    private string _filtroNome = string.Empty;
    public string FiltroNome
    {
        get => _filtroNome;
        set => SetProperty(ref _filtroNome, value);
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

    public List<string> TiposPagamento { get; } = new List<string> { "Todos", "PIX", "Dinheiro", "Cartao" };

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

    private ObservableCollection<Oferta> _ofertasSelecionadas = new();
    public ObservableCollection<Oferta> OfertasSelecionadas
    {
        get => _ofertasSelecionadas;
        set
        {
            if (_ofertasSelecionadas != null)
                _ofertasSelecionadas.CollectionChanged -= OfertasSelecionadas_CollectionChanged;
            SetProperty(ref _ofertasSelecionadas, value);
            if (_ofertasSelecionadas != null)
                _ofertasSelecionadas.CollectionChanged += OfertasSelecionadas_CollectionChanged;
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
        Ofertas = new ObservableCollection<Oferta>();
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

            _totalPaginas = result.TotalPages;
            TotalOfertas = result.TotalCount;

            foreach (var oferta in items)
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
            FiltroTipoPagamento != "Todos" ? FiltroTipoPagamento : null);
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
        await Shell.Current.GoToAsync("oferta-cadastro");
    }

    [RelayCommand]
    public async Task EditarOfertaAsync(Oferta oferta)
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
                todasOfertas.ToList(),
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
    public async Task BaixarModeloAsync()
    {
        try
        {
            var excelStream = _excelService.GerarModelo();
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
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "com.microsoft.excel.xlsx" } },
                    { DevicePlatform.Android, new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" } },
                    { DevicePlatform.WinUI, new[] { ".xlsx" } },
                    { DevicePlatform.MacCatalyst, new[] { "xlsx" } },
                })
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

    private static Page? GetMainPage()
    {
        return Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
    }
}
