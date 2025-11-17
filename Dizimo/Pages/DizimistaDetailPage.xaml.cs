using Dizimo.PageModels;

namespace Dizimo.Pages;

public partial class DizimistaDetailPage : ContentPage
{
    public DizimistaDetailPage(DizimistaDetailPageModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
