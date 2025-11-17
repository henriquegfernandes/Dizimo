using Dizimo.PageModels;

namespace Dizimo.Pages;

public partial class RelatoriosPage : ContentPage
{
    public RelatoriosPage(RelatoriosPageModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
