using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Dizimo.ViewModels;

public partial class LocalBackupViewModel : ObservableObject
{
    private readonly LocalBackupService _backupService;

    private string? _backupFolderPath;

    public LocalBackupViewModel(LocalBackupService backupService)
    {
        _backupService = backupService;
    }

    public string? BackupFolderPath
    {
        get => _backupFolderPath;
        set => SetProperty(ref _backupFolderPath, value);
    }

    [RelayCommand]
    public async Task EscolherPastaAsync()
    {
        var folder = await FolderPicker.Default.PickAsync();
        if (folder != null)
        {
            _backupService.SetBackupFolder(folder.Folder?.Path ?? string.Empty);
            BackupFolderPath = folder.Folder?.Path;
            var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.Count > 0
                ? Microsoft.Maui.Controls.Application.Current.Windows[0].Page
                : null;
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Backup", $"Pasta de backup configurada: {folder.Folder?.Path}", "OK");
        }
    }

    [RelayCommand]
    public async Task BackupAsync()
    {
        try
        {
            await _backupService.BackupAsync();
            var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.Count > 0
                ? Microsoft.Maui.Controls.Application.Current.Windows[0].Page
                : null;
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Backup", "Backup realizado com sucesso na pasta configurada.", "OK");
        }
        catch (Exception ex)
        {
            var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.Count > 0
                ? Microsoft.Maui.Controls.Application.Current.Windows[0].Page
                : null;
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Erro", $"Erro ao realizar backup: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task RestoreAsync()
    {
        try
        {
            await _backupService.RestoreAsync();
            var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.Count > 0
                ? Microsoft.Maui.Controls.Application.Current.Windows[0].Page
                : null;
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Backup", "Restauração realizada com sucesso.", "OK");
        }
        catch (Exception ex)
        {
            var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.Count > 0
                ? Microsoft.Maui.Controls.Application.Current.Windows[0].Page
                : null;
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Erro", $"Erro ao restaurar backup: {ex.Message}", "OK");
        }
    }
}
