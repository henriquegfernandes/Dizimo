using Dizimo.Domain.Repositories;
using Dizimo.Infrastructure.Repositories;
using Dizimo.Services;
using Dizimo.ViewModels;
using Microsoft.Maui.Controls;

namespace Dizimo.Pages
{
    public partial class MainPage : ContentPage
    {
        private readonly SessaoService _sessaoService;
        private readonly IUnitOfWork _unitOfWork;

        public MainPage(SessaoService sessaoService, MainPageViewModel viewModel, IUnitOfWork unitOfWork)
        {
            InitializeComponent();
            _sessaoService = sessaoService;
            _unitOfWork = unitOfWork;
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (!SessaoService.IsLogado)
            {
                await DisplayAlertAsync("Acesso negado", "Faça login para acessar o sistema.", "OK");
                await Shell.Current.GoToAsync("login");
            }

            var viewModel = (MainPageViewModel)BindingContext;
            await viewModel.CarregarDadosAsync();
        }
    }
}