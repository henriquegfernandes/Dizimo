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
    private async Task PerformLogoutWithUICleanupAsync()
    {
        // Usar SemaphoreSlim para evitar múltiplas execuções concorrentes
        await _logoutLock.WaitAsync();
        try
        {
            // Fazer logout da sessão
            SessaoService.Logout();

            // Obter referência segura da aplicação
            if (Microsoft.Maui.Controls.Application.Current is not App app)
                return;

            // Limpar DbContext através do UnitOfWork se disponível
            var unitOfWork = app.Services.GetService<Dizimo.Domain.Repositories.IUnitOfWork>();
            if (unitOfWork != null)
            {
                await unitOfWork.ClearDbContextAsync();
            }

            // Limpar conexões do pool SQLite
            SqliteConnection.ClearAllPools();
            await Task.Delay(100);

            // Renovar a janela principal com novo Shell
            var window = app.Windows.Count > 0 ? app.Windows[0] : null;
            if (window != null)
            {
                var newShell = new AppShell();
                window.Page = newShell;

                // Navegar para login de forma segura
                if (newShell.CurrentState?.Location != null)
                {
                    await newShell.GoToAsync("login");
                }
                else
                {
                    await Shell.Current.GoToAsync("login");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao fazer logout com limpeza de UI: {ex.Message}");
            
            // Fallback: ao menos tente renovar o Shell
            if (Microsoft.Maui.Controls.Application.Current is App app)
            {
                var window = app.Windows.Count > 0 ? app.Windows[0] : null;
                if (window != null)
                {
                    var newShell = new AppShell();
                    window.Page = newShell;
                    
                    try
                    {
                        if (newShell.CurrentState?.Location != null)
                        {
                            await newShell.GoToAsync("login");
                        }
                        else
                        {
                            await Shell.Current.GoToAsync("login");
                        }
                    }
                    catch { /* Ignorar erros de navegação */ }
                }
            }
        }
        finally
        {
            _logoutLock.Release();
        }
    }

    public async Task BackupAsync()
    {
        var destPath = Path.Combine(_backupFolderPath, Path.GetFileName(_dbFilePath));

        try
        {
            // Fazer checkpoint para consolidar WAL log no banco principal
            using (var connection = new SqliteConnection($"Data Source={_dbFilePath}"))
            {
                await connection.OpenAsync();
                using var command = connection.CreateCommand();
                command.CommandText = "PRAGMA wal_checkpoint(RESTART)";
                await command.ExecuteScalarAsync();
            }

            // Aguardar um pouco para garantir que tudo foi sincronizado
            await Task.Delay(100);

            // Copiar apenas o arquivo de banco (dados já consolidados pelo checkpoint)
            using var sourceStream = new FileStream(_dbFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var destinationStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await sourceStream.CopyToAsync(destinationStream);
        }
        catch (IOException)
        {
            // Se o arquivo está bloqueado, tentar com FileShare.ReadWrite
            await Task.Delay(500);

            using var sourceStream = new FileStream(_dbFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var destinationStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await sourceStream.CopyToAsync(destinationStream);
        }
    }

    public async Task RestoreAsync()
    {
        var srcPath = Path.Combine(_backupFolderPath, Path.GetFileName(_dbFilePath));

        if (!File.Exists(srcPath))
            throw new FileNotFoundException($"Arquivo de backup não encontrado em: {srcPath}");

        try
        {
            // Descartar conexões do pool para liberar o arquivo de banco
            SqliteConnection.ClearAllPools();
            await Task.Delay(300);

            // Fazer checkpoint e truncar o WAL para consolidar dados
            try
            {
                using (var connection = new SqliteConnection($"Data Source={_dbFilePath}"))
                {
                    await connection.OpenAsync();
                    using var command = connection.CreateCommand();
                    command.CommandText = "PRAGMA wal_checkpoint(TRUNCATE)";
                    await command.ExecuteScalarAsync();
                }
                SqliteConnection.ClearAllPools();
            }
            catch
            {
                // Ignorar erros durante checkpoint se o banco está corrompido
            }

            await Task.Delay(200);

            // Copiar arquivo de backup
            using (var sourceStream = new FileStream(srcPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var destinationStream = new FileStream(_dbFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await sourceStream.CopyToAsync(destinationStream);
            }

            // Limpar arquivos WAL e SHM para resetar o estado
            CleanupSqliteAuxiliaryFiles();

            // Executar logout com limpeza de UI
            await PerformLogoutWithUICleanupAsync();

            // Restaurar o DbContext após logout
            RestoreDbContext();
        }
        catch (IOException)
        {
            // Se o arquivo está bloqueado, tentar com delay e pool clearing múltiplos
            await Task.Delay(1000);
            SqliteConnection.ClearAllPools();
            await Task.Delay(500);
            SqliteConnection.ClearAllPools();
            await Task.Delay(500);

            using (var sourceStream = new FileStream(srcPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var destinationStream = new FileStream(_dbFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await sourceStream.CopyToAsync(destinationStream);
            }

            CleanupSqliteAuxiliaryFiles();

            // Executar logout com limpeza de UI
            await PerformLogoutWithUICleanupAsync();

            RestoreDbContext();
        }
    }

    /// <summary>
    /// Remove arquivos auxiliares do SQLite (WAL e SHM).
    /// </summary>
    private void CleanupSqliteAuxiliaryFiles()
    {
        var walPath = _dbFilePath + "-wal";
        if (File.Exists(walPath))
            File.Delete(walPath);

        var shmPath = _dbFilePath + "-shm";
        if (File.Exists(shmPath))
            File.Delete(shmPath);

        // Remover possíveis arquivos de journal
        var parentDir = Path.GetDirectoryName(_dbFilePath);
        if (!string.IsNullOrEmpty(parentDir) && Directory.Exists(parentDir))
        {
            var dbFileName = Path.GetFileName(_dbFilePath);
            var journalFile = Path.Combine(parentDir, dbFileName + "-journal");
            if (File.Exists(journalFile))
                File.Delete(journalFile);

            var backupFile = Path.Combine(parentDir, dbFileName + "-backup");
            if (File.Exists(backupFile))
                File.Delete(backupFile);
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
