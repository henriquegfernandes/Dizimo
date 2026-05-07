using Dizimo.Data;
using Dizimo.Models;
using Dizimo.Services;
using Dizimo.Utilities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using Dizimo.Application.Ofertas.Commands;
using Dizimo.Application.Ofertas.Handlers;
using Dizimo.Application.Ofertas.Queries;
using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;
using Dizimo.Application.Reporting.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System.IO;

namespace Dizimo.ViewModels;

public partial class OfertaCadastroViewModel : ObservableObject, INavigationAware
{
    private readonly CreateOfertaHandler _createHandler;
    private readonly UpdateOfertaHandler _updateHandler;
    private readonly GetOfertaHandlers _getHandler;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;

    private readonly string[] _mesesArray =
    [
        "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
        "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
    ];

    public OfertaCadastroViewModel(
        CreateOfertaHandler createHandler,
        UpdateOfertaHandler updateHandler,
        GetOfertaHandlers getHandler,
        IUnitOfWork unitOfWork,
        INavigationService navigationService,
        IDialogService dialogService)
    {
        _createHandler = createHandler ?? throw new ArgumentNullException(nameof(createHandler));
        _updateHandler = updateHandler ?? throw new ArgumentNullException(nameof(updateHandler));
        _getHandler = getHandler ?? throw new ArgumentNullException(nameof(getHandler));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        
        var meses = new ObservableCollection<string>
        {
            "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
            "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
        };
        MesesNomesCollection = meses;

        var anos = new ObservableCollection<int>();
        int anoAtual = DateTime.Now.Year;
        for (int i = anoAtual - 5; i <= anoAtual + 5; i++)
            anos.Add(i);
        AnosCollection = anos;

        var tiposPagamento = new ObservableCollection<string>
        {
            "PIX",
            "Dinheiro",
            "Cartão"
        };
        TiposPagamentoCollection = tiposPagamento;
    }

    public ObservableCollection<string> MesesNomesCollection { get; }
    public ObservableCollection<int> AnosCollection { get; }
    public ObservableCollection<string> TiposPagamentoCollection { get; }

    private Guid _id;
    public Guid Id { get => _id; set => SetProperty(ref _id, value); }

    private int _codigoDizimista;
    public int CodigoDizimista { get => _codigoDizimista; set => SetProperty(ref _codigoDizimista, value); }

    private string _nomeDizimista = "";
    public string NomeDizimista { get => _nomeDizimista; set => SetProperty(ref _nomeDizimista, value); }

    private Guid _dizimistaId;
    public Guid DizimistaIdProp { get => _dizimistaId; set => SetProperty(ref _dizimistaId, value); }

    private decimal _valor;
    public decimal Valor { get => _valor; set => SetProperty(ref _valor, value); }

    private DateTime? _data = DateTime.Today;
    public DateTime? DataOferta { get => _data; set => SetProperty(ref _data, value); }

    private string _tipoPagamento = "PIX";
    public string TipoPagamento { get => _tipoPagamento; set => SetProperty(ref _tipoPagamento, value); }

    private bool _usarRangoMeses = false;
    public bool UsarRangoMeses 
    { 
        get => _usarRangoMeses; 
        set => SetProperty(ref _usarRangoMeses, value);
    }

    private int _mesReferencia = DateTime.Today.Month;
    public int MesRef 
    { 
        get => _mesReferencia; 
        set 
        { 
            SetProperty(ref _mesReferencia, value);
            OnPropertyChanged(nameof(MesNomeAtual));
        }
    }

    public string MesNomeAtual
    {
        get => _mesReferencia >= 1 && _mesReferencia <= 12 ? _mesesArray[_mesReferencia - 1] : _mesesArray[0];
        set
        {
            for (int i = 0; i < _mesesArray.Length; i++)
            {
                if (_mesesArray[i].Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    MesRef = i + 1;
                    return;
                }
            }
        }
    }

    private int _mesReferenciaFim = DateTime.Today.Month;
    public int MesRefFim 
    { 
        get => _mesReferenciaFim; 
        set 
        { 
            SetProperty(ref _mesReferenciaFim, value);
            OnPropertyChanged(nameof(MesNomeAtualFim));
        }
    }

    public string MesNomeAtualFim
    {
        get => _mesReferenciaFim >= 1 && _mesReferenciaFim <= 12 ? _mesesArray[_mesReferenciaFim - 1] : _mesesArray[0];
        set
        {
            for (int i = 0; i < _mesesArray.Length; i++)
            {
                if (_mesesArray[i].Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    MesRefFim = i + 1;
                    return;
                }
            }
        }
    }

    private int _anoReferencia = DateTime.Today.Year;
    public int AnoRef 
    { 
        get => _anoReferencia; 
        set 
        { 
            SetProperty(ref _anoReferencia, value);
            OnPropertyChanged(nameof(AnoSelecionado));
        }
    }

    public int AnoSelecionado
    {
        get => _anoReferencia;
        set
        {
            if (_anoReferencia != value)
            {
                AnoRef = value;
            }
        }
    }

    private int _anoReferenciaFim = DateTime.Today.Year;
    public int AnoRefFim 
    { 
        get => _anoReferenciaFim; 
        set 
        { 
            SetProperty(ref _anoReferenciaFim, value);
            OnPropertyChanged(nameof(AnoSelecionadoFim));
        }
    }

    public int AnoSelecionadoFim
    {
        get => _anoReferenciaFim;
        set
        {
            if (_anoReferenciaFim != value)
            {
                AnoRefFim = value;
            }
        }
    }

    private bool _isEditMode;
    public bool IsEditMode { get => _isEditMode; set => SetProperty(ref _isEditMode, value); }

    private bool _dizimistaEncontrado;
    public bool DizimistaEncontrado { get => _dizimistaEncontrado; set => SetProperty(ref _dizimistaEncontrado, value); }

    private bool _dizimistaAtivo;
    public bool DizimistaAtivo { get => _dizimistaAtivo; set => SetProperty(ref _dizimistaAtivo, value); }

    private bool _nomeDizimistaEditable;
    /// <summary>
    /// Controla se o campo de nome do dizimista pode ser editado
    /// Fica habilitado quando o dizimista não é encontrado e o usuário opta por criar um novo
    /// </summary>
    public bool NomeDizimistaEditable { get => _nomeDizimistaEditable; set => SetProperty(ref _nomeDizimistaEditable, value); }

    [RelayCommand]
    public async Task BuscarDizimistaAsync()
    {
        if (CodigoDizimista <= 0)
        {
            ResetarDizimista();
            return;
        }

        try
        {
            var dizimistas = await _unitOfWork.Dizimistas.GetAllAsync();
            var dizimista = dizimistas.FirstOrDefault(d => d.NumeroCadastro == CodigoDizimista);

            if (dizimista != null)
            {
                if (!dizimista.Ativo)
                {
                    await _dialogService.ShowInfoAsync("Dizimista Inativo", $"O dizimista com código {CodigoDizimista} está inativo.");
                    ResetarDizimista();
                    return;
                }

                NomeDizimista = dizimista.Nome;
                DizimistaIdProp = dizimista.Id;
                DizimistaEncontrado = true;
                DizimistaAtivo = true;
                NomeDizimistaEditable = false;
            }
            else
            {
                var confirmar = await _dialogService.ShowConfirmAsync(
                    "Dizimista Não Encontrado",
                    $"Não existe um dizimista com o código {CodigoDizimista}.\n\nDeseja criar um novo dizimista com esse código?",
                    "Sim, Criar",
                    "Não");

                if (confirmar)
                {
                    NomeDizimista = string.Empty;
                    DizimistaIdProp = Guid.Empty;
                    DizimistaEncontrado = false;
                    DizimistaAtivo = true;
                    NomeDizimistaEditable = true;
                }
                else
                {
                    ResetarDizimista();
                }
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Erro ao buscar dizimista: {ex.Message}");
            ResetarDizimista();
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao buscar dizimista: {ex.Message}");
        }
    }

    private void ResetarDizimista()
    {
        NomeDizimista = string.Empty;
        CodigoDizimista = 0;
        DizimistaIdProp = Guid.Empty;
        DizimistaEncontrado = false;
        DizimistaAtivo = false;
        NomeDizimistaEditable = false;
    }

    private async Task CarregarOfertaAsync(Guid ofertaId)
    {
        var oferta = await _getHandler.Handle(new GetOfertaByIdQuery(ofertaId));
        if (oferta != null)
        {
            Id = oferta.Id;
            DizimistaIdProp = oferta.DizimistaId;
            Valor = oferta.Valor;
            DataOferta = oferta.Data;
            MesRef = oferta.MesReferencia;
            AnoRef = oferta.AnoReferencia;
            TipoPagamento = oferta.TipoPagamento.ToString().Replace("Cartao", "Cartão");
            IsEditMode = true;
            UsarRangoMeses = false;

            var dizimista = await _unitOfWork.Dizimistas.GetByIdAsync(DizimistaIdProp);
            if (dizimista != null)
            {
                CodigoDizimista = dizimista.NumeroCadastro;
                NomeDizimista = dizimista.Nome;
                DizimistaEncontrado = true;
                DizimistaAtivo = dizimista.Ativo;
            }
        }
    }

    private void LimparCampos()
    {
        Id = Guid.Empty;
        CodigoDizimista = 0;
        NomeDizimista = string.Empty;
        DizimistaIdProp = Guid.Empty;
        Valor = 0;
        DataOferta = DateTime.Today;
        MesRef = DateTime.Today.Month;
        MesRefFim = DateTime.Today.Month;
        AnoRef = DateTime.Today.Year;
        AnoRefFim = DateTime.Today.Year;
        TipoPagamento = "PIX";
        IsEditMode = false;
        DizimistaEncontrado = false;
        DizimistaAtivo = false;
        NomeDizimistaEditable = false;
        UsarRangoMeses = false;
    }

    [RelayCommand]
    public async Task SalvarAsync()
    {
        // Validações iniciais
        if (CodigoDizimista <= 0 || !DizimistaAtivo || Valor <= 0)
        {
            System.Diagnostics.Debug.WriteLine("[ERRO] Validação: dados inválidos");
            return;
        }

        // Validação para range de meses
        if (UsarRangoMeses)
        {
            var dataInicio = new DateTime(AnoRef, MesRef, 1);
            var dataFim = new DateTime(AnoRefFim, MesRefFim, 1);

            if (dataFim < dataInicio)
            {
                System.Diagnostics.Debug.WriteLine("[ERRO] Período de datas inválido");
                return;
            }
        }

        // Criar novo dizimista se necessário
        if (DizimistaIdProp == Guid.Empty && !DizimistaEncontrado)
        {
            if (string.IsNullOrWhiteSpace(NomeDizimista))
            {
                System.Diagnostics.Debug.WriteLine("[ERRO] Nome do dizimista não preenchido");
                return;
            }

            try
            {
                var novoDizimista = new Dizimista
                {
                    Id = Guid.NewGuid(),
                    NumeroCadastro = CodigoDizimista,
                    Nome = NomeDizimista,
                    DataNascimento = DateTime.Today,
                    Ativo = true,
                    Endereco = new Endereco(),
                    Telefone = string.Empty,
                    Whatsapp = string.Empty,
                    DataCadastro = DateTime.Today
                };

                await _unitOfWork.Dizimistas.AddAsync(novoDizimista);
                await _unitOfWork.SaveChangesAsync();

                DizimistaIdProp = novoDizimista.Id;
                DizimistaEncontrado = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao criar novo dizimista: {ex.Message}");
                return;
            }
        }

        if (!DizimistaEncontrado || DizimistaIdProp == Guid.Empty)
        {
            System.Diagnostics.Debug.WriteLine("[ERRO] Dizimista inválido");
            return;
        }

        try
        {
            var tipoPagamentoSelecionado = TipoPagamento.Replace("Cartão", "Cartao");
            TipoPagamento tipoPagamento = (TipoPagamento)Enum.Parse<TipoPagamento>(tipoPagamentoSelecionado);

            if (IsEditMode)
            {
                await _updateHandler.Handle(new UpdateOfertaCommand(Id, DizimistaIdProp, Valor, DataOferta ?? DateTime.Today, MesRef, AnoRef));
                var oferta = await _unitOfWork.Ofertas.GetByIdAsync(Id);
                if (oferta != null)
                {
                    oferta.TipoPagamento = tipoPagamento;
                    await _unitOfWork.Ofertas.UpdateAsync(oferta);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            else
            {
                if (UsarRangoMeses)
                {
                    await CriarOfertasRangoAsync(tipoPagamento);
                }
                else
                {
                    var novaOferta = new Oferta
                    {
                        Id = Guid.NewGuid(),
                        DizimistaId = DizimistaIdProp,
                        Valor = Valor,
                        Data = DataOferta ?? DateTime.Today,
                        MesReferencia = MesRef,
                        AnoReferencia = AnoRef,
                        TipoPagamento = tipoPagamento
                    };
                    await _unitOfWork.Ofertas.AddAsync(novaOferta);
                    await _unitOfWork.SaveChangesAsync();
                }
            }

            if (!IsEditMode)
            {
                System.Diagnostics.Debug.WriteLine("[INFO] Oferta(s) salva(s) com sucesso");
                
                var criarOutra = await _dialogService.ShowConfirmAsync(
                    "Oferta Criada com Sucesso!",
                    "Deseja cadastrar outra oferta?",
                    "Sim, Criar Outra",
                    "Não, Voltar");

                if (criarOutra)
                {
                    LimparCampos();
                }
                else
                {
                    _navigationService.GoBack();
                }
            }
            else
            {
                _navigationService.GoBack();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao salvar oferta: {ex.Message}");
        }
    }

    private async Task CriarOfertasRangoAsync(TipoPagamento tipoPagamento)
    {
        int mesInicio = MesRef;
        int mesFim = MesRefFim;
        int anoInicio = AnoRef;
        int anoFim = AnoRefFim;

        int qtdMeses = CalcularQuantidadeMeses(mesInicio, mesFim, anoInicio, anoFim);
        decimal valorPorMes = qtdMeses > 0 ? Valor / qtdMeses : Valor;

        int mesTemp = mesInicio;
        int anoTemp = anoInicio;
        
        while (anoTemp < anoFim || (anoTemp == anoFim && mesTemp <= mesFim))
        {
            var novaOferta = new Oferta
            {
                Id = Guid.NewGuid(),
                DizimistaId = DizimistaIdProp,
                Valor = valorPorMes,
                Data = DataOferta ?? DateTime.Today,
                MesReferencia = mesTemp,
                AnoReferencia = anoTemp,
                TipoPagamento = tipoPagamento
            };
            await _unitOfWork.Ofertas.AddAsync(novaOferta);
            
            mesTemp++;
            if (mesTemp > 12)
            {
                mesTemp = 1;
                anoTemp++;
            }
        }
        
        await _unitOfWork.SaveChangesAsync();
    }

    private static int CalcularQuantidadeMeses(int mesInicio, int mesFim, int anoInicio, int anoFim)
    {
        int qtdMeses = 0;
        int mesTemp = mesInicio;
        int anoTemp = anoInicio;

        while (anoTemp < anoFim || (anoTemp == anoFim && mesTemp <= mesFim))
        {
            qtdMeses++;
            mesTemp++;
            if (mesTemp > 12)
            {
                mesTemp = 1;
                anoTemp++;
            }
        }

        return qtdMeses;
    }
    [RelayCommand]
    public void Voltar()
    {
        try
        {
            _navigationService.GoBack();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao voltar: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task ExcluirAsync()
    {
        if (IsEditMode && Id != Guid.Empty)
        {
            try
            {
                await _unitOfWork.Ofertas.DeleteAsync(Id);
                await _unitOfWork.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine("[INFO] Oferta excluída com sucesso");
                _navigationService.GoBack();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao excluir oferta: {ex.Message}");
            }
        }
    }

    [RelayCommand]
    public async Task BaixarModeloAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[INFO] BaixarModeloAsync iniciado");
            
            var fileName = $"oferta_modelo_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            
            if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.ClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
            {
                try
                {
                    var storageProvider = desktop.MainWindow.StorageProvider;
                    
                    if (storageProvider != null)
                    {
                        var file = await storageProvider.SaveFilePickerAsync(new()
                        {
                            Title = "Salvar Planilha Modelo",
                            DefaultExtension = "xlsx",
                            FileTypeChoices = new[] { new FilePickerFileType("Arquivo Excel") { Patterns = new[] { "*.xlsx" } } },
                            SuggestedFileName = fileName
                        });

                        if (file != null)
                        {
                            var excelStream = OfertaExcelService.GerarModelo();
                            await using var fileStream = await file.OpenWriteAsync();
                            await fileStream.WriteAsync(excelStream.ToArray());
                            System.Diagnostics.Debug.WriteLine($"[INFO] Arquivo modelo salvo com sucesso em: {file.Path}");
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao abrir file picker: {ex.Message}");
                }
            }

            // Fallback: salvar em Downloads
            var excelStreamFallback = OfertaExcelService.GerarModelo();
            var downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");

            if (!Directory.Exists(downloadsPath))
                Directory.CreateDirectory(downloadsPath);

            var filePath = Path.Combine(downloadsPath, fileName);
            await File.WriteAllBytesAsync(filePath, excelStreamFallback.ToArray());

            System.Diagnostics.Debug.WriteLine($"[INFO] Arquivo modelo salvo em: {filePath}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao baixar modelo: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task ImportarAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[INFO] ImportarAsync iniciado");
            
            if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.ClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
            {
                try
                {
                    var storageProvider = desktop.MainWindow.StorageProvider;
                    if (storageProvider != null)
                    {
                        var files = await storageProvider.OpenFilePickerAsync(new()
                        {
                            Title = "Selecionar Planilha para Importar",
                            AllowMultiple = false,
                            FileTypeFilter = new[] { new FilePickerFileType("Arquivos Excel") { Patterns = new[] { "*.xlsx", "*.xls" } } }
                        });

                        if (files.Count > 0)
                        {
                            var file = files[0];
                            System.Diagnostics.Debug.WriteLine($"[INFO] Arquivo selecionado: {file.Name}");

                            await using var stream = await file.OpenReadAsync();
                            using var memoryStream = new MemoryStream();
                            await stream.CopyToAsync(memoryStream);
                            var excelBytes = memoryStream.ToArray();

                            var excelService = new OfertaExcelService(_unitOfWork);
                            var resultado = await excelService.ImportarAsync(excelBytes);
                            
                            if (resultado.OfertasImportadas.Count == 0)
                            {
                                var mensagemErro = resultado.Erros.Count > 0 
                                    ? $"Nenhuma oferta foi importada.\n\nErros:\n{string.Join("\n", resultado.Erros.Take(5))}{(resultado.Erros.Count > 5 ? $"\n... e mais {resultado.Erros.Count - 5} erro(s)" : "")}"
                                    : "Nenhuma oferta foi encontrada no arquivo.";
                                    
                                await _dialogService.ShowAlertAsync("Importação", mensagemErro);
                                return;
                            }

                            System.Diagnostics.Debug.WriteLine($"[INFO] {resultado.OfertasImportadas.Count} ofertas lidas da planilha");

                            int sucessos = 0;
                            int erros = resultado.Erros.Count;

                            foreach (var oferta in resultado.OfertasImportadas)
                            {
                                try
                                {
                                    await _unitOfWork.Ofertas.AddAsync(oferta);
                                    sucessos++;
                                }
                                catch (Exception ex)
                                {
                                    erros++;
                                    System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao importar oferta: {ex.Message}");
                                }
                            }

                            try
                            {
                                await _unitOfWork.SaveChangesAsync();
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao salvar ofertas: {ex.Message}");
                                await _dialogService.ShowErrorAsync($"Erro ao salvar ofertas: {ex.Message}");
                                return;
                            }

                            System.Diagnostics.Debug.WriteLine($"[INFO] Importação concluída: {sucessos} sucesso(s), {erros} erro(s)");

                            var mensagem = $"Importação concluída!\n\n✓ {sucessos} oferta(s) importada(s) com sucesso";
                            if (erros > 0)
                                mensagem += $"\n✗ {erros} erro(s)/aviso(s) durante a importação";

                            mensagem += "\n\nDeseja cadastrar outra oferta ou voltar para a lista?";

                            var result = await _dialogService.ShowConfirmAsync(
                                "Importação Concluída",
                                mensagem,
                                "Cadastrar Outra",
                                "Voltar para Lista");

                            if (!result)
                            {
                                _navigationService.GoBack();
                            }
                            else
                            {
                                LimparCampos();
                            }
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao abrir file picker: {ex.Message}");
                    await _dialogService.ShowErrorAsync($"Erro ao abrir file picker: {ex.Message}");
                }
            }

            System.Diagnostics.Debug.WriteLine("[ERRO] StorageProvider não disponível");
            await _dialogService.ShowErrorAsync("StorageProvider não disponível");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao importar: {ex.Message}");
            await _dialogService.ShowErrorAsync($"Erro ao importar: {ex.Message}");
        }
    }

    /// <summary>
    /// Implementação de INavigationAware - Carrega oferta quando navega para esta página
    /// </summary>
    public void OnNavigatedTo(NavigationParameters parameters)
    {
        if (parameters.TryGetValue("id", out var idObj) && Guid.TryParse(idObj?.ToString(), out var ofertaId))
        {
            CarregarOfertaAsync(ofertaId).GetAwaiter().GetResult();
        }
        else
        {
            LimparCampos();
        }
    }

    /// <summary>
    /// Implementação de INavigationAware - Limpa estado ao sair da página
    /// </summary>
    public void OnNavigatedFrom()
    {
    }
}
