using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Infrastructure.Services;
using Microsoft.Maui.Controls;

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
        // Simples: busca nome do usuário logado (pode ser otimizado)
        var repo = Application.Current.Services.GetService<Dizimo.Domain.Repositories.IUsuarioRepository>();
        var usuario = repo?.GetByIdAsync(_sessaoService.UsuarioId.Value).Result;
        return usuario?.Nome ?? "Usuário";
    }

    [RelayCommand]
    public async Task LogoutAsync()
    {
        _sessaoService.Logout();
        await Shell.Current.GoToAsync("//login");
    }

    public bool IsAdmin => _sessaoService.IsAdmin;
}
