using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Domain.Entities;
using Dizimo.Application.Dizimistas.Commands;
using Dizimo.Application.Dizimistas.Handlers;
using Dizimo.Application.Dizimistas.Queries;

namespace Dizimo.ViewModels;

public partial class DizimistaCadastroViewModel : ObservableObject, IQueryAttributable
{
    private readonly CreateDizimistaHandler _createHandler;
    private readonly UpdateDizimistaHandler _updateHandler;
    private readonly GetDizimistaHandlers _getHandler;

    [ObservableProperty] private int _numeroCadastro;
    [ObservableProperty] private string _nome = string.Empty;
    [ObservableProperty] private DateTime _dataNascimento = DateTime.Today;
    [ObservableProperty] private bool _ativo = true;
    [ObservableProperty] private Guid _id;
    [ObservableProperty] private bool _isEditMode;
    [ObservableProperty] private string _telefone = string.Empty;
    [ObservableProperty] private string _whatsapp = string.Empty;
    [ObservableProperty] private DateTime _dataCadastro = DateTime.Today;
    [ObservableProperty] private string _rua = string.Empty;
    [ObservableProperty] private string _numero = string.Empty;
    [ObservableProperty] private string _bairro = string.Empty;
    [ObservableProperty] private string _cidade = "Osasco";
    [ObservableProperty] private string _uf = "SP";
    [ObservableProperty] private string _cep = string.Empty;

    private string _complemento = string.Empty;

    public List<string> EstadosBrasileiros { get; } = new()
    {
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    };

    public DizimistaCadastroViewModel(CreateDizimistaHandler createHandler, UpdateDizimistaHandler updateHandler, GetDizimistaHandlers getHandler)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _getHandler = getHandler;
    }

    public Endereco Endereco => new Endereco
    {
        Rua = Rua,
        Numero = Numero,
        Complemento = _complemento,
        Bairro = Bairro,
        Cidade = Cidade,
        UF = Uf,
        CEP = Cep
    };

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var idObj) && Guid.TryParse(idObj?.ToString(), out var dizimistaId))
        {
            var dizimista = await _getHandler.Handle(new GetDizimistaByIdQuery(dizimistaId));
            if (dizimista != null)
            {
                Id = dizimista.Id;
                NumeroCadastro = dizimista.NumeroCadastro;
                Nome = dizimista.Nome;
                DataNascimento = dizimista.DataNascimento;
                Ativo = dizimista.Ativo;
                Telefone = dizimista.Telefone;
                Whatsapp = dizimista.Whatsapp;
                DataCadastro = dizimista.DataCadastro;
                Rua = dizimista.Endereco?.Rua ?? string.Empty;
                Numero = dizimista.Endereco?.Numero ?? string.Empty;
                _complemento = dizimista.Endereco?.Complemento ?? string.Empty;
                Bairro = dizimista.Endereco?.Bairro ?? string.Empty;
                Cidade = dizimista.Endereco?.Cidade ?? "Osasco";
                Uf = dizimista.Endereco?.UF ?? "SP";
                Cep = dizimista.Endereco?.CEP ?? string.Empty;
                IsEditMode = true;
            }
        }
        else
        {
            Id = Guid.Empty;
            NumeroCadastro = 0;
            Nome = string.Empty;
            DataNascimento = DateTime.Today;
            Ativo = true;
            Telefone = string.Empty;
            Whatsapp = string.Empty;
            DataCadastro = DateTime.Today;
            Rua = string.Empty;
            Numero = string.Empty;
            _complemento = string.Empty;
            Bairro = string.Empty;
            Cidade = "Osasco";
            Uf = "SP";
            Cep = string.Empty;
            IsEditMode = false;
        }
    }

    [RelayCommand]
    public async Task SalvarAsync()
    {
        if (IsEditMode)
        {
            await _updateHandler.Handle(new UpdateDizimistaCommand(Id, NumeroCadastro, Nome, DataNascimento, Ativo, Endereco, Telefone, Whatsapp, DataCadastro));
        }
        else
        {
            await _createHandler.Handle(new CreateDizimistaCommand(NumeroCadastro, Nome, DataNascimento, Endereco, Telefone, Whatsapp, DataCadastro));
        }
        await Shell.Current.GoToAsync("..", true);
    }
}
