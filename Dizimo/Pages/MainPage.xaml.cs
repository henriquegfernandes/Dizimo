using Dizimo.Models;
using Dizimo.PageModels;
using Microsoft.Maui.Controls;

namespace Dizimo.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }

        private void OnMenuButtonClicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = true;
        }
    }
}