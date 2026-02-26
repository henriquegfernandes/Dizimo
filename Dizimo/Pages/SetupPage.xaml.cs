using Dizimo.ViewModels;
using Dizimo.Domain.Repositories;

namespace Dizimo.Pages;

public partial class SetupPage : ContentPage
{
    public SetupPage()
    {
        InitializeComponent();
        var app = Microsoft.Maui.Controls.Application.Current as App;
        var unitOfWork = app?.Services.GetService<IUnitOfWork>();
        if (unitOfWork is not null)
        {
            BindingContext = new SetupViewModel(unitOfWork);
        }
    }

    private void OnNomeUsuarioCompleted(object sender, EventArgs e)
    {
        SenhaEntry.Focus();
    }

    private void OnSenhaCompleted(object sender, EventArgs e)
    {
        SenhaConfirmacaoEntry.Focus();
    }

    private void OnSenhaConfirmacaoCompleted(object sender, EventArgs e)
    {
        if (BindingContext is SetupViewModel vm && vm.CriarPrimeiroUsuarioCommand.CanExecute(null))
        {
            vm.CriarPrimeiroUsuarioCommand.Execute(null);
        }
    }
}
