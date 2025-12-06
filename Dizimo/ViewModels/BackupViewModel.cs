using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;

namespace Dizimo.ViewModels;

public partial class BackupViewModel : ObservableObject
{
    private readonly BackupService _backupService;
    public BackupViewModel(BackupService backupService)
    {
        _backupService = backupService;
    }

    [RelayCommand]
    public async Task BackupAsync()
    {
        try
        {
            await _backupService.BackupToCloudAsync();
            var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Backup", "Backup realizado com sucesso na nuvem.", "OK");
        }
        catch (Exception ex)
        {
            var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Erro", $"Erro ao realizar backup: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task RestoreAsync()
    {
        try
        {
            await _backupService.RestoreFromCloudAsync();
            var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Backup", "Restauração realizada com sucesso.", "OK");
        }
        catch (Exception ex)
        {
            var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
            if (mainPage != null)
                await mainPage.DisplayAlertAsync("Erro", $"Erro ao restaurar backup: {ex.Message}", "OK");
        }
    }
}
