using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Domain.Entities;
using Dizimo.Application.Dizimistas.Queries;
using Dizimo.Application.Dizimistas.Handlers;
using Dizimo.Application.Dizimistas.Commands;
using System.Collections.ObjectModel;
using Dizimo.Application.Relatorios;
using Dizimo.Domain.Repositories;

namespace Dizimo.ViewModels
{
    public partial class DizimistaListViewModel : ObservableObject
    {
        private readonly GetDizimistaHandlers _handlers;
        private readonly DeleteDizimistaHandler _deleteHandler;
        private readonly InativarDizimistaHandler _inativarHandler;
        private readonly DizimistaCsvService _csvService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly RelatorioAniversariantesService _relatorioAniversariantesService;

        // IDE0028: Collection initialization can be simplified
        public List<Dizimista> TodosDizimistas { get; private set; } = new List<Dizimista>();

        // IDE0028: Collection initialization can be simplified
        private ObservableCollection<Dizimista> _dizimistas = new ObservableCollection<Dizimista>();

        // IDE0028: Collection initialization can be simplified
        public ObservableCollection<Dizimista> Aniversariantes { get; private set; } = new ObservableCollection<Dizimista>();

        // IDE0028: Collection initialization can be simplified
        private string _filtroNome = string.Empty;
        private string _filtroNumeroCadastro = string.Empty;
        private Dizimista? _selectedDizimista;
        private int _filtroMesAniversario = DateTime.Today.Month;

        public ObservableCollection<Dizimista> Dizimistas
        {
            get => _dizimistas;
            private set
            {
                SetProperty(ref _dizimistas, value);
            }
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

        public string FiltroNumeroCadastro
        {
            get => _filtroNumeroCadastro;
            set => SetProperty(ref _filtroNumeroCadastro, value);
        }

        public int FiltroMesAniversario
        {
            get => _filtroMesAniversario;
            set => SetProperty(ref _filtroMesAniversario, value);
        }

        private int paginaAtual = 1;
        private const int tamanhoPagina = 20;
        private bool carregandoMais = false;

        public DizimistaListViewModel(
            GetDizimistaHandlers handlers,
            DeleteDizimistaHandler deleteHandler,
            InativarDizimistaHandler inativarHandler,
            DizimistaCsvService csvService,
            IUnitOfWork unitOfWork,
            RelatorioAniversariantesService relatorioAniversariantesService)
        {
            _handlers = handlers;
            _deleteHandler = deleteHandler;
            _inativarHandler = inativarHandler;
            _csvService = csvService;
            _unitOfWork = unitOfWork;
            _relatorioAniversariantesService = relatorioAniversariantesService;
            Dizimistas = new ObservableCollection<Dizimista>();
        }

        [RelayCommand]
        public async Task CarregarDizimistasAsync()
        {
            paginaAtual = 1;
            var lista = await _handlers.Handle(new GetAllDizimistasQuery());
            // CS0103: The name 'todosDizimistas' does not exist in the current context
            // CA1826: Do not use Enumerable methods on indexable collections. Instead use the collection directly.
            TodosDizimistas = lista is List<Dizimista> dizimistaList ? dizimistaList : lista.ToList();
            Dizimistas = new ObservableCollection<Dizimista>(TodosDizimistas.Count > tamanhoPagina
                ? TodosDizimistas.GetRange(0, tamanhoPagina)
                : TodosDizimistas);
        }

        [RelayCommand]
        public async Task CarregarMaisDizimistasAsync()
        {
            if (carregandoMais) return;
            carregandoMais = true;
            paginaAtual++;
            var lista = await _handlers.Handle(new GetAllDizimistasQuery());
            var sourceList = lista is List<Dizimista> dizimistaList ? dizimistaList : lista.ToList();
            int start = (paginaAtual - 1) * tamanhoPagina;
            int count = Math.Min(tamanhoPagina, sourceList.Count - start);
            if (count > 0)
            {
                var novos = sourceList.GetRange(start, count);
                foreach (var d in novos)
                {
                    Dizimistas.Add(d);
                }
            }
            carregandoMais = false;
        }

        [RelayCommand]
        public void AplicarFiltros()
        {
            IEnumerable<Dizimista> filtrados = TodosDizimistas;
            if (!string.IsNullOrWhiteSpace(FiltroNome))
                filtrados = filtrados.Where(d => d.Nome.Contains(FiltroNome, StringComparison.OrdinalIgnoreCase));
            if (int.TryParse(FiltroNumeroCadastro, out var num))
                filtrados = filtrados.Where(d => d.NumeroCadastro == num);

            // CA1826: Do not use Enumerable methods on indexable collections. Instead use the collection directly.
            var filteredList = filtrados is List<Dizimista> dizimistaList ? dizimistaList : filtrados.ToList();
            Dizimistas = new ObservableCollection<Dizimista>(filteredList);
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
                var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage != null)
                {
                    bool confirm = await mainPage.DisplayAlertAsync("Confirmação", $"Deseja excluir o dizimista '{SelectedDizimista.Nome}'?", "Sim", "Não");
                    if (confirm)
                    {
                        try
                        {
                            await _deleteHandler.Handle(new DeleteDizimistaCommand(SelectedDizimista.Id));
                            await CarregarDizimistasAsync();
                            await mainPage.DisplayAlertAsync("Sucesso", "Dizimista excluído com sucesso.", "OK");
                        }
                        catch (Exception ex)
                        {
                            await mainPage.DisplayAlertAsync("Erro", $"Erro ao excluir: {ex.Message}", "OK");
                        }
                    }
                }
            }
        }

        [RelayCommand]
        public async Task InativarDizimistaAsync()
        {
            if (SelectedDizimista != null)
            {
                var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage != null)
                {
                    bool confirm = await mainPage.DisplayAlertAsync("Confirmação", $"Deseja inativar o dizimista '{SelectedDizimista.Nome}'?", "Sim", "Não");
                    if (confirm)
                    {
                        try
                        {
                            await _inativarHandler.Handle(new InativarDizimistaCommand(SelectedDizimista.Id));
                            await CarregarDizimistasAsync();
                            await mainPage.DisplayAlertAsync("Sucesso", "Dizimista inativado com sucesso.", "OK");
                        }
                        catch (Exception ex)
                        {
                            await mainPage.DisplayAlertAsync("Erro", $"Erro ao inativar: {ex.Message}", "OK");
                        }
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

                var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage != null)
                {
                    await mainPage.DisplayAlertAsync("Exportação", $"Arquivo exportado para: {filePath}", "OK");
                }
            }
            catch (Exception ex)
            {
                var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage != null)
                {
                    await mainPage.DisplayAlertAsync("Erro", $"Erro ao exportar: {ex.Message}", "OK");
                }
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
                    var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                    if (mainPage != null)
                    {
                        await mainPage.DisplayAlertAsync("Importação", $"Coloque o arquivo 'dizimistas_import.csv' em: {filePath}", "OK");
                    }
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
                var mainPageSuccess = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPageSuccess != null)
                {
                    await mainPageSuccess.DisplayAlertAsync("Importação", $"Importação concluída com sucesso.", "OK");
                }
            }
            catch (Exception ex)
            {
                var mainPageError = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPageError != null)
                {
                    await mainPageError.DisplayAlertAsync("Erro", $"Erro ao importar: {ex.Message}", "OK");
                }
            }
        }

        [RelayCommand]
        public async Task BaixarModeloAsync()
        {
            try
            {
                var modeloPath = Path.Combine(FileSystem.Current.AppDataDirectory, "dizimistas_modelo.csv");
                var modeloOrigem = Path.Combine(AppContext.BaseDirectory, "dizimistas_modelo.csv");
                var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                if (File.Exists(modeloOrigem))
                {
                    File.Copy(modeloOrigem, modeloPath, true);
                    if (mainPage != null)
                    {
                        await mainPage.DisplayAlertAsync("Modelo", $"Arquivo modelo salvo em: {modeloPath}", "OK");
                    }
                }
                else
                {
                    if (mainPage != null)
                    {
                        await mainPage.DisplayAlertAsync("Modelo", "Arquivo modelo não encontrado.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage != null)
                {
                    await mainPage.DisplayAlertAsync("Erro", $"Erro ao baixar modelo: {ex.Message}", "OK");
                }
            }
        }

        [RelayCommand]
        public async Task GerarRelatorioAniversariantesAsync()
        {
            var lista = await _relatorioAniversariantesService.GetAniversariantesAsync(FiltroMesAniversario);
            Aniversariantes = new ObservableCollection<Dizimista>(lista);
            var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
            if (mainPage != null)
            {
                await mainPage.DisplayAlertAsync("Relatório Aniversariantes", $"{Aniversariantes.Count} aniversariantes encontrados para o mês {FiltroMesAniversario}", "OK");
            }
        }

        [RelayCommand]
        public async Task GerarRelatorioGeralAsync()
        {
            await CarregarDizimistasAsync();
            var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
            if (mainPage != null)
            {
                await mainPage.DisplayAlertAsync("Relatório Geral", $"Total de dizimistas: {Dizimistas.Count}", "OK");
            }
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
                var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage != null)
                {
                    await mainPage.DisplayAlertAsync("Exportação", $"Relatório geral exportado para: {filePath}", "OK");
                }
            }
            catch (Exception ex)
            {
                var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage != null)
                {
                    await mainPage.DisplayAlertAsync("Erro", $"Erro ao exportar relatório: {ex.Message}", "OK");
                }
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
                var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage != null)
                {
                    await mainPage.DisplayAlertAsync("Exportação", $"Relatório de aniversariantes exportado para: {filePath}", "OK");
                }
            }
            catch (Exception ex)
            {
                var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage != null)
                {
                    await mainPage.DisplayAlertAsync("Erro", $"Erro ao exportar relatório: {ex.Message}", "OK");
                }
            }
        }
    }
}
