using System.Text.Json;

namespace Dizimo.Services;

/// <summary>
/// Implementação de IPreferencesService usando JSON local
/// </summary>
public class LocalPreferencesService : IPreferencesService
{
    private readonly string _preferencesPath;
    private Dictionary<string, object> _preferences = [];

    public LocalPreferencesService(IDataPathProvider dataPathProvider)
    {
        var appDataDir = dataPathProvider.GetAppDataDirectory();
        _preferencesPath = Path.Combine(appDataDir, "preferences.json");
        LoadPreferences();
    }

    public T? Get<T>(string key, T? defaultValue = default)
    {
        if (_preferences.TryGetValue(key, out var value))
        {
            if (value is JsonElement je)
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(je.GetRawText());
                }
                catch
                {
                    return defaultValue;
                }
            }
            
            if (value is T typedValue)
                return typedValue;
        }

        return defaultValue;
    }

    public void Set<T>(string key, T value)
    {
        _preferences[key] = value!;
        SavePreferences();
    }

    public void Remove(string key)
    {
        _preferences.Remove(key);
        SavePreferences();
    }

    public void Clear()
    {
        _preferences.Clear();
        SavePreferences();
    }

    private void LoadPreferences()
    {
        try
        {
            if (File.Exists(_preferencesPath))
            {
                var json = File.ReadAllText(_preferencesPath);
                _preferences = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? [];
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AVISO] Erro ao carregar preferências: {ex.Message}");
            _preferences = [];
        }
    }

    private void SavePreferences()
    {
        try
        {
            var json = JsonSerializer.Serialize(_preferences);
            var directory = Path.GetDirectoryName(_preferencesPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory!);
            
            File.WriteAllText(_preferencesPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao salvar preferências: {ex.Message}");
        }
    }
}

