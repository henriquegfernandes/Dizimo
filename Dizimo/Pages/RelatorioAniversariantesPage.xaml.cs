using Dizimo.PageModels;

namespace Dizimo.Pages;

public partial class RelatorioAniversariantesPage : ContentPage
{
    public RelatorioAniversariantesPage(RelatorioAniversariantesPageModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is RelatorioAniversariantesPageModel vm)
        {
            vm.SelectedMesIndex = DateTime.Today.Month - 1;
            _ = vm.FiltrarAsync();
        }
    }
}
