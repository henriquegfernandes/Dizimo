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
            _ = RunFilterAsync(vm);
        }
    }

    private async Task RunFilterAsync(RelatorioAniversariantesPageModel vm)
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
