namespace Dizimo.Infrastructure.Backup.Services;

/// <summary>
/// Interface para operações de arquivo com suporte a retry.
/// Abstrai as operações de cópia e substituição de arquivo.
/// Segue Single Responsibility Principle.
/// </summary>
public interface IFileOperationService
{
    /// <summary>
    /// Copia um arquivo de forma assíncrona com retry automático para locks.
    /// </summary>
    /// <param name="sourcePath">Caminho do arquivo de origem.</param>
    /// <param name="destinationPath">Caminho do arquivo de destino.</param>
    /// <param name="shareMode">Modo de compartilhamento de arquivo.</param>
    Task CopyFileAsync(string sourcePath, string destinationPath, FileShare shareMode);

    /// <summary>
    /// Substitui um arquivo com outro de forma atômica, com retry automático.
    /// </summary>
    /// <param name="sourcePath">Caminho do arquivo de origem.</param>
    /// <param name="destinationPath">Caminho do arquivo de destino.</param>
    Task ReplaceFileAsync(string sourcePath, string destinationPath);

    /// <summary>
    /// Remove arquivos auxiliares do SQLite (WAL, SHM, etc).
    /// </summary>
    /// <param name="databaseFilePath">Caminho do arquivo de banco de dados.</param>
    void CleanupAuxiliaryFiles(string databaseFilePath);
}

