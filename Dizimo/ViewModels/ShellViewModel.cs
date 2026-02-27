using System.Windows.Input;
using Dizimo.ViewModels;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Dizimo.ViewModels;

public partial class ShellViewModel : ObservableObject, IDisposable
{
    private readonly MainViewModel _mainVm;
    private readonly LocalBackupViewModel _backupVm;
    private bool _disposed;

    public MainViewModel MainVm => _mainVm;
    public LocalBackupViewModel BackupVm => _backupVm;

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
        _mainVm = mainVm;
        _backupVm = backupVm;

        UpdateValues();
    }

    private void UpdateValues()
    {
        IsAdmin = MainViewModel.IsAdmin;
        UsuarioNome = MainViewModel.UsuarioNome;
        UsuarioPerfil = MainViewModel.UsuarioPerfil;
    }

    public ICommand LogoutCommand => _mainVm.LogoutCommand;
    public ICommand EscolherPastaCommand => _backupVm.EscolherPastaCommand;
    public ICommand BackupCommand => _backupVm.BackupCommand;
    public ICommand RestoreCommand => _backupVm.RestoreCommand;
    public string BackupFolderPath => _backupVm.BackupFolderPath ?? string.Empty;

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
