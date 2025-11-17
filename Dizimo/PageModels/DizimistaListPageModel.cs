using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Models;
using Dizimo.Core.Services;

namespace Dizimo.PageModels;

public partial class DizimistaListPageModel : ObservableObject
{
    private readonly DizimoService _service;
    private readonly BackupService _backupService;

    [ObservableProperty]
    private List<Dizimista> dizimistas = new();

    public DizimistaListPageModel(DizimoService service, BackupService backupService)
    {
        _service = service;
        _backupService = backupService;
    }

    [ICommand]
    public async Task NewAsync()
    {
        await Shell.Current.GoToAsync("dizimista");
    }

    [ICommand]
    public async Task ExportAsync()
    {
        var path = Path.Combine(FileSystem.CacheDirectory, $"backup_{DateTime.Now:yyyyMMddHHmmss}.json");
        await _backupService.ExportJsonAsync(path);
        // Ideally show a toast/alert
        await Shell.Current.DisplayAlert("Backup", $"Backup salvo em: {path}", "OK");
    }

    [ICommand]
    public async Task ImportAsync()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Escolha o arquivo de backup" });
            if (result is null) return;
            var stream = await result.OpenReadAsync();
            var temp = Path.Combine(FileSystem.CacheDirectory, result.FileName);
            using var fs = File.OpenWrite(temp);
            await stream.CopyToAsync(fs);
            var imported = await _backupService.ImportJsonAsync(temp);
            await Shell.Current.DisplayAlert("Importar", $"{imported} novos dizimistas importados.", "OK");
            await LoadAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
        }
    }

    [ICommand]
    public async Task LoadAsync()
    {
        Dizimistas = await _service.ListDizimistasAsync();
    }

    [ICommand]
    public async Task LaunchByCodigo(string codigo, decimal valor)
    {
        var d = await _service.GetDizimistaByCodigoAsync(codigo);
        if (d is null) return;
        await _service.LançarOfertaAsync(d.ID, valor, DateTime.Now);
        await LoadAsync();
    }
}
