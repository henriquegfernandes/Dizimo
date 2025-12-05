using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;
using Dizimo.Application.Ofertas.Commands;
using Dizimo.Application.Ofertas.Handlers;
using Dizimo.Application.Ofertas.Queries;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Storage;
using System.Text;

namespace Dizimo.ViewModels;

public partial class OfertaListViewModel : ObservableObject
{
    private readonly GetOfertaHandlers _getHandlers;
    private readonly DeleteOfertaHandler _deleteHandler;
    private readonly OfertaCsvService _csvService;
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

    private DateTime? _filtroData = DateTime.Today;
    public DateTime? FiltroData
    {
        get => _filtroData;
        set 
        { 
            SetProperty(ref _filtroData, value);
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
        OfertaCsvService csvService,
        IUnitOfWork unitOfWork)
    {
        _getHandlers = getHandlers ?? throw new ArgumentNullException(nameof(getHandlers));
        _deleteHandler = deleteHandler ?? throw new ArgumentNullException(nameof(deleteHandler));
        _csvService = csvService ?? throw new ArgumentNullException(nameof(csvService));
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

    [RelayCommand]
    public void AplicarFiltros()
    {
        IEnumerable<Oferta> filtrados = TodasOfertas;

        // Filtro por data (apenas se uma data foi selecionada)
        if (FiltroData.HasValue)
            filtrados = filtrados.Where(o => o.Data.Date == FiltroData.Value.Date);

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

        var filteredList = filtrados is List<Oferta> ofertaList ? ofertaList : filtrados.ToList();
        Ofertas = new ObservableCollection<Oferta>(filteredList);
    }

    [RelayCommand]
    public void LimparFiltros()
    {
        FiltroNome = string.Empty;
        FiltroData = null;
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
            var csv = await _csvService.ExportarAsync(Ofertas.ToList());

            var fileName = $"ofertas_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

#if WINDOWS
            var folder = await FolderPicker.Default.PickAsync(CancellationToken.None);

            if (folder?.Folder?.Path == null)
            {
                System.Diagnostics.Debug.WriteLine("[INFO] Exportação cancelada pelo usuário");
                return;
            }

            var filePath = Path.Combine(folder.Folder.Path, fileName);
            await File.WriteAllTextAsync(filePath, csv, Encoding.UTF8);

            System.Diagnostics.Debug.WriteLine($"[INFO] Arquivo exportado para: {filePath}");

            var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
            if (mainPage != null)
            {
                await mainPage.DisplayAlertAsync("Exportação",
                    $"Planilha de ofertas exportada com sucesso!\n\nLocalização: {filePath}", "OK");
            }
#else
            var downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");

            if (!Directory.Exists(downloadsPath))
                Directory.CreateDirectory(downloadsPath);

            var filePath = Path.Combine(downloadsPath, fileName);
            await File.WriteAllTextAsync(filePath, csv, Encoding.UTF8);

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
            var csv = _csvService.GerarModelo();
            var fileName = "ofertas_modelo.csv";

#if WINDOWS
            var folder = await FolderPicker.Default.PickAsync(CancellationToken.None);

            if (folder?.Folder?.Path == null)
            {
                System.Diagnostics.Debug.WriteLine("[INFO] Download do modelo cancelado pelo usuário");
                return;
            }

            var filePath = Path.Combine(folder.Folder.Path, fileName);
            await File.WriteAllTextAsync(filePath, csv, Encoding.UTF8);

            System.Diagnostics.Debug.WriteLine($"[INFO] Arquivo modelo salvo em: {filePath}");

            var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
            if (mainPage != null)
            {
                await mainPage.DisplayAlertAsync("Modelo Baixado",
                    $"Planilha modelo baixada com sucesso!\n\nLocalização: {filePath}", "OK");
            }
#else
            var downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");

            if (!Directory.Exists(downloadsPath))
                Directory.CreateDirectory(downloadsPath);

            var filePath = Path.Combine(downloadsPath, fileName);
            await File.WriteAllTextAsync(filePath, csv, Encoding.UTF8);

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
                PickerTitle = "Selecione um arquivo CSV",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "public.comma-separated-values-text" } },
                    { DevicePlatform.Android, new[] { "text/csv" } },
                    { DevicePlatform.WinUI, new[] { ".csv" } },
                    { DevicePlatform.MacCatalyst, new[] { "csv" } },
                })
            });

            if (result == null)
            {
                System.Diagnostics.Debug.WriteLine("[INFO] Importação cancelada pelo usuário");
                return;
            }

            // Ler o arquivo selecionado
            var csv = await File.ReadAllTextAsync(result.FullPath, Encoding.UTF8);
            var resultado = await _csvService.ImportarAsync(csv);
            
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
