using Dizimo.Domain.Repositories;
using Dizimo.Infrastructure.Repositories;
using Dizimo.Services;
using Dizimo.ViewModels;
using Microsoft.Maui.Controls;

namespace Dizimo.Pages
{
    public partial class MainPage : ContentPage
    {

        public MainPage(MainPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override async void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);
            
            if (!SessaoService.IsLogado)
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.GoToAsync("login");
                }
                return;
            }

            if (BindingContext is MainPageViewModel viewModel)
            {
                await viewModel.CarregarDadosAsync();
            }
        }
    }
}