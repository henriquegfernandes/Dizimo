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

namespace Dizimo.ViewModels;

public partial class OfertaListViewModel : ObservableObject
{
    private readonly GetOfertaHandlers _getHandlers;
    private readonly DeleteOfertaHandler _deleteHandler;
    private readonly OfertaExcelService _excelService;
    private readonly IUnitOfWork _unitOfWork;

    public List<Oferta> TodasOfertas { get; private set; } = new List<Oferta>();
    
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
            SetProperty(ref _filtroDataInicio, value);
            // Aplicar filtros automaticamente quando a data muda
            AplicarFiltros();
        }
    }

    private DateTime? _filtroDataFim = DateTime.Today;
    public DateTime? FiltroDataFim
    {
        get => _filtroDataFim;
        set 
        { 
            SetProperty(ref _filtroDataFim, value);
            // Aplicar filtros automaticamente quando a data muda
            AplicarFiltros();
        }
    }

    private Oferta? _selectedOferta;
    public Oferta? SelectedOferta
    {
        get => _selectedOferta;
        set => SetProperty(ref _selectedOferta, value);
    }

    private int paginaAtual = 1;
    private const int tamanhoPagina = 20;
    private bool carregandoMais = false;

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

    [RelayCommand]
    public async Task CarregarOfertasAsync()
    {
        paginaAtual = 1;
        await _unitOfWork.ClearDbContextAsync();
        var lista = await _getHandlers.Handle(new GetAllOfertasQuery());
        TodasOfertas = lista is List<Oferta> ofertaList ? ofertaList : lista.ToList();
        
        // Aplicar filtros automaticamente após carregar
        AplicarFiltros();
    }

    [RelayCommand]
    public async Task CarregarMaisOfertasAsync()
    {
        if (carregandoMais) return;
        carregandoMais = true;
        paginaAtual++;
        var lista = await _getHandlers.Handle(new GetAllOfertasQuery());
        var sourceList = lista is List<Oferta> ofertaList ? ofertaList : lista.ToList();
        int start = (paginaAtual - 1) * tamanhoPagina;
        int count = Math.Min(tamanhoPagina, sourceList.Count - start);
        if (count > 0)
        {
            var novos = sourceList.GetRange(start, count);
            foreach (var o in novos)
            {
                Ofertas.Add(o);
            }
        }
        carregandoMais = false;
    }

    private decimal _valorTotal;
    public decimal ValorTotal
    {
        get => _valorTotal;
        private set => SetProperty(ref _valorTotal, value);
    }

    // Lista de opções para o filtro de tipo de pagamento
    public List<string> TiposPagamento { get; } = new List<string> { "Todos", "PIX", "Dinheiro", "Cartao" };

    private string _filtroTipoPagamento = "Todos";
    public string FiltroTipoPagamento
    {
        get => _filtroTipoPagamento;
        set
        {
            if (SetProperty(ref _filtroTipoPagamento, value))
            {
                AplicarFiltros();
            }
        }
    }

    [RelayCommand]
    public void AplicarFiltros()
    {
        IEnumerable<Oferta> filtrados = TodasOfertas;

        // Filtro por data (apenas se uma data foi selecionada)
        if (FiltroDataInicio.HasValue)
            filtrados = filtrados.Where(o => o.Data.Date >= FiltroDataInicio.Value.Date);

        if (FiltroDataFim.HasValue)
            filtrados = filtrados.Where(o => o.Data.Date <= FiltroDataFim.Value.Date);

        // Filtro por tipo de pagamento (apenas se não for "Todos")
        if (!string.IsNullOrWhiteSpace(FiltroTipoPagamento) && FiltroTipoPagamento != "Todos")
        {
            if (Enum.TryParse<TipoPagamento>(FiltroTipoPagamento, out var tipo))
            {
                filtrados = filtrados.Where(o => o.TipoPagamento == tipo);
            }
        }

        // Filtro unificado: busca por nome do dizimista OU código do dizimista
        if (!string.IsNullOrWhiteSpace(FiltroNome))
        {
            filtrados = filtrados.Where(o =>
            {
                var dizimista = _unitOfWork.Dizimistas.GetByIdAsync(o.DizimistaId).Result;
                return dizimista != null && (
                    dizimista.Nome.Contains(FiltroNome, StringComparison.OrdinalIgnoreCase) ||
                    dizimista.NumeroCadastro.ToString().Contains(FiltroNome));
            });
        }

        // Ordenação: Data, DizimistaId, AnoReferencia, MesReferencia
        var ordered = filtrados
            .OrderBy(o => o.Data)
            .ThenBy(o => o.DizimistaId)
            .ThenBy(o => o.AnoReferencia)
            .ThenBy(o => o.MesReferencia)
            .ToList();
        Ofertas = new ObservableCollection<Oferta>(ordered);
        // Calcular total
        ValorTotal = Ofertas.Sum(o => o.Valor);
    }

    [RelayCommand]
    public void LimparFiltros()
    {
        FiltroNome = string.Empty;
        FiltroDataInicio = null;
        FiltroDataFim = null;
        FiltroTipoPagamento = "Todos";
        AplicarFiltros();
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

        var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
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
        var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
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
            // Exportar considerando os filtros aplicados
            var excelStream = await _excelService.ExportarAsync(Ofertas.ToList());

            var fileName = $"ofertas_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

#if WINDOWS
            var result = await FileSaver.Default.SaveAsync(fileName, excelStream, CancellationToken.None);

            if (result.IsSuccessful)
            {
                System.Diagnostics.Debug.WriteLine($"[INFO] Arquivo exportado para: {result.FilePath}");

                var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage != null)
                {
                    await mainPage.DisplayAlertAsync("Exportação",
                        $"Planilha de ofertas exportada com sucesso!", "OK");
                }
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

            var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
            if (mainPage != null)
            {
                await mainPage.DisplayAlertAsync("Exportação",
                    $"Planilha de ofertas exportada com sucesso!\n\nLocalização: {filePath}", "OK");
            }
#endif
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao exportar: {ex.Message}");
            var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
            if (mainPage != null)
            {
                await mainPage.DisplayAlertAsync("Erro", $"Erro ao exportar: {ex.Message}", "OK");
            }
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

                var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage != null)
                {
                    await mainPage.DisplayAlertAsync("Modelo Baixado",
                        $"Planilha modelo baixada com sucesso!", "OK");
                }
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

            var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
            if (mainPage != null)
            {
                await mainPage.DisplayAlertAsync("Modelo Baixado",
                    $"Planilha modelo baixada com sucesso!\n\nLocalização: {filePath}", "OK");
            }
#endif
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao baixar modelo: {ex.Message}");

            var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
            if (mainPage != null)
            {
                await mainPage.DisplayAlertAsync("Erro", $"Erro ao baixar modelo: {ex.Message}", "OK");
            }
        }
    }

    [RelayCommand]
    public async Task ImportarAsync()
    {
        try
        {
            var mainPage = GetMainPage();
            
            // Usar FilePicker para selecionar o arquivo
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

            // Ler o arquivo selecionado
            var excelBytes = await File.ReadAllBytesAsync(result.FullPath);
            var resultado = await _excelService.ImportarAsync(excelBytes);
            
            // Adicionar ofertas importadas
            foreach (var o in resultado.OfertasImportadas)
            {
                await _unitOfWork.Ofertas.AddAsync(o);
            }
            await _unitOfWork.SaveChangesAsync();
            await CarregarOfertasAsync();
            
            // Montar mensagem de resultado
            var mensagem = $"Importação concluída!\n\n";
            mensagem += $"✓ Ofertas importadas: {resultado.OfertasImportadas.Count}\n";
            
            if (resultado.Erros.Count > 0)
            {
                mensagem += $"\n❌ Ofertas não importadas: {resultado.Erros.Count}\n\n";
                mensagem += "Erros:\n";
                
                // Limitar a 10 erros para não deixar a mensagem muito grande
                var errosExibir = resultado.Erros.Take(10).ToList();
                foreach (var erro in errosExibir)
                {
                    mensagem += $"• {erro}\n";
                }
                
                if (resultado.Erros.Count > 10)
                {
                    mensagem += $"\n... e mais {resultado.Erros.Count - 10} erro(s)";
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
