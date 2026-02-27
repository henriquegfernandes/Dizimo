using System.Windows.Input;
using Dizimo.ViewModels;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Dizimo.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    public MainViewModel MainVm { get; }
    public LocalBackupViewModel BackupVm { get; }

    private bool _isAdmin;
    public bool IsAdmin
    {
        get => _isAdmin;
        set => SetProperty(ref _isAdmin, value);
    }

    private string _usuarioNome = string.Empty;
    public string UsuarioNome
    {
        get => _usuarioNome;
        set => SetProperty(ref _usuarioNome, value);
    }

    private string _usuarioPerfil = string.Empty;
    public string UsuarioPerfil
    {
        get => _usuarioPerfil;
        set => SetProperty(ref _usuarioPerfil, value);
    }

    public ShellViewModel(MainViewModel mainVm, LocalBackupViewModel backupVm)
    {
        MainVm = mainVm;
        BackupVm = backupVm;

        UpdateValues();
        MainVm.PropertyChanged += (s, e) => UpdateValues();
    }

    private void UpdateValues()
    {
        IsAdmin = MainViewModel.IsAdmin;
        UsuarioNome = MainViewModel.UsuarioNome;
        UsuarioPerfil = MainViewModel.UsuarioPerfil;
    }

    public ICommand LogoutCommand => MainVm.LogoutCommand;
    public ICommand EscolherPastaCommand => BackupVm.EscolherPastaCommand;
    public ICommand BackupCommand => BackupVm.BackupCommand;
    public ICommand RestoreCommand => BackupVm.RestoreCommand;
    public string BackupFolderPath => BackupVm.BackupFolderPath ?? string.Empty;
}
