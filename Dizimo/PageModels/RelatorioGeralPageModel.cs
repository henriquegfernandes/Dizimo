using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Models;
using Dizimo.Core.Services;

namespace Dizimo.PageModels;

public partial class RelatorioGeralPageModel : ObservableObject
{
    private readonly DizimoService _service;

    [ObservableProperty]
    private List<Models.ReportItem> items = new();

    [ObservableProperty]
    private decimal totalGeral = 0;

    public RelatorioGeralPageModel(DizimoService service)
    {
        _service = service;
    }

    [ICommand]
    public async Task LoadAsync()
    {
        var raw = await _service.RelatorioGeralAsync();
        var list = raw.Select(r => new Models.ReportItem { Nome = r.Dizimista.Nome, Count = r.Count, Total = r.Total }).ToList();
        Items = list;
        TotalGeral = list.Sum(x => x.Total);
    }
}
