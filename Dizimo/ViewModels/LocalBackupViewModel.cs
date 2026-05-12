using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Infrastructure.Backup.Services;

namespace Dizimo.ViewModels;

public partial class LocalBackupViewModel : ObservableObject
{
    private const string AutoBackupEnabledKey = "AutoBackupEnabled";
    private readonly IAuthenticationService _authenticationService;
    private readonly LocalBackupService _backupService;
    private readonly IDialogService _dialogService;
    private readonly IPreferencesService? _preferencesService;

    private bool _isAutoBackupEnabled;

    private string? backupFolderPath;

    public LocalBackupViewModel(LocalBackupService backupService, IDialogService? dialogService = null,
        IAuthenticationService? authenticationService = null, IPreferencesService? preferencesService = null)
    {
        _backupService = backupService;
        _dialogService = dialogService ?? new DialogService();
        _authenticationService = authenticationService ??
                                 throw new ArgumentNullException(nameof(authenticationService),
                                     "IAuthenticationService é obrigatório");
        _preferencesService = preferencesService;
        LoadBackupFolderPath();
        LoadAutoBackupPreference();
    }

    public string? BackupFolderPath
    {
        get => backupFolderPath;
        set => SetProperty(ref backupFolderPath, value);
    }

    public bool IsAutoBackupEnabled
    {
        get => _isAutoBackupEnabled;
        set
        {
            if (SetProperty(ref _isAutoBackupEnabled, value))
            {
                SaveAutoBackupPreference(value);
                _ = OnAutoBackupToggleAsync(value);
            }
        }
    }

    /// <summary>
    ///     Define o callback a ser executado após restauração bem-sucedida (DEPRECATED - usar IAuthenticationService)
    /// </summary>
    public void SetOnRestoreSuccess(Func<Task> onRestoreSuccess)
    {
        // Este método mantém compatibilidade com código legado
        // A configuração real é feita via IAuthenticationService no AppRootViewModel
        Debug.WriteLine(
            "[AUTH] SetOnRestoreSuccess (deprecated) chamado - use IAuthenticationService.SetOnLogoutComplete");
    }

    private void LoadBackupFolderPath()
    {
        BackupFolderPath = _backupService.BackupFolderPath;
    }

    private void LoadAutoBackupPreference()
    {
        if (_preferencesService != null)
        {
            _isAutoBackupEnabled = _preferencesService.Get(AutoBackupEnabledKey, false);
            OnPropertyChanged(nameof(IsAutoBackupEnabled));
            Debug.WriteLine($"[BACKUP] Preferência de backup automático carregada: {_isAutoBackupEnabled}");
        }
    }

    private void SaveAutoBackupPreference(bool value)
    {
        if (_preferencesService != null)
        {
            _preferencesService.Set(AutoBackupEnabledKey, value);
            Debug.WriteLine($"[BACKUP] Preferência de backup automático salva: {value}");
        }
    }

    private async Task OnAutoBackupToggleAsync(bool enabled)
    {
        if (enabled)
            await _dialogService.ShowAlertAsync(
                "Backup Automático Ativado",
                "✓ Backup automático foi ativado!\n\n" +
                "Toda vez que você fechar o aplicativo, um arquivo de backup será criado automaticamente na pasta:\n\n" +
                $"{_backupService.BackupFolderPath}\n\n" +
                "O backup será feito de forma silenciosa, sem interromper o fechamento do app."
            );
    }

    [RelayCommand]
    public async Task EscolherPastaAsync()
    {
        Debug.WriteLine("[NAV] Escolher pasta de backup");

        try
        {
            var selectedPath = await _dialogService.ShowFolderPickerAsync(
                "Selecione a pasta para backup",
                BackupFolderPath
            );

            if (!string.IsNullOrEmpty(selectedPath))
            {
                BackupFolderPath = selectedPath;
                _backupService.SetBackupFolder(selectedPath);
                await _dialogService.ShowSuccessAsync("Pasta de backup atualizada com sucesso!");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] Erro ao escolher pasta: {ex.Message}");
            await _dialogService.ShowErrorAsync($"Erro ao escolher pasta: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task BackupAsync()
    {
        try
        {
            await _backupService.BackupAsync();
            await _dialogService.ShowAlertAsync("Backup", "Backup realizado com sucesso na pasta configurada.");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Erro ao realizar backup: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task RestoreAsync()
    {
        try
        {
            // Abre o file picker na pasta de backup
            var selectedFile = await _dialogService.ShowFilePickerAsync(
                "Selecione o arquivo de backup para restaurar",
                BackupFolderPath,
                new[] { "db" }
            );

            if (string.IsNullOrEmpty(selectedFile))
            {
                Debug.WriteLine("[INFO] Restauração cancelada pelo usuário");
                return;
            }

            // Mostra confirmação com aviso
            var confirmed = await _dialogService.ShowConfirmAsync(
                "⚠️ Confirmar Restauração",
                $"Você está prestes a restaurar o banco de dados de:\n\n{Path.GetFileName(selectedFile)}\n\n" +
                "Esta operação irá SUBSTITUIR todos os dados atuais pelos dados do backup.\n" +
                "Dados que não estão no backup serão PERDIDOS.\n\n" +
                "O aplicativo será encerrado e reiniciado automaticamente.\n\n" +
                "Tem certeza que deseja continuar?",
                "Sim, restaurar",
                "Cancelar"
            );

            if (!confirmed)
            {
                Debug.WriteLine("[INFO] Restauração cancelada pelo usuário na confirmação");
                return;
            }

            // Restaura o banco de dados
            Debug.WriteLine("[INFO] Iniciando restauração do banco...");
            await _backupService.RestoreFromFileAsync(selectedFile);
            Debug.WriteLine("[INFO] Banco restaurado com sucesso");

            // Aguardar para garantir limpeza completa
            await Task.Delay(1000);

            // Mostra sucesso
            await _dialogService.ShowAlertAsync("Sucesso",
                "Restauração realizada com sucesso! O aplicativo será encerrado e reiniciado.");
            Debug.WriteLine("[INFO] Dialogo de sucesso exibido");

            // Aguardar mais um pouco
            await Task.Delay(500);

            // Reiniciar a aplicação completamente
            Debug.WriteLine("[INFO] Encerrando aplicação para reiniciar...");

            // Limpar a sessão do usuário ANTES de sair
            SessaoService.Logout();
            Debug.WriteLine("[AUTH] Sessão do usuário limpa");

            var currentProcess = Process.GetCurrentProcess();
            var exePath = currentProcess.MainModule?.FileName;

            if (!string.IsNullOrEmpty(exePath))
            {
                // Iniciar nova instância
                Process.Start(exePath);
                Debug.WriteLine($"[INFO] Nova instância iniciada: {exePath}");
            }

            // Encerrar aplicação atual
            Debug.WriteLine("[INFO] Encerrando aplicação atual...");
            Environment.Exit(0);
        }
        catch (FileNotFoundException ex)
        {
            await _dialogService.ShowErrorAsync($"Arquivo não encontrado: {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] Erro ao restaurar backup: {ex.Message}");
            await _dialogService.ShowErrorAsync($"Erro ao restaurar backup: {ex.Message}");
        }
    }
}