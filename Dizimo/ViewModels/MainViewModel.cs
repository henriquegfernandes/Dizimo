using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Domain.Repositories;

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
        try
        {
            SessaoService.Logout();

            // Limpar o DbContext para garantir que não haja dados em cache
            if (Microsoft.Maui.Controls.Application.Current is App app)
            {
                var unitOfWork = app.Services.GetService<Dizimo.Domain.Repositories.IUnitOfWork>();
                if (unitOfWork != null)
                {
                    await unitOfWork.ClearDbContextAsync();
                }

                // Recrear o Shell para descartar todas as páginas em cache
                var newShell = new AppShell();
                app.Windows[0].Page = newShell;

                // Navegar para login
                await Shell.Current.GoToAsync("login");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao fazer logout: {ex.Message}");
            if (Microsoft.Maui.Controls.Application.Current is App app)
            {
                var newShell = new AppShell();
                app.Windows[0].Page = newShell;
                await Shell.Current.GoToAsync("login");
            }
        }
    }

    public static bool IsAdmin => SessaoService.IsAdmin;
}
