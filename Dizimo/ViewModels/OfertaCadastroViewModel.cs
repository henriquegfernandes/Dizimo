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
    }

    public ObservableCollection<string> MesesNomesCollection { get; }
    public ObservableCollection<int> AnosCollection { get; }

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

    private int _mesReferencia = DateTime.Today.Month;
    public int MesRef 
    { 
        get => _mesReferencia; 
        set 
        { 
            SetProperty(ref _mesReferencia, value);
            // Atualizar o nome do mês quando MesRef muda
            OnPropertyChanged(nameof(MesNomeAtual));
        }
    }

    public string MesNomeAtual
    {
        get => _mesReferencia >= 1 && _mesReferencia <= 12 ? _mesesArray[_mesReferencia - 1] : _mesesArray[0];
        set
        {
            // Converter nome do mês para int
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

    private int _anoReferencia = DateTime.Today.Year;
    public int AnoRef 
    { 
        get => _anoReferencia; 
        set 
        { 
            SetProperty(ref _anoReferencia, value);
            // Atualizar o ano quando AnoRef muda
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
            IsEditMode = true;

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
        AnoRef = DateTime.Today.Year;
        IsEditMode = false;
        DizimistaEncontrado = false;
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

        try
        {
            if (IsEditMode)
                await _updateHandler.Handle(new UpdateOfertaCommand(Id, DizimistaIdProp, Valor, DataOferta, MesRef, AnoRef));
            else
                await _createHandler.Handle(new CreateOfertaCommand(DizimistaIdProp, Valor, DataOferta, MesRef, AnoRef));

            var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Sucesso", "Oferta salva com sucesso!", "OK");

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
