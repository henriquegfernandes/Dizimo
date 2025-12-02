using Dizimo.Application.Dizimistas.Handlers;
using Dizimo.Application.Relatorios;
using Dizimo.Domain.Repositories;
using Dizimo.ViewModels;

namespace Dizimo.Pages
{
    public partial class RelatorioPage : ContentPage
    {
        public RelatorioPage(
            GetDizimistaHandlers handlers,
            DeleteDizimistaHandler deleteHandler,
            InativarDizimistaHandler inativarHandler,
            DizimistaCsvService csvService,
            IUnitOfWork unitOfWork,
            RelatorioAniversariantesService relatorioAniversariantesService)
        {
            InitializeComponent();
            BindingContext = new DizimistaListViewModel(
                handlers,
                deleteHandler,
                inativarHandler,
                csvService,
                unitOfWork,
                relatorioAniversariantesService
            );
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (!IsUsuarioLogado())
            {
                await DisplayAlertAsync("Acesso negado", "Faça login para acessar os relatórios.", "OK");
                await Shell.Current.GoToAsync("login");
            }
        }

        private bool IsUsuarioLogado()
        {
            return Preferences.ContainsKey("UsuarioId");
        }
    }
}
