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
        try
        {
            SessaoService.Logout();

            if (Microsoft.Maui.Controls.Application.Current is App app)
            {
                var unitOfWork = app.Services.GetService<Dizimo.Domain.Repositories.IUnitOfWork>();
                if (unitOfWork != null)
                {
                    await unitOfWork.ClearDbContextAsync();
                }

                // Obter a janela de forma segura
                var window = app.Windows.Count > 0 ? app.Windows[0] : null;
                if (window != null)
                {
                    var newShell = new AppShell();
                    window.Page = newShell;

                    // Navegar usando o novo Shell para evitar inconsistências
                    if (newShell.CurrentState?.Location != null)
                    {
                        await newShell.GoToAsync("login");
                    }
                    else
                    {
                        await Shell.Current.GoToAsync("login");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao fazer logout: {ex.Message}");
            
            if (Microsoft.Maui.Controls.Application.Current is App app)
            {
                var window = app.Windows.Count > 0 ? app.Windows[0] : null;
                if (window != null)
                {
                    var newShell = new AppShell();
                    window.Page = newShell;
                    
                    if (newShell.CurrentState?.Location != null)
                    {
                        await newShell.GoToAsync("login");
                    }
                    else
                    {
                        await Shell.Current.GoToAsync("login");
                    }
                }
            }
        }
    }

    public static bool IsAdmin => SessaoService.IsAdmin;
}
