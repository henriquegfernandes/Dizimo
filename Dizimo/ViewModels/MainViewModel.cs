using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Dizimo.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public static string UsuarioNome => SessaoService.UsuarioId != null ? GetUsuarioNome() : "Não logado";
    public static string UsuarioPerfil => SessaoService.Perfil?.ToString() ?? "";

    private static string GetUsuarioNome()
    {
        var app = Microsoft.Maui.Controls.Application.Current as App;
        if (app?.Services == null)
            return "Usuário";

        var repo = app.Services.GetService<Dizimo.Domain.Repositories.IUsuarioRepository>();
        if (repo == null || SessaoService.UsuarioId == null)
            return "Usuário";

        var usuario = repo.GetByIdAsync(SessaoService.UsuarioId.Value).Result;
        return usuario?.Nome ?? "Usuário";
    }

    [RelayCommand]
    public static async Task LogoutAsync()
    {
        SessaoService.Logout();
        await Shell.Current.GoToAsync("login");
    }

    public static bool IsAdmin => SessaoService.IsAdmin;
}
