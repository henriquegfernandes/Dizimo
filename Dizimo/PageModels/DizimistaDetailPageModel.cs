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
