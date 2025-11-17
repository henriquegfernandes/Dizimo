using Dizimo.Models;
using Dizimo.PageModels;

namespace Dizimo.Pages;

public partial class MainPage : ContentPage
{
    public MainPage(MainPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}