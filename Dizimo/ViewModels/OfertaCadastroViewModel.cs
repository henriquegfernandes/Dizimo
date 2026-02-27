using System.Collections.ObjectModel;
using Dizimo.Application.Ofertas.Commands;
using Dizimo.Application.Ofertas.Handlers;
using Dizimo.Application.Ofertas.Queries;
using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Storage;

namespace Dizimo.ViewModels;

public partial class OfertaCadastroViewModel : ObservableObject, IQueryAttributable
{
    private readonly CreateOfertaHandler _createHandler;
    private readonly UpdateOfertaHandler _updateHandler;
    private readonly GetOfertaHandlers _getHandler;
    private readonly IUnitOfWork _unitOfWork;

    private static readonly FilePickerFileType ExcelFileType = new(new Dictionary<DevicePlatform, IEnumerable<string>>
    {
        { DevicePlatform.WinUI, new[] { ".xlsx" } },
        { DevicePlatform.macOS, new[] { ".xlsx" } },
        { DevicePlatform.iOS, new[] { "com.microsoft.excel.xlsx" } },
        { DevicePlatform.Android, new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" } },
    });

    private readonly string[] _mesesArray =
    [
        "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
        "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
    ];

    public OfertaCadastroViewModel(
        CreateOfertaHandler createHandler,
        UpdateOfertaHandler updateHandler,
        GetOfertaHandlers getHandler,
        IUnitOfWork unitOfWork)
    {
        _createHandler = createHandler ?? throw new ArgumentNullException(nameof(createHandler));
        _updateHandler = updateHandler ?? throw new ArgumentNullException(nameof(updateHandler));
        _getHandler = getHandler ?? throw new ArgumentNullException(nameof(getHandler));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        
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

    private DateTime _data = DateTime.Today;
    public DateTime DataOferta { get => _data; set => SetProperty(ref _data, value); }

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

    public IAsyncRelayCommand BuscarDizimistaCommand => new AsyncRelayCommand(BuscarDizimistaAsync);

    public async Task BuscarDizimistaAsync()
    {
        if (CodigoDizimista <= 0)
        {
            NomeDizimista = string.Empty;
            DizimistaEncontrado = false;
            DizimistaAtivo = false;
            DizimistaIdProp = Guid.Empty;
            return;
        }

        try
        {
            var dizimistas = await _unitOfWork.Dizimistas.GetAllAsync();
            var dizimista = dizimistas.FirstOrDefault(d => d.NumeroCadastro == CodigoDizimista);

            if (dizimista != null)
            {
                // Verificar se o dizimista está ativo
                if (!dizimista.Ativo)
                {
                    NomeDizimista = "Dizimista inativo";
                    DizimistaEncontrado = false;
                    DizimistaAtivo = false;
                    DizimistaIdProp = Guid.Empty;
                    var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
                    var mainPage = windows is { Count: > 0 } ? windows[0].Page : null;
                    if (mainPage != null)
                        await mainPage.DisplayAlertAsync("Aviso", $"O dizimista com código {CodigoDizimista} está inativo. Não é possível criar ofertas para dizimistas inativos.", "OK");
                    return;
                }

                NomeDizimista = dizimista.Nome;
                DizimistaIdProp = dizimista.Id;
                DizimistaEncontrado = true;
                DizimistaAtivo = true;
            }
            else
            {
                // Dizimista não encontrado - mostrar opção de criar novo
                NomeDizimista = string.Empty;
                DizimistaEncontrado = false;
                DizimistaAtivo = true; // Permitir prosseguir
                DizimistaIdProp = Guid.Empty;

                var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
                var mainPage = windows is { Count: > 0 } ? windows[0].Page : null;
                if (mainPage != null)
                {
                    bool criarNovo = await mainPage.DisplayAlertAsync(
                        "Dizimista Não Encontrado", 
                        $"Nenhum dizimista encontrado com o código {CodigoDizimista}.\n\n" +
                        $"Se você prosseguir com o cadastro, um novo dizimista será criado com este código. " +
                        $"Não esqueça de inserir o nome do dizimista no campo abaixo.", 
                        "Criar Novo", 
                        "Cancelar");

                    if (!criarNovo)
                    {
                        // Limpar os campos se o usuário cancelar
                        CodigoDizimista = 0;
                        NomeDizimista = string.Empty;
                        DizimistaEncontrado = false;
                        DizimistaAtivo = false;
                        DizimistaIdProp = Guid.Empty;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            NomeDizimista = "Erro ao buscar";
            DizimistaEncontrado = false;
            DizimistaAtivo = false;
            DizimistaIdProp = Guid.Empty;
            var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
            var mainPage = windows is { Count: > 0 } ? windows[0].Page : null;
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Erro", $"Erro ao buscar dizimista: {ex.Message}", "OK");
        }
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var idObj) && Guid.TryParse(idObj?.ToString(), out var ofertaId))
        {
            CarregarOfertaAsync(ofertaId).GetAwaiter().GetResult();
        }
        else
        {
            LimparCampos();
        }
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
        UsarRangoMeses = false;
    }

    public IAsyncRelayCommand SalvarCommand => new AsyncRelayCommand(SalvarAsync);

    public async Task SalvarAsync()
    {
        // Validações iniciais
        if (CodigoDizimista <= 0)
        {
            var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
            var mainPage = windows is { Count: > 0 } ? windows[0].Page : null;
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Validação", "Por favor, insira um código de dizimista.", "OK");
            return;
        }

        if (!DizimistaAtivo)
        {
            var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
            var mainPage = windows is { Count: > 0 } ? windows[0].Page : null;
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Validação", "O dizimista selecionado está inativo. Não é possível criar ofertas para dizimistas inativos.", "OK");
            return;
        }

        if (Valor <= 0)
        {
            var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
            var mainPage = windows is { Count: > 0 } ? windows[0].Page : null;
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Validação", "O valor da oferta deve ser maior que zero.", "OK");
            return;
        }

        // Validação para range de meses
        if (UsarRangoMeses)
        {
            int mesInicio = MesRef;
            int mesFim = MesRefFim;
            int anoInicio = AnoRef;
            int anoFim = AnoRefFim;

            // Criar datas para comparação
            var dataInicio = new DateTime(anoInicio, mesInicio, 1);
            var dataFim = new DateTime(anoFim, mesFim, 1);

            if (dataFim < dataInicio)
            {
                var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
                var mainPage = windows is { Count: > 0 } ? windows[0].Page : null;
                if (mainPage != null)
                    await mainPage.DisplayAlertAsync("Validação", "A data final deve ser maior ou igual à data de início. Por favor, verifique o período selecionado.", "OK");
                return;
            }
        }

        // Se o dizimista não foi encontrado, criar um novo
        if (DizimistaIdProp == Guid.Empty && !DizimistaEncontrado)
        {
            if (string.IsNullOrWhiteSpace(NomeDizimista))
            {
                var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
                var mainPage = windows is { Count: > 0 } ? windows[0].Page : null;
                if (mainPage != null)
                    await mainPage.DisplayAlertAsync("Validação", "Por favor, insira o nome do dizimista.", "OK");
                return;
            }

            try
            {
                // Criar novo dizimista
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

                var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
                var mainPage = windows is { Count: > 0 } ? windows[0].Page : null;
                if (mainPage != null)
                {
                    await mainPage.DisplayAlertAsync(
                        "Sucesso", 
                        $"Novo dizimista '{NomeDizimista}' (código {CodigoDizimista}) foi cadastrado com sucesso.", 
                        "OK");
                }
            }
            catch (Exception ex)
            {
                var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
                var mainPage = windows is { Count: > 0 } ? windows[0].Page : null;
                if (mainPage != null)
                    await mainPage.DisplayAlertAsync("Erro", $"Erro ao criar novo dizimista: {ex.Message}", "OK");
                return;
            }
        }

        if (!DizimistaEncontrado || DizimistaIdProp == Guid.Empty)
        {
            var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
            var mainPage = windows is { Count: > 0 } ? windows[0].Page : null;
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Validação", "Dizimista inválido. Por favor, verifique o código.", "OK");
            return;
        }

        try
        {
            var tipoPagamentoSelecionado = TipoPagamento.Replace("Cartão", "Cartao");
            TipoPagamento tipoPagamento = (TipoPagamento)Enum.Parse<TipoPagamento>(tipoPagamentoSelecionado);

            if (IsEditMode)
            {
                await _updateHandler.Handle(new UpdateOfertaCommand(Id, DizimistaIdProp, Valor, DataOferta, MesRef, AnoRef));
                // Atualizar tipo de pagamento
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
                    int mesInicio = MesRef;
                    int mesFim = MesRefFim;
                    int anoInicio = AnoRef;
                    int anoFim = AnoRefFim;

                    // Calcular quantidade total de meses no range
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

                    // Dividir valor igualmente entre os meses
                    decimal valorPorMes = qtdMeses > 0 ? Valor / qtdMeses : Valor;

                    // Criar ofertas para cada mês no range
                    mesTemp = mesInicio;
                    anoTemp = anoInicio;
                    while (anoTemp < anoFim || (anoTemp == anoFim && mesTemp <= mesFim))
                    {
                        var novaOferta = new Oferta
                        {
                            Id = Guid.NewGuid(),
                            DizimistaId = DizimistaIdProp,
                            Valor = valorPorMes,
                            Data = DataOferta,
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
                else
                {
                    // Oferta única
                    var novaOferta = new Oferta
                    {
                        Id = Guid.NewGuid(),
                        DizimistaId = DizimistaIdProp,
                        Valor = Valor,
                        Data = DataOferta,
                        MesReferencia = MesRef,
                        AnoReferencia = AnoRef,
                        TipoPagamento = tipoPagamento
                    };
                    await _unitOfWork.Ofertas.AddAsync(novaOferta);
                    await _unitOfWork.SaveChangesAsync();
                }
            }

            var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
            var mainPage = windows is { Count: > 0 } ? windows[0].Page : null;
            if (mainPage != null)
            {
                var resultado = await mainPage.DisplayAlertAsync(
                    "Sucesso",
                    "Oferta(s) salva(s) com sucesso! Deseja cadastrar outra oferta?",
                    "Sim",
                    "Não");

                if (resultado)
                {
                    // Limpar o formulário para novo cadastro
                    LimparCampos();
                }
                else
                {
                    // Ir para a lista de ofertas
                    await Shell.Current.GoToAsync("///ofertas", true);
                }
            }
            else
            {
                await Shell.Current.GoToAsync("///ofertas", true);
            }
        }
        catch (Exception ex)
        {
            var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
            var mainPage = windows is { Count: > 0 } ? windows[0].Page : null;
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Erro", $"Erro ao salvar oferta: {ex.Message}", "OK");
        }
    }

    public IAsyncRelayCommand ExcluirCommand => new AsyncRelayCommand(ExcluirAsync);

    public async Task ExcluirAsync()
    {
        if (IsEditMode && Id != Guid.Empty)
        {
            var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
            var mainPage = windows is { Count: > 0 } ? windows[0].Page : null;
            if (mainPage != null)
            {
                bool confirm = await mainPage.DisplayAlertAsync(
                    "Confirmação",
                    $"Deseja excluir a oferta de valor {Valor:C} em {DataOferta:dd/MM/yyyy}?",
                    "Sim", "Não");

                if (confirm)
                {
                    try
                    {
                        await _unitOfWork.Ofertas.DeleteAsync(Id);
                        await _unitOfWork.SaveChangesAsync();
                        await mainPage.DisplayAlertAsync("Sucesso", "Oferta excluída com sucesso.", "OK");
                        await Shell.Current.GoToAsync("///ofertas", true);
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
    public static async Task BaixarModeloAsync()
    {
        try
        {
            var excelService = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services.GetService<OfertaExcelService>()?? throw new InvalidOperationException("OfertaExcelService não está registrado no contêiner de serviços.");

            var templateStream = OfertaExcelService.GerarModelo();
            var fileName = $"oferta_modelo_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

#if WINDOWS
            var result = await FileSaver.Default.SaveAsync(fileName, templateStream, CancellationToken.None);

            var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
            var mainPageResult = windows is { Count: > 0 } ? windows[0].Page : null;
            if (mainPageResult != null)
            {
                if (result.IsSuccessful)
                    await mainPageResult.DisplayAlertAsync("Sucesso", "Planilha modelo baixada com sucesso!", "OK");
                else
                    await mainPageResult.DisplayAlertAsync("Erro", "Erro ao salvar o arquivo.", "OK");
            }
#else
            var downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");

            if (!Directory.Exists(downloadsPath))
            {
                Directory.CreateDirectory(downloadsPath);
            }

            var filePath = Path.Combine(downloadsPath, fileName);
            await File.WriteAllBytesAsync(filePath, templateStream.ToArray());

            var mainPageSuccess = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
            if (mainPageSuccess != null)
                await mainPageSuccess.DisplayAlertAsync("Sucesso", 
                    $"Planilha modelo baixada com sucesso!\n\nLocalização: {filePath}", "OK");
#endif
        }
        catch (Exception ex)
        {
            var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
            var mainPageError = windows is { Count: > 0 } ? windows[0].Page : null;
            if (mainPageError != null)
                await mainPageError.DisplayAlertAsync("Erro", $"Erro ao baixar modelo: {ex.Message}", "OK");
        }
    }

    public IAsyncRelayCommand ImportarCommand => new AsyncRelayCommand(ImportarAsync);

    public async Task ImportarAsync()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Selecione a planilha de ofertas",
                FileTypes = ExcelFileType
            });

            if (result == null)
                return;

            var excelService = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services.GetService<OfertaExcelService>();

            if (excelService == null)
            {
                var windowsNull = Microsoft.Maui.Controls.Application.Current?.Windows;
                var mainPageNull = windowsNull is { Count: > 0 } ? windowsNull[0].Page : null;
                if (mainPageNull != null)
                    await mainPageNull.DisplayAlertAsync("Erro", "Serviço de Excel não está disponível.", "OK");
                return;
            }

            using var stream = await result.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();
            var resultado = await excelService.ImportarAsync(fileBytes);

            if (resultado.OfertasImportadas.Count > 0)
            {
                var windowsConfirm = Microsoft.Maui.Controls.Application.Current?.Windows;
                var mainPage = windowsConfirm is { Count: > 0 } ? windowsConfirm[0].Page : null;
                if (mainPage != null)
                {
                    bool confirmar = await mainPage.DisplayAlertAsync(
                        "Confirmar Importação",
                        $"{resultado.OfertasImportadas.Count} oferta(s) encontrada(s). Deseja importar?",
                        "Sim", "Não");

                    if (confirmar)
                    {
                        foreach (var oferta in resultado.OfertasImportadas)
                        {
                            await _unitOfWork.Ofertas.AddAsync(oferta);
                        }
                        await _unitOfWork.SaveChangesAsync();

                        await mainPage.DisplayAlertAsync("Sucesso", 
                            $"{resultado.OfertasImportadas.Count} oferta(s) importada(s) com sucesso!", "OK");

                        // Navegar de volta para a lista de ofertas
                        await Shell.Current.GoToAsync("///ofertas", true);
                    }
                }
            }
            else
            {
                var windowsEmpty = Microsoft.Maui.Controls.Application.Current?.Windows;
                var mainPageEmpty = windowsEmpty is { Count: > 0 } ? windowsEmpty[0].Page : null;
                if (mainPageEmpty != null)
                    await mainPageEmpty.DisplayAlertAsync("Aviso", "Nenhuma oferta encontrada na planilha.", "OK");
            }
        }
        catch (Exception ex)
        {
            var windowsError = Microsoft.Maui.Controls.Application.Current?.Windows;
            var mainPageError = windowsError is { Count: > 0 } ? windowsError[0].Page : null;
            if (mainPageError != null)
                await mainPageError.DisplayAlertAsync("Erro", $"Erro ao importar: {ex.Message}", "OK");
        }
    }
}
