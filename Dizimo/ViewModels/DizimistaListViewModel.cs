using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Domain.Entities;
using Dizimo.Application.Dizimistas.Queries;
using Dizimo.Application.Dizimistas.Handlers;
using Dizimo.Application.Dizimistas.Commands;
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

        private ObservableCollection<Dizimista> _dizimistas = new ObservableCollection<Dizimista>();

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
            set => SetProperty(ref _filtroNome, value);
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

        public List<string> StatusOptions { get; } = new() { "Todos", "Ativos", "Inativos" };
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

        private ObservableCollection<Dizimista> _dizimistasSelecionados = new();
        public ObservableCollection<Dizimista> DizimistasSelecionados
        {
            get => _dizimistasSelecionados;
            set
            {
                if (_dizimistasSelecionados != null)
                    _dizimistasSelecionados.CollectionChanged -= DizimistasSelecionados_CollectionChanged;
                SetProperty(ref _dizimistasSelecionados, value);
                if (_dizimistasSelecionados != null)
                    _dizimistasSelecionados.CollectionChanged += DizimistasSelecionados_CollectionChanged;
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
            Dizimistas = new ObservableCollection<Dizimista>();
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
        public async Task CarregarMaisDizimistasAsync()
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

                foreach (var dizimista in result.Items)
                {
                    Dizimistas.Add(dizimista);
                }

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
        public async Task NovoDizimistaCommand()
        {
            System.Diagnostics.Debug.WriteLine("[INFO] Comando NovoDizimista executado!");
            await Shell.Current.GoToAsync("dizimista-cadastro");
        }

        [RelayCommand]
        public async Task EditarDizimistaCommand(Dizimista dizimista)
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
        public async Task VerDetalhesDizimistaCommand(Dizimista dizimista)
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
                var excelStream = await _excelService.ExportarAsync();
                
                var fileName = $"dizimistas_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                
#if WINDOWS
                var result = await FileSaver.Default.SaveAsync(fileName, excelStream, CancellationToken.None);

                if (result.IsSuccessful)
                {
                    System.Diagnostics.Debug.WriteLine($"[INFO] Arquivo exportado para: {result.FilePath}");

                    var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
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
                System.Diagnostics.Debug.WriteLine("[INFO] BaixarModeloAsync iniciado");
                
                var excelStream = _excelService.GerarModelo();
                var fileName = "dizimistas_modelo.xlsx";

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
                System.Diagnostics.Debug.WriteLine($"[ERRO] Stack trace: {ex.StackTrace}");
                
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
                System.Diagnostics.Debug.WriteLine("[INFO] ImportarAsync iniciado");
                
                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.WinUI, new[] { ".xlsx" } },
                        { DevicePlatform.Android, new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" } },
                        { DevicePlatform.iOS, new[] { "com.microsoft.excel.xlsx" } }
                    }),
                    PickerTitle = "Selecionar arquivo Excel de dizimistas para importar"
                });

                if (result == null)
                {
                    System.Diagnostics.Debug.WriteLine("[INFO] Importação cancelada pelo usuário");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[INFO] Arquivo selecionado: {result.FullPath}");
                var excelBytes = await File.ReadAllBytesAsync(result.FullPath);
                var dizimistas = await _excelService.ImportarAsync(excelBytes);
                
                System.Diagnostics.Debug.WriteLine($"[INFO] {dizimistas.Count} dizimistas lidos do arquivo");
                
                foreach (var d in dizimistas)
                {
                    await _unitOfWork.Dizimistas.AddAsync(d);
                }
                await _unitOfWork.SaveChangesAsync();
                await CarregarDizimistasAsync();
                
                System.Diagnostics.Debug.WriteLine("[INFO] Importação concluída com sucesso");
                
                var mainPageSuccess = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
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
                
                var mainPageError = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
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
                sb.AppendLine("Relatório Geral de Dizimistas");
                sb.AppendLine($"Data: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                sb.AppendLine($"Total de Dizimistas: {Dizimistas.Count}");
                sb.AppendLine();
                
                var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage != null)
                {
                    await mainPage.DisplayAlertAsync("Relatório", sb.ToString(), "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao gerar relatório: {ex.Message}");
                var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
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
            var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
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
            var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
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
    }
}
