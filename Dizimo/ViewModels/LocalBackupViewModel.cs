using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Infrastructure.Services;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;

namespace Dizimo.ViewModels;

public partial class LocalBackupViewModel : ObservableObject
{
    private readonly LocalBackupService _backupService;
    public LocalBackupViewModel(LocalBackupService backupService)
    {
        _backupService = backupService;
        BackupFolderPath = _backupService.BackupFolderPath;
    }

    [ObservableProperty]
    private string backupFolderPath;

    [RelayCommand]
    public async Task EscolherPastaAsync()
    {
        var folder = await FolderPicker.Default.PickAsync();
        if (folder != null)
        {
            BackupFolderPath = folder.Path;
            _backupService.SetBackupFolder(folder.Path);
            await Application.Current.MainPage.DisplayAlert("Backup", $"Pasta de backup configurada: {folder.Path}", "OK");
        }
    }

    [RelayCommand]
    public async Task BackupAsync()
    {
        try
        {
            await _backupService.BackupAsync();
            await Application.Current.MainPage.DisplayAlert("Backup", "Backup realizado com sucesso na pasta configurada.", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Erro", $"Erro ao realizar backup: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task RestoreAsync()
    {
        try
        {
            await _backupService.RestoreAsync();
            await Application.Current.MainPage.DisplayAlert("Backup", "Restauração realizada com sucesso.", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Erro", $"Erro ao restaurar backup: {ex.Message}", "OK");
        }
    }
}
