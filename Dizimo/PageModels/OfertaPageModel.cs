using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Core.Services;

namespace Dizimo.PageModels;

public partial class OfertaPageModel : ObservableObject
{
    private readonly DizimoService _service;

    [ObservableProperty] private string codigo = string.Empty;
    [ObservableProperty] private string valor = string.Empty;
    [ObservableProperty] private string observacao = string.Empty;

    public OfertaPageModel(DizimoService service)
    {
        _service = service;
    }

    [ICommand]
    public async Task LançarAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Codigo)) return;
            if (!decimal.TryParse(Valor, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var v)) return;
            var d = await _service.GetDizimistaByCodigoAsync(Codigo);
            if (d is null)
            {
                await Shell.Current.DisplayAlert("Erro", "Dizimista não encontrado.", "OK");
                return;
            }

            await _service.LançarOfertaAsync(d.ID, v, DateTime.Now, Observacao);
            // clear inputs
            Codigo = string.Empty;
            Valor = string.Empty;
            Observacao = string.Empty;

            await Shell.Current.DisplayAlert("Oferta", "Oferta lançada.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
        }
    }
}
