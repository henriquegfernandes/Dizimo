using Dizimo.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace Dizimo.Infrastructure.Services;

public class SessaoService
{
    public Guid? UsuarioId => App.Current.Properties.TryGetValue("UsuarioId", out var id) && Guid.TryParse(id?.ToString(), out var guid) ? guid : null;
    public PerfilUsuario? Perfil => App.Current.Properties.TryGetValue("UsuarioPerfil", out var perfil) && Enum.TryParse<PerfilUsuario>(perfil?.ToString(), out var p) ? p : null;
    public bool IsLogado => UsuarioId != null;
    public bool IsAdmin => Perfil == PerfilUsuario.Admin;

    public void Login(Guid usuarioId, PerfilUsuario perfil)
    {
        App.Current.Properties["UsuarioId"] = usuarioId.ToString();
        App.Current.Properties["UsuarioPerfil"] = perfil.ToString();
    }

    public void Logout()
    {
        App.Current.Properties.Remove("UsuarioId");
        App.Current.Properties.Remove("UsuarioPerfil");
    }

    public static string HashSenha(string senha)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(senha));
        return Convert.ToBase64String(bytes);
    }
}
