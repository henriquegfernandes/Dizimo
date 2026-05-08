namespace Dizimo.Infrastructure.Backup.Services;

/// <summary>
/// Interface para gerenciar preferências de backup.
/// Abstrai a implementação específica, permitindo diferentes estratégias de persistência.
/// Segue o princípio de Dependency Inversion.
/// </summary>
public interface IBackupPreferencesProvider
{
    /// <summary>
    /// Obtém o caminho da pasta de backup configurada.
    /// </summary>
    /// <returns>Caminho da pasta ou null se não configurado.</returns>
    string? GetBackupFolderPath();

    /// <summary>
    /// Define o caminho da pasta de backup.
    /// </summary>
    void SetBackupFolderPath(string folderPath);
}

