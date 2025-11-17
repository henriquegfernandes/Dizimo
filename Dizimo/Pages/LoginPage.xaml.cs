using Dizimo.PageModels;

namespace Dizimo.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginPageModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
