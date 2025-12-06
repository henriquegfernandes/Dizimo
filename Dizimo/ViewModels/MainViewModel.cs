using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Dizimo.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly SessaoService _sessaoService;
    public MainViewModel(SessaoService sessaoService)
    {
        _sessaoService = sessaoService;
    }

    public string UsuarioNome => _sessaoService.UsuarioId != null ? GetUsuarioNome() : "Não logado";
    public string UsuarioPerfil => _sessaoService.Perfil?.ToString() ?? "";

    private string GetUsuarioNome()
    {
        var app = Microsoft.Maui.Controls.Application.Current as App;
        if (app?.Services == null)
            return "Usuário";

        var repo = app.Services.GetService<Dizimo.Domain.Repositories.IUsuarioRepository>();
        if (repo == null || _sessaoService.UsuarioId == null)
            return "Usuário";

        var usuario = repo.GetByIdAsync(_sessaoService.UsuarioId.Value).Result;
        return usuario?.Nome ?? "Usuário";
    }

    [RelayCommand]
    public async Task LogoutAsync()
    {
        _sessaoService.Logout();
        await Shell.Current.GoToAsync("login");
    }

    public bool IsAdmin => _sessaoService.IsAdmin;
}
