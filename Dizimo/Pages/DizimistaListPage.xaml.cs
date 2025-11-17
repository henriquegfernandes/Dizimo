using Dizimo.PageModels;

namespace Dizimo.Pages;

public partial class DizimistaListPage : ContentPage
{
    public DizimistaListPage(DizimistaListPageModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DizimistaListPageModel vm)
        {
            _ = RunLoadAsync(vm);
        }
    }

    private async Task RunLoadAsync(DizimistaListPageModel vm)
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

    private async void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Dizimista item)
        {
            // navigate to detail page with id
            await Shell.Current.GoToAsync($"dizimista?id={item.ID}");
        }
    }
}
