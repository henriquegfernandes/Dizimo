using Microsoft.Extensions.DependencyInjection;
using Dizimo.Domain.Repositories;

namespace Dizimo.Infrastructure.Backup.Services;

public class LocalBackupService(string dbFilePath, IServiceProvider serviceProvider, object? preferencesService = null)
{
    private readonly string _dbFilePath = dbFilePath;
    private string _backupFolderPath = GetBackupPath(preferencesService) ?? GetDefaultBackupPath();
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly object? _preferencesService = preferencesService;

    private static string? GetBackupPath(object? preferencesService)
    {
        if (preferencesService == null) return null;
        
        try
        {
            var getMethod = preferencesService.GetType().GetMethod("Get");
            if (getMethod != null)
            {
                return getMethod.Invoke(preferencesService, new object[] { "BackupFolderPath", GetDefaultBackupPath() }) as string;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AVISO] Erro ao obter caminho de backup do PreferencesService: {ex.Message}");
        }
        
        return null;
    }

    private static string GetDefaultBackupPath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Dizimo",
            "Backups");
    }

    public string BackupFolderPath
    {
        get => _backupFolderPath;
        set
        {
            _backupFolderPath = value;
            if (_preferencesService != null)
            {
                try
                {
                    var setMethod = _preferencesService.GetType().GetMethod("Set");
                    setMethod?.Invoke(_preferencesService, new object[] { "BackupFolderPath", value });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[AVISO] Erro ao salvar caminho de backup no PreferencesService: {ex.Message}");
                }
            }
        }
    }

    public bool IsBackupFolderConfigured => !string.IsNullOrEmpty(_backupFolderPath) && _backupFolderPath != GetDefaultBackupPath();

    public void SetBackupFolder(string folderPath)
    {
        BackupFolderPath = folderPath;
    }

    /// <summary>
    /// Limpa o DbContext através do UnitOfWork de forma completa.
    /// </summary>
    private async Task ClearDbContextAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetService(typeof(IUnitOfWork)) as IUnitOfWork;
            if (unitOfWork != null)
            {
                await unitOfWork.ClearDbContextAsync();
                System.Diagnostics.Debug.WriteLine("[INFO] DbContext desanexado");
            }

            // Também tenta acessar e descartar o DbContext diretamente
            var dbContext = scope.ServiceProvider.GetService(typeof(Persistence.DizimoDbContext)) as Persistence.DizimoDbContext;
            if (dbContext != null)
            {
                System.Diagnostics.Debug.WriteLine("[INFO] Descartando DbContext");
                foreach (var entry in dbContext.ChangeTracker.Entries())
                {
                    entry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                }
                await dbContext.DisposeAsync();
                System.Diagnostics.Debug.WriteLine("[INFO] DbContext descartado");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AVISO] Erro ao limpar DbContext: {ex.Message}");
        }
    }

    public async Task BackupAsync()
    {
        try
        {
            if (!Directory.Exists(BackupFolderPath))
            {
                Directory.CreateDirectory(BackupFolderPath);
            }

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"dizimo_backup_{timestamp}.db";
            var backupFilePath = Path.Combine(BackupFolderPath, backupFileName);

            await ClearDbContextAsync();
            await CopyFileAsync(_dbFilePath, backupFilePath, FileShare.Read);

            System.Diagnostics.Debug.WriteLine($"[INFO] Backup realizado com sucesso: {backupFilePath}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao fazer backup: {ex.Message}");
            throw;
        }
    }

    public async Task RestoreAsync()
    {
        try
        {
            var backupFiles = Directory.GetFiles(BackupFolderPath, "dizimo_backup_*.db")
                .OrderByDescending(f => new FileInfo(f).CreationTime)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(backupFiles))
            {
                System.Diagnostics.Debug.WriteLine("[AVISO] Nenhum arquivo de backup encontrado");
                return;
            }

            await ClearDbContextAsync();
            await CopyFileAsync(backupFiles, _dbFilePath, FileShare.Read);
            
            CleanupSqliteAuxiliaryFiles();
            System.Diagnostics.Debug.WriteLine($"[INFO] Restauração realizada com sucesso de: {backupFiles}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao restaurar backup: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Restaura um arquivo de backup específico
    /// </summary>
    public async Task RestoreFromFileAsync(string backupFilePath)
    {
        try
        {
            if (!File.Exists(backupFilePath))
            {
                throw new FileNotFoundException($"Arquivo de backup não encontrado: {backupFilePath}");
            }

            System.Diagnostics.Debug.WriteLine($"[INFO] Iniciando restauração de: {backupFilePath}");

            // 1. Limpar DbContext para desanexar entidades
            await ClearDbContextAsync();
            System.Diagnostics.Debug.WriteLine("[INFO] DbContext limpo");

            // 2. Limpar WAL/SHM ANTES de copiar (importante para liberar locks)
            CleanupSqliteAuxiliaryFiles();
            System.Diagnostics.Debug.WriteLine("[INFO] Arquivos auxiliares SQLite limpos");

            // 3. Forçar garbage collection para liberar handles de arquivo
            System.Diagnostics.Debug.WriteLine("[INFO] Iniciando coleta de lixo...");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            System.Diagnostics.Debug.WriteLine("[INFO] Coleta de lixo concluída");

            // 4. Aguardar para garantir que os arquivos foram liberados
            System.Diagnostics.Debug.WriteLine("[INFO] Aguardando 1 segundo para liberar recursos...");
            await Task.Delay(1000);

            // 5. Copiar o arquivo de backup
            System.Diagnostics.Debug.WriteLine($"[INFO] Copiando arquivo de: {backupFilePath} para: {_dbFilePath}");
            await CopyFileAsync(backupFilePath, _dbFilePath, FileShare.Read);
            System.Diagnostics.Debug.WriteLine("[INFO] Arquivo copiado com sucesso");

            // 6. Verificar se o arquivo foi realmente copiado
            if (!File.Exists(_dbFilePath))
            {
                throw new InvalidOperationException($"Falha ao verificar o arquivo restaurado: {_dbFilePath}");
            }

            var fileInfo = new FileInfo(_dbFilePath);
            System.Diagnostics.Debug.WriteLine($"[INFO] Arquivo restaurado: {fileInfo.FullName} ({fileInfo.Length} bytes)");

            // 7. Limpar WAL/SHM DEPOIS de copiar
            CleanupSqliteAuxiliaryFiles();
            System.Diagnostics.Debug.WriteLine("[INFO] Arquivos auxiliares SQLite limpos novamente");

            // 8. Novo garbage collection para garantir limpeza
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            System.Diagnostics.Debug.WriteLine("[INFO] Segunda coleta de lixo concluída");

            // 9. Aguardar mais um pouco
            await Task.Delay(500);

            System.Diagnostics.Debug.WriteLine("[INFO] Restauração realizada com sucesso!");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao restaurar backup: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ERRO] Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// Remove arquivos auxiliares do SQLite (WAL e SHM).
    /// </summary>
    private void CleanupSqliteAuxiliaryFiles()
    {
        var auxiliaryFiles = new[] { "-wal", "-shm", "-journal", "-backup" };
        
        foreach (var suffix in auxiliaryFiles)
        {
            var filePath = _dbFilePath + suffix;
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[AVISO] Erro ao deletar {filePath}: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Copia arquivo de forma assíncrona.
    /// </summary>
    private static async Task CopyFileAsync(string sourcePath, string destinationPath, FileShare shareMode)
    {
        using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, shareMode);
        using var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await sourceStream.CopyToAsync(destinationStream);
    }
}



