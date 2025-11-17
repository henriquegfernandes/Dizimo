using Dizimo.PageModels;

namespace Dizimo.Pages;

public partial class OfertaPage : ContentPage
{
    public OfertaPage(OfertaPageModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
