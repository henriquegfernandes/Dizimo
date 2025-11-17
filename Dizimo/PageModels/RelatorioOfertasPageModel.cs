using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Models;
using Dizimo.Core.Services;

namespace Dizimo.PageModels;

public partial class RelatorioOfertasPageModel : ObservableObject
{
    private readonly DizimoService _service;

    [ObservableProperty]
    private DateTime data = DateTime.Today;

    [ObservableProperty]
    private List<Oferta> items = new();

    [ObservableProperty]
    private decimal totalOfertas = 0;

    public RelatorioOfertasPageModel(DizimoService service)
    {
        _service = service;
    }

    [ICommand]
    public async Task FiltrarAsync()
    {
        Items = await _service.RelatorioOfertasPorDataAsync(Data);
        TotalOfertas = Items.Sum(x => x.Valor);
    }
}
