using Dizimo.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace Dizimo.Services;

public class SessaoService
{
    public static Guid? UsuarioId {
        get {
            var id = Preferences.Default.Get<string>("UsuarioId", string.Empty);
            return Guid.TryParse(id, out var guid) ? guid : null;
        }
    }
    public static PerfilUsuario? Perfil {
        get {
            var perfil = Preferences.Default.Get<string>("UsuarioPerfil", string.Empty);
            return Enum.TryParse<PerfilUsuario>(perfil, out var p) ? p : null;
        }
    }
    public static bool IsLogado => UsuarioId != null;
    public static bool IsAdmin => Perfil == PerfilUsuario.Admin;

    public static void Login(Guid usuarioId, PerfilUsuario perfil)
    {
        Preferences.Default.Set("UsuarioId", usuarioId.ToString());
        Preferences.Default.Set("UsuarioPerfil", perfil.ToString());
    }

    public static void Logout()
    {
        Preferences.Default.Remove("UsuarioId");
        Preferences.Default.Remove("UsuarioPerfil");
    }

    public static string HashSenha(string senha)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(senha));
        return Convert.ToBase64String(bytes);
    }
}
