namespace Dizimo.Services;

public class BackupOnCloseService
{
    private readonly LocalBackupService _backupService;

    public BackupOnCloseService(LocalBackupService backupService)
    {
        _backupService = backupService;
    }

    public async Task<bool> PerformBackupAsync(Page? mainPage)
    {
        try
        {
            if (_backupService == null || !_backupService.IsBackupFolderConfigured)
                return true;

            try
            {
                await _backupService.BackupAsync();
                System.Diagnostics.Debug.WriteLine("Backup executado com sucesso ao fechar o app");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao realizar backup ao fechar: {ex.Message}");
                return true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao tentar fazer backup ao fechar: {ex.Message}");
            return true;
        }
    }
}
