using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Dizimo.Services;

public class UserRecord
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
}

public class AuthService
{
    private readonly string _path;
    private List<UserRecord> _users = new();
    public UserRecord? CurrentUser { get; private set; }

    public AuthService()
    {
        _path = Path.Combine(FileSystem.AppDataDirectory, "users.json");
        Load();
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(_path)) return;
            var txt = File.ReadAllText(_path);
            _users = JsonSerializer.Deserialize<List<UserRecord>>(txt) ?? new List<UserRecord>();
        }
        catch { _users = new List<UserRecord>(); }
    }

    private void Save()
    {
        var txt = JsonSerializer.Serialize(_users);
        File.WriteAllText(_path, txt);
    }

    public bool CreateUser(string username, string password, string role = "User")
    {
        if (_users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase))) return false;
        var salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        var hash = HashPassword(password, salt);
        var u = new UserRecord { Username = username, PasswordHash = hash, Salt = salt, Role = role };
        _users.Add(u);
        Save();
        return true;
    }

    public bool Authenticate(string username, string password)
    {
        var u = _users.FirstOrDefault(x => x.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        if (u is null) return false;
        var h = HashPassword(password, u.Salt);
        if (h == u.PasswordHash)
        {
            CurrentUser = u;
            return true;
        }
        return false;
    }

    public bool IsInRole(string role)
    {
        return CurrentUser is not null && CurrentUser.Role.Equals(role, StringComparison.OrdinalIgnoreCase);
    }

    private static string HashPassword(string password, string salt)
    {
        var pbkdf2 = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), 10000, HashAlgorithmName.SHA256);
        return Convert.ToBase64String(pbkdf2.GetBytes(32));
    }
}
