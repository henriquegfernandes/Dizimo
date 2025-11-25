using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Domain.Entities;
using Dizimo.Application.Dizimistas.Commands;
using Dizimo.Application.Dizimistas.Handlers;
using Dizimo.Application.Dizimistas.Queries;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Dizimo.ViewModels;

public partial class DizimistaCadastroViewModel : ObservableObject, IQueryAttributable
{
    private readonly CreateDizimistaHandler _createHandler;
    private readonly UpdateDizimistaHandler _updateHandler;
    private readonly GetDizimistaHandlers _getHandler;

    [ObservableProperty] private int numeroCadastro;
    [ObservableProperty] private string nome = string.Empty;
    [ObservableProperty] private DateTime dataNascimento = DateTime.Today;
    [ObservableProperty] private bool ativo = true;
    [ObservableProperty] private Guid id;
    [ObservableProperty] private bool isEditMode;

    public DizimistaCadastroViewModel(CreateDizimistaHandler createHandler, UpdateDizimistaHandler updateHandler, GetDizimistaHandlers getHandler)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _getHandler = getHandler;
    }

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
            IsEditMode = false;
        }
    }

    [RelayCommand]
    public async Task SalvarAsync()
    {
        if (IsEditMode)
        {
            await _updateHandler.Handle(new UpdateDizimistaCommand(Id, NumeroCadastro, Nome, DataNascimento, Ativo));
        }
        else
        {
            await _createHandler.Handle(new CreateDizimistaCommand(NumeroCadastro, Nome, DataNascimento));
        }
        await Shell.Current.GoToAsync("..", true);
    }
}
