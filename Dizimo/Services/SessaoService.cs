using Dizimo.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace Dizimo.Services;

public class SessaoService
{
    public Guid? UsuarioId {
        get {
            var id = Preferences.Default.Get<string>("UsuarioId", string.Empty);
            return Guid.TryParse(id, out var guid) ? guid : null;
        }
    }
    public PerfilUsuario? Perfil {
        get {
            var perfil = Preferences.Default.Get<string>("UsuarioPerfil", string.Empty);
            return Enum.TryParse<PerfilUsuario>(perfil, out var p) ? p : null;
        }
    }
    public bool IsLogado => UsuarioId != null;
    public bool IsAdmin => Perfil == PerfilUsuario.Admin;

    public void Login(Guid usuarioId, PerfilUsuario perfil)
    {
        Preferences.Default.Set("UsuarioId", usuarioId.ToString());
        Preferences.Default.Set("UsuarioPerfil", perfil.ToString());
    }

    public void Logout()
    {
        Preferences.Default.Remove("UsuarioId");
        Preferences.Default.Remove("UsuarioPerfil");
    }

    public static string HashSenha(string senha)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(senha));
        return Convert.ToBase64String(bytes);
    }
}
