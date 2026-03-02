using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Dizimo.Infrastructure.Persistence;

namespace Dizimo.Services;

public class LocalBackupService(string dbFilePath, IServiceProvider serviceProvider)
{
    private readonly string _dbFilePath = dbFilePath;
    private string _backupFolderPath = Preferences.Default.Get("BackupFolderPath", FileSystem.Current.AppDataDirectory);
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private static readonly SemaphoreSlim _logoutLock = new(1, 1);

    public string BackupFolderPath
    {
        get => _backupFolderPath;
        set
        {
            _backupFolderPath = value;
            Preferences.Default.Set("BackupFolderPath", value);
        }
    }

    public bool IsBackupFolderConfigured => !string.IsNullOrEmpty(_backupFolderPath) && _backupFolderPath != FileSystem.Current.AppDataDirectory;

    public void SetBackupFolder(string folderPath)
    {
        BackupFolderPath = folderPath;
    }

    /// <summary>
    /// Executa logout com limpeza segura da UI e contexto do banco de dados.
    /// </summary>
    private static async Task PerformLogoutWithUICleanupAsync()
    {
        await _logoutLock.WaitAsync();
        try
        {
            SessaoService.Logout();

            if (Microsoft.Maui.Controls.Application.Current is not App app)
                return;

            await ClearDbContextAsync(app);
            SqliteConnection.ClearAllPools();
            await Task.Delay(100);

            if (await TryNavigateToLoginAsync(app))
                return;

            // Fallback: tentar com Shell.Current existente
            await Shell.Current.GoToAsync("login");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao fazer logout com limpeza de UI: {ex.Message}");
            await FallbackNavigateToLoginAsync();
        }
        finally
        {
            _logoutLock.Release();
        }
    }

    /// <summary>
    /// Limpa o DbContext através do UnitOfWork.
    /// </summary>
    private static async Task ClearDbContextAsync(App app)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetService<Dizimo.Domain.Repositories.IUnitOfWork>();
            if (unitOfWork != null)
            {
                await unitOfWork.ClearDbContextAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AVISO] Erro ao limpar DbContext: {ex.Message}");
        }
    }

    /// <summary>
    /// Tenta navegar para login criando novo Shell.
    /// </summary>
    private static async Task<bool> TryNavigateToLoginAsync(App app)
    {
        try
        {
            var window = app.Windows.Count > 0 ? app.Windows[0] : null;
            if (window == null)
                return false;

            var newShell = new AppShell();
            window.Page = newShell;

            var navigateLocation = newShell.CurrentState?.Location;
            if (navigateLocation != null)
            {
                await newShell.GoToAsync("login");
            }
            else
            {
                await Shell.Current.GoToAsync("login");
            }

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AVISO] Erro ao criar novo Shell: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Fallback final para navegação ao login.
    /// </summary>
    private static async Task FallbackNavigateToLoginAsync()
    {
        try
        {
            if (Microsoft.Maui.Controls.Application.Current is App app)
            {
                var window = app.Windows.Count > 0 ? app.Windows[0] : null;
                if (window != null)
                {
                    try
                    {
                        var newShell = new AppShell();
                        window.Page = newShell;
                    }
                    catch { /* Ignorar erro de criação */ }
                }
            }

            await Shell.Current.GoToAsync("login");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Falha no fallback de navegação: {ex.Message}");
        }
    }

    public async Task BackupAsync()
    {
        var destPath = Path.Combine(_backupFolderPath, Path.GetFileName(_dbFilePath));

        try
        {
            await ExecuteCheckpointAsync(_dbFilePath, "PRAGMA wal_checkpoint(RESTART)");
            await Task.Delay(100);

            await CopyFileAsync(_dbFilePath, destPath, FileShare.Read);
        }
        catch (IOException)
        {
            await Task.Delay(500);
            await CopyFileAsync(_dbFilePath, destPath, FileShare.ReadWrite);
        }
    }

    public async Task RestoreAsync()
    {
        var srcPath = Path.Combine(_backupFolderPath, Path.GetFileName(_dbFilePath));

        if (!File.Exists(srcPath))
            throw new FileNotFoundException($"Arquivo de backup não encontrado em: {srcPath}");

        try
        {
            SqliteConnection.ClearAllPools();
            await Task.Delay(300);

            await ExecuteCheckpointAsync(_dbFilePath, "PRAGMA wal_checkpoint(TRUNCATE)");
            await Task.Delay(200);

            await CopyFileAsync(srcPath, _dbFilePath, FileShare.Read);
            CleanupSqliteAuxiliaryFiles();

            await PerformLogoutWithUICleanupAsync();
            RestoreDbContext();
        }
        catch (IOException)
        {
            await Task.Delay(1000);
            SqliteConnection.ClearAllPools();
            await Task.Delay(500);

            await CopyFileAsync(srcPath, _dbFilePath, FileShare.ReadWrite);
            CleanupSqliteAuxiliaryFiles();

            await PerformLogoutWithUICleanupAsync();
            RestoreDbContext();
        }
    }

    /// <summary>
    /// Executa checkpoint do SQLite de forma segura.
    /// </summary>
    private static async Task ExecuteCheckpointAsync(string dbPath, string pragmaCommand)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            await connection.OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = pragmaCommand;
            await command.ExecuteScalarAsync();
        }
        catch
        {
            // Ignorar erros durante checkpoint
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
    /// Restaura o DbContext e executa migrações.
    /// </summary>
    private void RestoreDbContext()
    {
        if (_serviceProvider == null)
            return;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var newContext = scope.ServiceProvider.GetRequiredService<DizimoDbContext>();
            newContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AVISO] Erro ao restaurar DbContext: {ex.Message}");
        }
    }
}
