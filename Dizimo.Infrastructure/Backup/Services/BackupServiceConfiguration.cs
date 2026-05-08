namespace Dizimo.Infrastructure.Backup.Services;

/// <summary>
/// Configuração para operações de backup.
/// Segue o padrão de Configuração e mantém valores em um único lugar.
/// </summary>
public static class BackupServiceConfiguration
{
    // Nomes de arquivos
    public const string BackupFilePattern = "dizimo_backup_*.db";
    public const string BackupFilePrefix = "dizimo_backup_";
    public const string BackupFileExtension = ".db";
    public const string BackupFolderName = "Dizimo";
    public const string BackupSubfolderName = "Backups";
    public const string BackupFileBackupSuffix = ".bak";

    // Sufixos de arquivos auxiliares do SQLite
    public static readonly string[] SqliteAuxiliaryFileSuffixes = { "-wal", "-shm", "-journal", "-backup" };

    // Configurações de retry
    public const int MaxRetries = 5;
    public const int InitialRetryDelayMs = 300;
    public const int MaxRetryDelayMs = 2000;

    // Configurações de arquivo
    public const int FileStreamBufferSize = 4096;
    public const int CopyBufferSize = 81920; // 80KB
}

