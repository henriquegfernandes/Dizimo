using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Dizimo.Domain.Repositories;
using Microsoft.Data.Sqlite;

namespace Dizimo.Infrastructure.Backup.Services;

/// <summary>
/// Serviço de backup local que gerencia operações de backup e restauração do banco de dados.
/// Implementa operações seguras com tratamento de locks do Windows e retry automático.
/// Segue Clean Architecture com injeção de dependências e separação de responsabilidades.
/// </summary>
public class LocalBackupService
{
    private readonly string _dbFilePath;
    private readonly IServiceProvider _serviceProvider;
    private readonly IBackupPreferencesProvider _preferencesProvider;
    private readonly IFileOperationService _fileOperationService;
    private readonly ILogger<LocalBackupService> _logger;
    private string _backupFolderPath;

    public LocalBackupService(
        string dbFilePath,
        IServiceProvider serviceProvider,
        IBackupPreferencesProvider preferencesProvider,
        IFileOperationService fileOperationService,
        ILogger<LocalBackupService> logger)
    {
        _dbFilePath = dbFilePath ?? throw new ArgumentNullException(nameof(dbFilePath));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _preferencesProvider = preferencesProvider ?? throw new ArgumentNullException(nameof(preferencesProvider));
        _fileOperationService = fileOperationService ?? throw new ArgumentNullException(nameof(fileOperationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _backupFolderPath = _preferencesProvider.GetBackupFolderPath() ?? GetDefaultBackupPath();
    }

    /// <summary>
    /// Obtém o caminho padrão para backups.
    /// </summary>
    private static string GetDefaultBackupPath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            BackupServiceConfiguration.BackupFolderName,
            BackupServiceConfiguration.BackupSubfolderName);
    }

    /// <summary>
    /// Obtém ou define o caminho da pasta de backup.
    /// </summary>
    public string BackupFolderPath
    {
        get => _backupFolderPath;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value));

            _backupFolderPath = value;
            _preferencesProvider.SetBackupFolderPath(value);
        }
    }

    /// <summary>
    /// Verifica se um caminho de backup diferente do padrão foi configurado.
    /// </summary>
    public bool IsBackupFolderConfigured =>
        !string.IsNullOrEmpty(_backupFolderPath) && 
        !_backupFolderPath.Equals(GetDefaultBackupPath(), StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Define o caminho da pasta de backup.
    /// </summary>
    public void SetBackupFolder(string folderPath)
    {
        BackupFolderPath = folderPath;
    }

    /// <summary>
    /// Realiza um backup do banco de dados para a pasta configurada.
    /// </summary>
    public async Task BackupAsync()
    {
        try
        {
            EnsureBackupFolderExists();

            string backupFilePath = GenerateBackupFilePath();
            
            _logger.LogInformation("Iniciando backup para: {FilePath}", backupFilePath);

            await PrepareForFileOperationAsync();
            await _fileOperationService.CopyFileAsync(_dbFilePath, backupFilePath, FileShare.ReadWrite);

            _logger.LogInformation("Backup realizado com sucesso: {FilePath}", backupFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer backup");
            throw;
        }
    }

    /// <summary>
    /// Restaura o banco de dados a partir do backup mais recente.
    /// </summary>
    public async Task RestoreAsync()
    {
        try
        {
            string? latestBackupFile = FindLatestBackupFile();

            if (string.IsNullOrEmpty(latestBackupFile))
            {
                _logger.LogWarning("Nenhum arquivo de backup encontrado");
                return;
            }

            _logger.LogInformation("Restaurando backup de: {FilePath}", latestBackupFile);

            await PrepareForFileOperationAsync();
            await _fileOperationService.ReplaceFileAsync(latestBackupFile, _dbFilePath);
            _fileOperationService.CleanupAuxiliaryFiles(_dbFilePath);

            _logger.LogInformation("Restauração realizada com sucesso de: {FilePath}", latestBackupFile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao restaurar backup");
            throw;
        }
    }

    /// <summary>
    /// Restaura o banco de dados a partir de um arquivo de backup específico.
    /// </summary>
    public async Task RestoreFromFileAsync(string backupFilePath)
    {
        try
        {
            if (!File.Exists(backupFilePath))
                throw new FileNotFoundException($"Arquivo de backup não encontrado: {backupFilePath}");

            _logger.LogInformation("Iniciando restauração de arquivo específico: {FilePath}", backupFilePath);

            await PrepareForFileOperationAsync();
            await _fileOperationService.ReplaceFileAsync(backupFilePath, _dbFilePath);
            _fileOperationService.CleanupAuxiliaryFiles(_dbFilePath);

            if (!File.Exists(_dbFilePath))
                throw new InvalidOperationException($"Falha ao verificar o arquivo restaurado: {_dbFilePath}");

            var fileInfo = new FileInfo(_dbFilePath);
            _logger.LogInformation("Arquivo restaurado com sucesso: {FilePath} ({FileSize} bytes)",
                fileInfo.FullName, fileInfo.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao restaurar arquivo de backup específico");
            throw;
        }
    }

    /// <summary>
    /// Prepara o sistemas para operações de arquivo liberando conexões de banco.
    /// </summary>
    private async Task PrepareForFileOperationAsync()
    {
        await ClearDbContextAsync();
        _fileOperationService.CleanupAuxiliaryFiles(_dbFilePath);
        ForceGarbageCollection();
    }

    /// <summary>
    /// Limpa o DbContext e desabilita todas as conexões SQLite ativas.
    /// </summary>
    private async Task ClearDbContextAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            
            // Limpar através do UnitOfWork
            var unitOfWork = scope.ServiceProvider.GetService(typeof(IUnitOfWork)) as IUnitOfWork;
            if (unitOfWork != null)
            {
                await unitOfWork.ClearDbContextAsync();
                _logger.LogDebug("DbContext desanexado via UnitOfWork");
            }

            // Limpar DbContext diretamente como backup
            var dbContext = scope.ServiceProvider.GetService(typeof(Persistence.DizimoDbContext)) as Persistence.DizimoDbContext;
            if (dbContext != null)
            {
                DetachAllEntries(dbContext);
                await dbContext.DisposeAsync();
                _logger.LogDebug("DbContext descartado");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao limpar DbContext");
        }

        // Limpar pool de conexões
        SqliteConnection.ClearAllPools();
        _logger.LogDebug("Pool de conexões SQLite limpo");
    }

    /// <summary>
    /// Desanexada todas as entidades rastreadas no DbContext.
    /// </summary>
    private static void DetachAllEntries(Persistence.DizimoDbContext dbContext)
    {
        foreach (var entry in dbContext.ChangeTracker.Entries())
        {
            entry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        }
    }

    /// <summary>
    /// Força coleta de lixo para liberar handles de arquivo.
    /// </summary>
    private static void ForceGarbageCollection()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    /// <summary>
    /// Garante que a pasta de backup existe.
    /// </summary>
    private void EnsureBackupFolderExists()
    {
        if (!Directory.Exists(BackupFolderPath))
        {
            Directory.CreateDirectory(BackupFolderPath);
            _logger.LogInformation("Pasta de backup criada: {FolderPath}", BackupFolderPath);
        }
    }

    /// <summary>
    /// Encontra o arquivo de backup mais recente.
    /// </summary>
    private string? FindLatestBackupFile()
    {
        return Directory.GetFiles(BackupFolderPath, BackupServiceConfiguration.BackupFilePattern)
            .OrderByDescending(f => new FileInfo(f).CreationTime)
            .FirstOrDefault();
    }

    /// <summary>
    /// Gera o caminho completo para um novo arquivo de backup.
    /// </summary>
    private string GenerateBackupFilePath()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string backupFileName = $"{BackupServiceConfiguration.BackupFilePrefix}{timestamp}{BackupServiceConfiguration.BackupFileExtension}";
        return Path.Combine(BackupFolderPath, backupFileName);
    }
}
