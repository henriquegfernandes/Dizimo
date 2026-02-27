using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Dizimo.Infrastructure.Persistence;

namespace Dizimo.Services;

public class LocalBackupService(string dbFilePath, SessaoService sessaoService, IServiceProvider serviceProvider)
{
    private readonly string _dbFilePath = dbFilePath;
    private string _backupFolderPath = Preferences.Default.Get("BackupFolderPath", FileSystem.Current.AppDataDirectory);
    private readonly SessaoService _sessaoService = sessaoService;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

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

            // Limpar o pool de conexões SQLite para garantir que todas as conexões sejam fechadas
            SqliteConnection.ClearAllPools();
            await Task.Delay(200);

            // Fazer checkpoint e truncar o WAL para consolidar dados e fechar logs
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
            var walPath = _dbFilePath + "-wal";
            if (File.Exists(walPath))
                File.Delete(walPath);

            var shmPath = _dbFilePath + "-shm";
            if (File.Exists(shmPath))
                File.Delete(shmPath);

            // Limpar arquivos temporários do SQLite
            var parentDir = Path.GetDirectoryName(_dbFilePath);
            var dbFileName = Path.GetFileName(_dbFilePath);
            if (!string.IsNullOrEmpty(parentDir) && Directory.Exists(parentDir))
            {
                // Remover possíveis arquivos de journal
                var journalFile = Path.Combine(parentDir, dbFileName + "-journal");
                if (File.Exists(journalFile))
                    File.Delete(journalFile);

                // Remover possíveis arquivos de backup
                var backupFile = Path.Combine(parentDir, dbFileName + "-backup");
                if (File.Exists(backupFile))
                    File.Delete(backupFile);
            }

            // Fazer logout forçado para que usuário faça login com dados atualizados
            _sessaoService?.Logout();

            // Restaurar o DbContext após logout
            if (_serviceProvider != null)
            {
                using var scope = _serviceProvider.CreateScope();
                var newContext = scope.ServiceProvider.GetRequiredService<DizimoDbContext>();
                newContext.Database.Migrate();
            }

            // Limpar o cache de preferências relacionadas à sessão se necessário
            await Task.Delay(100);
        }
        catch (IOException ex)
        {
            // Se o arquivo está bloqueado, tentar com delay maior
            await Task.Delay(1000);

            // Limpar múltiplas vezes para garantir que o pool foi esvaziado
            SqliteConnection.ClearAllPools();
            await Task.Delay(500);
            SqliteConnection.ClearAllPools();
            await Task.Delay(500);

            using (var sourceStream = new FileStream(srcPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var destinationStream = new FileStream(_dbFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await sourceStream.CopyToAsync(destinationStream);
            }

            // Limpar arquivos WAL e SHM
            var walPath = _dbFilePath + "-wal";
            if (File.Exists(walPath))
                File.Delete(walPath);

            var shmPath = _dbFilePath + "-shm";
            if (File.Exists(shmPath))
                File.Delete(shmPath);

            // Fazer logout forçado
            _sessaoService?.Logout();

            // Restaurar o DbContext após logout
            if (_serviceProvider != null)
            {
                using var scope = _serviceProvider.CreateScope();
                var newContext = scope.ServiceProvider.GetRequiredService<DizimoDbContext>();
                newContext.Database.Migrate();
            }
        }
    }
}
