using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Models;
using Dizimo.Core.Services;

namespace Dizimo.PageModels;

public partial class DizimistaDetailPageModel : ObservableObject
{
    private readonly DizimoService _service;
    private readonly AuthService _auth;

    [ObservableProperty]
    private Dizimista dizimista = new();

    public DizimistaDetailPageModel(DizimoService service, AuthService auth)
    {
        _service = service;
        _auth = auth;
    }

    [ICommand]
    public async Task SaveAsync()
    {
        // Basic validation
        if (string.IsNullOrWhiteSpace(Dizimista.Nome))
        {
            await Shell.Current.DisplayAlert("Validação", "Nome é obrigatório.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(Dizimista.Codigo))
        {
            await Shell.Current.DisplayAlert("Validação", "Código é obrigatório.", "OK");
            return;
        }

        // Ensure codigo uniqueness (allow same codigo if editing the same ID)
        var all = await _service.ListDizimistasAsync();
        var existing = all.FirstOrDefault(x => x.Codigo == Dizimista.Codigo && x.ID != Dizimista.ID);
        if (existing is not null)
        {
            await Shell.Current.DisplayAlert("Validação", "Código já está em uso por outro dizimista.", "OK");
            return;
        }

        await _service.SaveDizimistaAsync(Dizimista);
    }

    [ICommand]
    public async Task InativarAsync()
    {
        if (!_auth.IsInRole("Admin"))
        {
            await Shell.Current.DisplayAlert("Permissão", "Apenas administradores podem inativar.", "OK");
            return;
        }

        Dizimista.Ativo = false;
        await _service.SaveDizimistaAsync(Dizimista);
    }
}

// Support query parameter for navigation
[QueryProperty(nameof(Id), "id")]
public partial class DizimistaDetailPageModel
{
    public int Id { get; set; }

    partial void OnIdChanged(int value)
    {
        _ = LoadAsync(value);
    }

    public async Task LoadAsync(int id)
    {
        if (id == 0) return;
        var d = await _service.ListDizimistasAsync();
        var item = d.FirstOrDefault(x => x.ID == id);
        if (item != null) Dizimista = item;
    }
}
