namespace Dizimo.Services;

public class LocalBackupService
{
    private readonly string _dbFilePath;
    private string _backupFolderPath;

    public LocalBackupService(string dbFilePath)
    {
        _dbFilePath = dbFilePath;
        _backupFolderPath = Preferences.Default.Get("BackupFolderPath", FileSystem.Current.AppDataDirectory);
    }

    public string BackupFolderPath
    {
        get => _backupFolderPath;
        set
        {
            _backupFolderPath = value;
            Preferences.Default.Set("BackupFolderPath", value);
        }
    }

    public void SetBackupFolder(string folderPath)
    {
        BackupFolderPath = folderPath;
    }

    public async Task BackupAsync()
    {
        var destPath = Path.Combine(_backupFolderPath, Path.GetFileName(_dbFilePath));
        File.Copy(_dbFilePath, destPath, true);
        await Task.CompletedTask;
    }

    public async Task RestoreAsync()
    {
        var srcPath = Path.Combine(_backupFolderPath, Path.GetFileName(_dbFilePath));
        
        if (!File.Exists(srcPath))
            throw new FileNotFoundException($"Arquivo de backup não encontrado em: {srcPath}");
        
        File.Copy(srcPath, _dbFilePath, true);
        await Task.CompletedTask;
    }
}
