using System.Windows.Input;
using Dizimo.ViewModels;
using CommunityToolkit.Mvvm.Input;

namespace Dizimo.ViewModels;

public class ShellViewModel(MainViewModel mainVm, LocalBackupViewModel backupVm)
{
    public MainViewModel MainVm { get; } = mainVm;
    public LocalBackupViewModel BackupVm { get; } = backupVm;

    public ICommand LogoutCommand => MainVm.LogoutCommand;
    public ICommand EscolherPastaCommand => BackupVm.EscolherPastaCommand;
    public ICommand BackupCommand => BackupVm.BackupCommand;
    public ICommand RestoreCommand => BackupVm.RestoreCommand;
    public static string UsuarioNome => MainViewModel.UsuarioNome;
    public static string UsuarioPerfil => MainViewModel.UsuarioPerfil;
    public string BackupFolderPath => BackupVm.BackupFolderPath ?? string.Empty;
    public static bool IsAdmin => MainViewModel.IsAdmin;
}
