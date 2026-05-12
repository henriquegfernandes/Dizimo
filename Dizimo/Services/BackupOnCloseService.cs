using System.Diagnostics;
using Dizimo.Infrastructure.Backup.Services;

namespace Dizimo.Services;

public class BackupOnCloseService
{
    private const string AutoBackupEnabledKey = "AutoBackupEnabled";
    private readonly LocalBackupService _backupService;
    private readonly IDialogService? _dialogService;
    private readonly IPreferencesService? _preferencesService;

    public BackupOnCloseService(LocalBackupService backupService, IPreferencesService? preferencesService = null,
        IDialogService? dialogService = null)
    {
        _backupService = backupService;
        _preferencesService = preferencesService;
        _dialogService = dialogService;
    }

    /// <summary>
    ///     Verifica se backup automático está habilitado
    /// </summary>
    public bool IsAutoBackupEnabled
    {
        get
        {
            if (_preferencesService == null)
                return false;

            return _preferencesService.Get(AutoBackupEnabledKey, false);
        }
    }

    /// <summary>
    ///     Executa backup automático ao fechar a aplicação com confirmação do usuário
    /// </summary>
    public async Task<bool> PerformBackupWithConfirmationAsync()
    {
        try
        {
            // Verificar se backup automático está habilitado
            if (!IsAutoBackupEnabled)
            {
                Debug.WriteLine("[BACKUP] Backup automático desabilitado");
                return true;
            }

            // Verificar se o serviço está disponível
            if (_backupService == null)
            {
                Debug.WriteLine("[BACKUP] LocalBackupService não disponível");
                return true;
            }

            // Verificar se a pasta tem um caminho válido
            if (string.IsNullOrWhiteSpace(_backupService.BackupFolderPath))
            {
                Debug.WriteLine("[BACKUP] Caminho de backup inválido");
                return true;
            }

            // IMPORTANTE: Quando este método é chamado durante o encerramento da aplicação,
            // a janela principal já foi fechada, então NÃO podemos mostrar diálogos.
            // O usuario já recebeu a confirmação quando ativou o toggle de backup automático,
            // então apenas fazemos o backup silenciosamente.

            Debug.WriteLine("[BACKUP] Executando backup automático silenciosamente...");

            // Executar backup sem confirmação
            await _backupService.BackupAsync();
            Debug.WriteLine("[BACKUP] Backup automático executado com sucesso ao fechar o app");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[BACKUP] Erro ao fazer backup automático ao fechar: {ex.Message}");
            return true; // Permite fechar mesmo se houver erro
        }
    }

    /// <summary>
    ///     Versão legada: Executa backup automático ao fechar a aplicação sem confirmação
    /// </summary>
    public async Task<bool> PerformBackupAsync(object? context = null)
    {
        try
        {
            if (_backupService == null || !_backupService.IsBackupFolderConfigured)
                return true;

            try
            {
                await _backupService.BackupAsync();
                Debug.WriteLine("Backup executado com sucesso ao fechar o app");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao realizar backup ao fechar: {ex.Message}");
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erro ao tentar fazer backup ao fechar: {ex.Message}");
            return true;
        }
    }
}