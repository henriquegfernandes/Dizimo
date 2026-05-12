using System.Security.Cryptography;
using System.Text;
using Dizimo.Domain.Entities;

namespace Dizimo.Services;

public class SessaoService
{
    private static IPreferencesService? _preferencesService;

    public static Guid? UsuarioId
    {
        get
        {
            if (_preferencesService == null)
                return null;

            var id = _preferencesService.Get<string>("UsuarioId", string.Empty);
            return Guid.TryParse(id, out var guid) ? guid : null;
        }
    }

    public static PerfilUsuario? Perfil
    {
        get
        {
            if (_preferencesService == null)
                return null;

            var perfil = _preferencesService.Get<string>("UsuarioPerfil", string.Empty);
            return Enum.TryParse<PerfilUsuario>(perfil, out var p) ? p : null;
        }
    }

    public static string UsuarioNome
    {
        get
        {
            if (_preferencesService == null)
                return string.Empty;

            return _preferencesService.Get<string>("UsuarioNome", string.Empty) ?? string.Empty;
        }
    }

    public static bool IsLogado => UsuarioId != null;
    public static bool IsAdmin => Perfil == PerfilUsuario.Admin;

    public static void Initialize(IPreferencesService preferencesService)
    {
        _preferencesService = preferencesService;
    }

    public static void Login(Guid usuarioId, PerfilUsuario perfil, string usuarioNome)
    {
        if (_preferencesService != null)
        {
            _preferencesService.Set("UsuarioId", usuarioId.ToString());
            _preferencesService.Set("UsuarioPerfil", perfil.ToString());
            _preferencesService.Set("UsuarioNome", usuarioNome);
        }
    }

    public static void Logout()
    {
        if (_preferencesService != null)
        {
            _preferencesService.Remove("UsuarioId");
            _preferencesService.Remove("UsuarioPerfil");
            _preferencesService.Remove("UsuarioNome");
        }
    }

    public static string HashSenha(string senha)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(senha));
        return Convert.ToBase64String(bytes);
    }
}