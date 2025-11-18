using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Models;
using Dizimo.Core.Services;

namespace Dizimo.PageModels;

public partial class RelatorioAniversariantesPageModel : ObservableObject
{
    private readonly DizimoService _service;

    [ObservableProperty]
    private List<Dizimista> items = new();

    [ObservableProperty]
    private List<string> meses = new() { "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho", "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro" };

    [ObservableProperty]
    private int selectedMesIndex = 0;

    public RelatorioAniversariantesPageModel(DizimoService service)
    {
        _service = service;
    }

    [ICommand]
    public async Task FiltrarAsync()
    {
        var mes = SelectedMesIndex + 1;
        Items = await _service.RelatorioAniversariantesAsync(mes);
    }
}
