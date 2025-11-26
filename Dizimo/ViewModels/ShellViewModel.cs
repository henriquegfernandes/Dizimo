using System.Windows.Input;
using Dizimo.ViewModels;
using CommunityToolkit.Mvvm.Input;

namespace Dizimo.ViewModels
{
    public class ShellViewModel
    {
        public MainViewModel MainVm { get; }
        public LocalBackupViewModel BackupVm { get; }

        public ICommand LogoutCommand => MainVm.LogoutCommand;
        public ICommand EscolherPastaCommand => BackupVm.EscolherPastaCommand;
        public ICommand BackupCommand => BackupVm.BackupCommand;
        public ICommand RestoreCommand => BackupVm.RestoreCommand;
        public string UsuarioNome => MainVm.UsuarioNome;
        public string UsuarioPerfil => MainVm.UsuarioPerfil;
        public string BackupFolderPath => BackupVm.BackupFolderPath ?? string.Empty;
        public bool IsAdmin => MainVm.IsAdmin;

        public ShellViewModel(MainViewModel mainVm, LocalBackupViewModel backupVm)
        {
            MainVm = mainVm;
            BackupVm = backupVm;
        }
    }
}
