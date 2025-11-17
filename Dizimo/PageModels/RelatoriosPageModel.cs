using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Core.Services;

namespace Dizimo.PageModels;

public partial class RelatoriosPageModel : ObservableObject
{
    private readonly DizimoService _service;

    public RelatoriosPageModel(DizimoService service)
    {
        _service = service;
    }

    [ICommand]
    public async Task OpenGeral()
    {
        await Shell.Current.GoToAsync("relatorio/geral");
    }

    [ICommand]
    public async Task OpenAniversariantes()
    {
        await Shell.Current.GoToAsync("relatorio/aniversariantes");
    }

    [ICommand]
    public async Task OpenOfertas()
    {
        await Shell.Current.GoToAsync("relatorio/ofertas");
    }
}
