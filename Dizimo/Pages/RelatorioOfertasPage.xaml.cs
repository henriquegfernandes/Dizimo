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
            _ = vm.FiltrarAsync();
        }
    }
}
