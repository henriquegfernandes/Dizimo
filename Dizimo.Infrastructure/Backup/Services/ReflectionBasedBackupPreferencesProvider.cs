using Microsoft.Extensions.Logging;

namespace Dizimo.Infrastructure.Backup.Services;

/// <summary>
///     Adaptador para o PreferencesService usando reflection.
///     Esta é uma implementação de ponte que permite usar a interface IBackupPreferencesProvider
///     com o PreferencesService antigo, sem modificá-lo.
///     Uma refatoração futura deveria injetar PreferencesService diretamente.
/// </summary>
public class ReflectionBasedBackupPreferencesProvider : IBackupPreferencesProvider
{
    private const string PreferencesKey = "BackupFolderPath";
    private readonly ILogger<ReflectionBasedBackupPreferencesProvider> _logger;
    private readonly object? _preferencesService;

    public ReflectionBasedBackupPreferencesProvider(
        object? preferencesService,
        ILogger<ReflectionBasedBackupPreferencesProvider> logger)
    {
        _preferencesService = preferencesService;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    ///     Obtém o caminho da pasta de backup usando reflection.
    /// </summary>
    public string? GetBackupFolderPath()
    {
        if (_preferencesService == null)
            return null;

        try
        {
            var getMethod = _preferencesService.GetType().GetMethod("Get");
            if (getMethod != null)
            {
                var defaultPath = GetDefaultBackupPath();
                var result = getMethod.Invoke(_preferencesService, new object[] { PreferencesKey, defaultPath });
                return result as string;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Erro ao obter caminho de backup: {Message}", ex.Message);
        }

        return null;
    }

    /// <summary>
    ///     Define o caminho da pasta de backup usando reflection.
    /// </summary>
    public void SetBackupFolderPath(string folderPath)
    {
        if (_preferencesService == null || string.IsNullOrWhiteSpace(folderPath))
            return;

        try
        {
            var setMethod = _preferencesService.GetType().GetMethod("Set");
            setMethod?.Invoke(_preferencesService, new object[] { PreferencesKey, folderPath });
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Erro ao salvar caminho de backup: {Message}", ex.Message);
        }
    }

    private static string GetDefaultBackupPath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            BackupServiceConfiguration.BackupFolderName,
            BackupServiceConfiguration.BackupSubfolderName);
    }
}