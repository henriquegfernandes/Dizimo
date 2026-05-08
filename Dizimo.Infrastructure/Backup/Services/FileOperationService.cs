using Microsoft.Extensions.Logging;

namespace Dizimo.Infrastructure.Backup.Services;

/// <summary>
/// Implementação de operações de arquivo com retry automático.
/// Encapsula toda a lógica de cópia e substituição de arquivo com tratamento de locks do Windows.
/// </summary>
public class FileOperationService : IFileOperationService
{
    private readonly ILogger<FileOperationService> _logger;

    public FileOperationService(ILogger<FileOperationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Copia um arquivo com retry automático para lidar com locks do Windows.
    /// </summary>
    public async Task CopyFileAsync(string sourcePath, string destinationPath, FileShare shareMode)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
            throw new ArgumentNullException(nameof(sourcePath));
        if (string.IsNullOrWhiteSpace(destinationPath))
            throw new ArgumentNullException(nameof(destinationPath));

        int retryDelay = BackupServiceConfiguration.InitialRetryDelayMs;

        for (int attempt = 0; attempt < BackupServiceConfiguration.MaxRetries; attempt++)
        {
            try
            {
                using var sourceStream = new FileStream(
                    sourcePath,
                    FileMode.Open,
                    FileAccess.Read,
                    shareMode,
                    BackupServiceConfiguration.FileStreamBufferSize,
                    FileOptions.SequentialScan);

                using var destinationStream = new FileStream(
                    destinationPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    BackupServiceConfiguration.FileStreamBufferSize,
                    FileOptions.SequentialScan);

                await sourceStream.CopyToAsync(destinationStream, BackupServiceConfiguration.CopyBufferSize);
                
                _logger.LogInformation("Arquivo copiado com sucesso: {FilePath} -> {Destination} (tentativa {Attempt})",
                    sourcePath, destinationPath, attempt + 1);
                return;
            }
            catch (IOException ex) when (attempt < BackupServiceConfiguration.MaxRetries - 1)
            {
                _logger.LogWarning("Tentativa {Attempt}/{MaxRetries} de cópia falhou: {Message}. Aguardando {DelayMs}ms...",
                    attempt + 1, BackupServiceConfiguration.MaxRetries, ex.Message, retryDelay);

                await Task.Delay(retryDelay);
                retryDelay = Math.Min(retryDelay * 2, BackupServiceConfiguration.MaxRetryDelayMs);
            }
            catch (IOException ex)
            {
                _logger.LogError("Falha final ao copiar arquivo: {Message}", ex.Message);
                throw;
            }
        }
    }

    /// <summary>
    /// Substitui um arquivo com outro de forma atômica, com retry automático.
    /// </summary>
    public async Task ReplaceFileAsync(string sourcePath, string destinationPath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
            throw new ArgumentNullException(nameof(sourcePath));
        if (string.IsNullOrWhiteSpace(destinationPath))
            throw new ArgumentNullException(nameof(destinationPath));

        string backupPath = destinationPath + BackupServiceConfiguration.BackupFileBackupSuffix;
        int retryDelay = BackupServiceConfiguration.InitialRetryDelayMs;

        for (int attempt = 0; attempt < BackupServiceConfiguration.MaxRetries; attempt++)
        {
            try
            {
                CleanupBackupFile(backupPath);
                
                File.Replace(sourcePath, destinationPath, backupPath, ignoreMetadataErrors: true);
                
                _logger.LogInformation("Arquivo substituído com sucesso: {FilePath} (tentativa {Attempt})",
                    destinationPath, attempt + 1);
                
                CleanupBackupFile(backupPath);
                return;
            }
            catch (IOException ex) when (attempt < BackupServiceConfiguration.MaxRetries - 1)
            {
                _logger.LogWarning("Tentativa {Attempt}/{MaxRetries} de substituição falhou: {Message}. Aguardando {DelayMs}ms...",
                    attempt + 1, BackupServiceConfiguration.MaxRetries, ex.Message, retryDelay);

                await Task.Delay(retryDelay);
                retryDelay = Math.Min(retryDelay * 2, BackupServiceConfiguration.MaxRetryDelayMs);
            }
            catch (IOException ex)
            {
                _logger.LogError("Falha final ao substituir arquivo: {Message}", ex.Message);
                throw;
            }
        }
    }

    /// <summary>
    /// Remove arquivos auxiliares do SQLite.
    /// </summary>
    public void CleanupAuxiliaryFiles(string databaseFilePath)
    {
        if (string.IsNullOrWhiteSpace(databaseFilePath))
            throw new ArgumentNullException(nameof(databaseFilePath));

        foreach (var suffix in BackupServiceConfiguration.SqliteAuxiliaryFileSuffixes)
        {
            string auxiliaryFilePath = databaseFilePath + suffix;
            if (File.Exists(auxiliaryFilePath))
            {
                try
                {
                    File.Delete(auxiliaryFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Erro ao deletar arquivo auxiliar {FilePath}: {Message}",
                        auxiliaryFilePath, ex.Message);
                }
            }
        }
    }

    private static void CleanupBackupFile(string backupPath)
    {
        try
        {
            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }
        }
        catch
        {
            // Ignorar erros ao limpar arquivo de backup temporário
        }
    }
}

