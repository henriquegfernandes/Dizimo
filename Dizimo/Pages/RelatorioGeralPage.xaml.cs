using Dizimo.PageModels;

namespace Dizimo.Pages;

public partial class RelatorioGeralPage : ContentPage
{
    public RelatorioGeralPage(RelatorioGeralPageModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is RelatorioGeralPageModel vm)
        {
            _ = RunLoadAsync(vm);
        }
    }

    private async Task RunLoadAsync(RelatorioGeralPageModel vm)
    {
        try
        {
            await vm.LoadAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
        }
    }
}
