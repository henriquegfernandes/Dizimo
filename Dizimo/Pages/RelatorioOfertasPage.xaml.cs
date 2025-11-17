using Dizimo.PageModels;

namespace Dizimo.Pages;

public partial class RelatorioOfertasPage : ContentPage
{
    public RelatorioOfertasPage(RelatorioOfertasPageModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is RelatorioOfertasPageModel vm)
        {
            _ = RunFilterAsync(vm);
        }
    }

    private async Task RunFilterAsync(RelatorioOfertasPageModel vm)
    {
        try
        {
            await vm.FiltrarAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
        }
    }
}
