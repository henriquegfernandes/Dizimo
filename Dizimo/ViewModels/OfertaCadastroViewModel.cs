using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;
using Dizimo.Application.Ofertas.Commands;
using Dizimo.Application.Ofertas.Handlers;
using Dizimo.Application.Ofertas.Queries;
using System;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Dizimo.ViewModels;

public partial class OfertaCadastroViewModel : ObservableObject, IQueryAttributable
{
    private readonly CreateOfertaHandler _createHandler;
    private readonly UpdateOfertaHandler _updateHandler;
    private readonly GetOfertaHandlers _getHandler;
    private readonly IUnitOfWork _unitOfWork;

    [ObservableProperty] private Guid id;
    [ObservableProperty] private Guid dizimistaId;
    [ObservableProperty] private decimal valor;
    [ObservableProperty] private DateTime data = DateTime.Today;
    [ObservableProperty] private bool isEditMode;

    [ObservableProperty] private ObservableCollection<Dizimista> dizimistas = new();

    public OfertaCadastroViewModel(CreateOfertaHandler createHandler, UpdateOfertaHandler updateHandler, GetOfertaHandlers getHandler, IUnitOfWork unitOfWork)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _getHandler = getHandler;
        _unitOfWork = unitOfWork;
        CarregarDizimistas();
    }

    private async void CarregarDizimistas()
    {
        var lista = await _unitOfWork.Dizimistas.GetAllAsync();
        Dizimistas = new ObservableCollection<Dizimista>(lista);
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var idObj) && Guid.TryParse(idObj?.ToString(), out var ofertaId))
        {
            var oferta = await _getHandler.Handle(new GetOfertaByIdQuery(ofertaId));
            if (oferta != null)
            {
                Id = oferta.Id;
                DizimistaId = oferta.DizimistaId;
                Valor = oferta.Valor;
                Data = oferta.Data;
                IsEditMode = true;
            }
        }
        else
        {
            Id = Guid.Empty;
            DizimistaId = Guid.Empty;
            Valor = 0;
            Data = DateTime.Today;
            IsEditMode = false;
        }
    }

    [RelayCommand]
    public async Task SalvarAsync()
    {
        if (IsEditMode)
        {
            await _updateHandler.Handle(new UpdateOfertaCommand(Id, DizimistaId, Valor, Data));
        }
        else
        {
            await _createHandler.Handle(new CreateOfertaCommand(DizimistaId, Valor, Data));
        }
        await Shell.Current.GoToAsync("..", true);
    }
}
