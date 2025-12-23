using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Dizimo.Application.Ofertas.Commands;
using Dizimo.Application.Ofertas.Handlers;
using Dizimo.Application.Ofertas.Queries;
using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Dizimo.ViewModels;

public partial class OfertaCadastroViewModel : ObservableObject, IQueryAttributable
{
    private readonly CreateOfertaHandler _createHandler;
    private readonly UpdateOfertaHandler _updateHandler;
    private readonly GetOfertaHandlers _getHandler;
    private readonly IUnitOfWork _unitOfWork;

    private readonly string[] _mesesArray = new[]
    {
        "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
        "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
    };

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

    public IAsyncRelayCommand BuscarDizimistaCommand => new AsyncRelayCommand(BuscarDizimistaAsync);

    public async Task BuscarDizimistaAsync()
    {
        if (CodigoDizimista <= 0)
        {
            NomeDizimista = string.Empty;
            DizimistaEncontrado = false;
            DizimistaIdProp = Guid.Empty;
            return;
        }

        try
        {
            var dizimistas = await _unitOfWork.Dizimistas.GetAllAsync();
            var dizimista = dizimistas.FirstOrDefault(d => d.NumeroCadastro == CodigoDizimista);

            if (dizimista != null)
            {
                NomeDizimista = dizimista.Nome;
                DizimistaIdProp = dizimista.Id;
                DizimistaEncontrado = true;
            }
            else
            {
                NomeDizimista = "Dizimista não encontrado";
                DizimistaEncontrado = false;
                DizimistaIdProp = Guid.Empty;
                var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage != null)
                    await mainPage.DisplayAlertAsync("Aviso", $"Nenhum dizimista encontrado com o código {CodigoDizimista}", "OK");
            }
        }
        catch (Exception ex)
        {
            NomeDizimista = "Erro ao buscar";
            DizimistaEncontrado = false;
            DizimistaIdProp = Guid.Empty;
            var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
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
        UsarRangoMeses = false;
    }

    public IAsyncRelayCommand SalvarCommand => new AsyncRelayCommand(SalvarAsync);

    public async Task SalvarAsync()
    {
        if (!DizimistaEncontrado || DizimistaIdProp == Guid.Empty)
        {
            var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Validação", "Dizimista não encontrado. Por favor, insira um código válido.", "OK");
            return;
        }

        if (Valor <= 0)
        {
            var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
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
                var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage != null)
                    await mainPage.DisplayAlertAsync("Validação", "A data final deve ser maior ou igual à data de início. Por favor, verifique o período selecionado.", "OK");
                return;
            }
        }

        try
        {
            var tipoPagamentoSelecionado = TipoPagamento.Replace("Cartão", "Cartao");
            var tipoPagamento = (Dizimo.Domain.Entities.TipoPagamento)Enum.Parse(typeof(Dizimo.Domain.Entities.TipoPagamento), tipoPagamentoSelecionado);

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

            var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Sucesso", "Oferta(s) salva(s) com sucesso!", "OK");

            await Shell.Current.GoToAsync("///ofertas", true);
        }
        catch (Exception ex)
        {
            var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Erro", $"Erro ao salvar oferta: {ex.Message}", "OK");
        }
    }

    public IAsyncRelayCommand ExcluirCommand => new AsyncRelayCommand(ExcluirAsync);

    public async Task ExcluirAsync()
    {
        if (IsEditMode && Id != Guid.Empty)
        {
            var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
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
}
