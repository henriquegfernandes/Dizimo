namespace Dizimo.Services;

/// <summary>
///     Serviço de preferências agnostic para MAUI
/// </summary>
public interface IPreferencesService
{
    T? Get<T>(string key, T? defaultValue = default);
    void Set<T>(string key, T value);
    void Remove(string key);
    void Clear();
}