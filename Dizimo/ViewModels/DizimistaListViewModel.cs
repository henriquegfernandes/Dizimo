using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Domain.Entities;
using Dizimo.Application.Dizimistas.Queries;
using Dizimo.Application.Dizimistas.Handlers;
using Dizimo.Application.Dizimistas.Commands;
using Dizimo.Services;
using System.Collections.ObjectModel;
using Dizimo.Domain.Repositories;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Storage;
using CommunityToolkit.Maui.Storage;
using System.IO;
using Dizimo.Domain.Models;

namespace Dizimo.ViewModels
{
    public partial class DizimistaListViewModel : ObservableObject
    {
        private readonly GetDizimistaHandlers _handlers;
        private readonly DeleteDizimistaHandler _deleteHandler;
        private readonly InativarDizimistaHandler _inativarHandler;
        private readonly DizimistaExcelService _excelService;
        private readonly IUnitOfWork _unitOfWork;

        private static readonly FilePickerFileType ExcelFileType = new(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.WinUI, new[] { ".xlsx" } },
            { DevicePlatform.macOS, new[] { ".xlsx" } },
            { DevicePlatform.iOS, new[] { "com.microsoft.excel.xlsx" } },
            { DevicePlatform.Android, new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" } },
        });

        private ObservableCollection<Dizimista> _dizimistas = [];

        private string _filtroNome = string.Empty;
        private Dizimista? _selectedDizimista;

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
            set
            {
                if (SetProperty(ref _filtroNome, value))
                {
                    ResetarPaginacao();
                    _ = CarregarDizimistasAsync();
                }
            }
        }

        private int _paginaAtual = 1;
        private const int _tamanho_pagina = 20;
        private bool _carregandoMais = false;
        private int _totalPaginas = 1;

        private bool _temProxima = false;
        public bool TemProxima
        {
            get => _temProxima;
            private set => SetProperty(ref _temProxima, value);
        }

        private int _totalDizimistas = 0;
        public int TotalDizimistas
        {
            get => _totalDizimistas;
            private set => SetProperty(ref _totalDizimistas, value);
        }

        public string TextoResultados
        {
            get => $"{Dizimistas.Count} de {TotalDizimistas} resultados";
        }

        public List<string> StatusOptions { get; } = [ "Todos", "Ativos", "Inativos" ];
        private string _statusSelecionado = "Todos";
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

        private ObservableCollection<Dizimista> _dizimistasSelecionados = [];
        public ObservableCollection<Dizimista> DizimistasSelecionados
        {
            get => _dizimistasSelecionados;
            set
            {
                _dizimistasSelecionados?.CollectionChanged -= DizimistasSelecionados_CollectionChanged;
                SetProperty(ref _dizimistasSelecionados, value);
                _dizimistasSelecionados?.CollectionChanged += DizimistasSelecionados_CollectionChanged;
                OnPropertyChanged(nameof(DizimistasSelecionados));
                OnPropertyChanged(nameof(DizimistasSelecionados.Count));
            }
        }

        private void DizimistasSelecionados_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(DizimistasSelecionados));
            OnPropertyChanged(nameof(DizimistasSelecionados.Count));
        }

        public DizimistaListViewModel(
            GetDizimistaHandlers handlers,
            DeleteDizimistaHandler deleteHandler,
            InativarDizimistaHandler inativarHandler,
            DizimistaExcelService excelService,
            IUnitOfWork unitOfWork)
        {
            _handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));
            _deleteHandler = deleteHandler ?? throw new ArgumentNullException(nameof(deleteHandler));
            _inativarHandler = inativarHandler ?? throw new ArgumentNullException(nameof(inativarHandler));
            _excelService = excelService ?? throw new ArgumentNullException(nameof(excelService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            Dizimistas = [];
        }

        private void ResetarPaginacao()
        {
            _paginaAtual = 1;
            TemProxima = false;
            Dizimistas.Clear();
        }

        [RelayCommand]
        public async Task CarregarDizimistasAsync()
        {
            ResetarPaginacao();
            await _unitOfWork.ClearDbContextAsync();
            await CarregarProximaPaginaAsync();
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

                foreach (var dizimista in result.Items)
                {
                    Dizimistas.Add(dizimista);
                }

                OnPropertyChanged(nameof(TextoResultados));

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
        public static async Task NovoDizimistaAsync()
        {
            await Shell.Current.GoToAsync("dizimista-cadastro");
        }

        [RelayCommand]
        public static async Task EditarDizimistaAsync(Dizimista dizimista)
        {
            if (dizimista != null)
            {
                var navigationParameter = new Dictionary<string, object>
                {
                    { "id", dizimista.Id.ToString() }
                };
                await Shell.Current.GoToAsync("dizimista-cadastro", navigationParameter);
            }
        }

        [RelayCommand]
        public static async Task VerDetalhesDizimistaAsync(Dizimista dizimista)
        {
            if (dizimista != null)
            {
                System.Diagnostics.Debug.WriteLine($"[INFO] VerDetalhesDizimistaCommand - ID: {dizimista.Id}");
                var navigationParameter = new Dictionary<string, object>
                {
                    { "id", dizimista.Id.ToString() }
                };
                await Shell.Current.GoToAsync("dizimista-detalhes", navigationParameter);
            }
        }

        [RelayCommand]
        public async Task ExportarAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[INFO] ExportarAsync iniciado");
                // Passar os filtros aplicados para exportação
                var excelStream = await _excelService.ExportarAsync(FiltroNome, StatusSelecionado);
                
                var fileName = $"dizimistas_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                
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

                    var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
                    var mainPage = windows is { Count: > 0 } ? windows[0].Page : null;
                    if (mainPage != null)
                    {
                        await mainPage.DisplayAlertAsync("Exportação", 
                            $"Planilha de dizimistas exportada com sucesso!", "OK");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[INFO] Exportação cancelada pelo usuário");
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
                
                var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage != null)
                {
                    await mainPage.DisplayAlertAsync("Exportação", 
                        $"Planilha de dizimistas exportada com sucesso!\n\nLocalização: {filePath}", "OK");
                }
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao exportar: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ERRO] Stack trace: {ex.StackTrace}");
                var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
                var mainPage = windows is { Count: > 0 } ? windows[0].Page : null;
                if (mainPage != null)
                {
                    await mainPage.DisplayAlertAsync("Erro", $"Erro ao exportar: {ex.Message}", "OK");
                }
            }
        }

        [RelayCommand]
        public static async Task BaixarModeloAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[INFO] BaixarModeloAsync iniciado");
                
                var excelStream = DizimistaExcelService.GerarModelo();
                var fileName = "dizimistas_modelo.xlsx";

#if WINDOWS
                var result = await FileSaver.Default.SaveAsync(fileName, excelStream, CancellationToken.None);

                if (result.IsSuccessful)
                {
                    System.Diagnostics.Debug.WriteLine($"[INFO] Arquivo modelo salvo em: {result.FilePath}");

                    var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
                    var mainPage = windows is { Count: > 0 } ? windows[0].Page : null;
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
                System.Diagnostics.Debug.WriteLine($"[ERRO] Stack trace: {ex.StackTrace}");
                
                var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
                var mainPage = windows is { Count: > 0 } ? windows[0].Page : null;
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
                System.Diagnostics.Debug.WriteLine("[INFO] ImportarAsync iniciado");
                
                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    FileTypes = ExcelFileType,
                    PickerTitle = "Selecionar arquivo Excel de dizimistas para importar"
                });

                if (result == null)
                {
                    System.Diagnostics.Debug.WriteLine("[INFO] Importação cancelada pelo usuário");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[INFO] Arquivo selecionado: {result.FullPath}");
                var excelBytes = await File.ReadAllBytesAsync(result.FullPath);
                var dizimistas = await DizimistaExcelService.ImportarAsync(excelBytes);
                
                System.Diagnostics.Debug.WriteLine($"[INFO] {dizimistas.Count} dizimistas lidos do arquivo");
                
                foreach (var d in dizimistas)
                {
                    await _unitOfWork.Dizimistas.AddAsync(d);
                }
                await _unitOfWork.SaveChangesAsync();
                await CarregarDizimistasAsync();
                
                System.Diagnostics.Debug.WriteLine("[INFO] Importação concluída com sucesso");
                
                var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
                var mainPageSuccess = windows is { Count: > 0 } ? windows[0].Page : null;
                if (mainPageSuccess != null)
                {
                    await mainPageSuccess.DisplayAlertAsync("Importação", 
                        $"Importação concluída com sucesso!\n\n{dizimistas.Count} dizimista(s) importado(s).", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao importar: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ERRO] Stack trace: {ex.StackTrace}");
                
                var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
                var mainPageError = windows is { Count: > 0 } ? windows[0].Page : null;
                if (mainPageError != null)
                {
                    await mainPageError.DisplayAlertAsync("Erro", $"Erro ao importar: {ex.Message}", "OK");
                }
            }
        }

        [RelayCommand]
        public async Task GerarRelatorioGeralAsync()
        {
            await CarregarDizimistasAsync();
            var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
            var mainPage = windows is { Count: > 0 } ? windows[0].Page : null;
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
                sb.AppendLine("Relatório Geral de Dizimistas");
                sb.AppendLine($"Data: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                sb.AppendLine($"Total de Dizimistas: {Dizimistas.Count}");
                sb.AppendLine();
                
                var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
                var mainPage = windows is { Count: > 0 } ? windows[0].Page : null;
                if (mainPage != null)
                {
                    await mainPage.DisplayAlertAsync("Relatório", sb.ToString(), "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao gerar relatório: {ex.Message}");
                var windows = Microsoft.Maui.Controls.Application.Current?.Windows ;
                var mainPage = windows is { Count: > 0 } ? windows[0].Page : null;
                if (mainPage != null)
                {
                    await mainPage.DisplayAlertAsync("Erro", $"Erro ao gerar relatório: {ex.Message}", "OK");
                }
            }
        }

        [RelayCommand]
        public async Task ExcluirDizimistasSelecionadosAsync()
        {
            if (DizimistasSelecionados.Count == 0) return;
            var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
            var mainPage = windows is { Count: > 0 } ? windows[0].Page : null;
            if (mainPage != null)
            {
                bool confirm = await mainPage.DisplayAlertAsync("Confirmação", $"Deseja excluir {DizimistasSelecionados.Count} dizimista(s)?", "Sim", "Não");
                if (!confirm) return;
            }
            foreach (var dizimista in DizimistasSelecionados.ToList())
            {
                await _deleteHandler.Handle(new DeleteDizimistaCommand(dizimista.Id));
            }
            await CarregarDizimistasAsync();
            DizimistasSelecionados.Clear();
        }

        [RelayCommand]
        public async Task InativarDizimistasSelecionadosAsync()
        {
            if (DizimistasSelecionados.Count == 0) return;
            var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
            var mainPage = windows is { Count: > 0 } ? windows[0].Page : null;
            if (mainPage != null)
            {
                bool confirm = await mainPage.DisplayAlertAsync("Confirmação", $"Deseja ativar/inativar {DizimistasSelecionados.Count} dizimista(s)?", "Sim", "Não");
                if (!confirm) return;
            }
            foreach (var dizimista in DizimistasSelecionados.ToList())
            {
                await _inativarHandler.Handle(new InativarDizimistaCommand(dizimista.Id));
            }
            await CarregarDizimistasAsync();
            DizimistasSelecionados.Clear();
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
                    var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
                    var mainPage = windows is { Count: > 0 } ? windows[0].Page : null;
                    if (mainPage != null)
                        await mainPage.DisplayAlertAsync("Aviso", "Não foi possível abrir o navegador. Verifique se possui um navegador padrão configurado.", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao gerar relatório: {ex.Message}");
                var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
                var mainPage = windows is { Count: > 0 } ? windows[0].Page : null;
                if (mainPage != null)
                    await mainPage.DisplayAlertAsync("Erro", $"Erro ao gerar relatório: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Carrega TODOS os dizimistas com os filtros aplicados, mantendo a mesma ordem da paginação
        /// </summary>
        private async Task<List<Dizimista>> CarregarTodosDizimistasComFiltrosAsync()
        {
            var todosDizimistas = new List<Dizimista>();
            int pageNumber = 1;
            int totalPages = 1;

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
}
