using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Infrastructure.Services;
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
            await Application.Current.MainPage.DisplayAlert("Backup", "Backup realizado com sucesso na nuvem.", "OK");
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
            await _backupService.RestoreFromCloudAsync();
            await Application.Current.MainPage.DisplayAlert("Backup", "Restauração realizada com sucesso.", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Erro", $"Erro ao restaurar backup: {ex.Message}", "OK");
        }
    }
}
