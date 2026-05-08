using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dizimo.Infrastructure.Backup.Services;

/// <summary>
/// Extensões para registrar serviços de backup no container de injeção de dependências.
/// </summary>
public static class BackupServiceCollectionExtensions
{
    /// <summary>
    /// Registra os serviços de backup no container de DI.
    /// </summary>
    /// <param name="services">Coleção de serviços.</param>
    /// <param name="dbPath">Caminho do arquivo de banco de dados.</param>
    /// <param name="preferencesService">Serviço de preferências (opcional, para compatibilidade com código legado).</param>
    /// <returns>Coleção de serviços para encadeamento.</returns>
    public static IServiceCollection AddBackupServices(
        this IServiceCollection services,
        string dbPath,
        object? preferencesService = null)
    {
        if (string.IsNullOrWhiteSpace(dbPath))
            throw new ArgumentNullException(nameof(dbPath), "Caminho do banco de dados não pode ser nulo ou vazio");

        // Registrar serviço de operações de arquivo
        services.AddSingleton<IFileOperationService, FileOperationService>();

        // Registrar provedor de preferências
        services.AddSingleton<IBackupPreferencesProvider>(sp =>
            new ReflectionBasedBackupPreferencesProvider(
                preferencesService,
                sp.GetRequiredService<ILogger<ReflectionBasedBackupPreferencesProvider>>()));

        // Registrar serviço de backup local
        services.AddSingleton<LocalBackupService>(sp =>
            new LocalBackupService(
                dbPath,
                sp,
                sp.GetRequiredService<IBackupPreferencesProvider>(),
                sp.GetRequiredService<IFileOperationService>(),
                sp.GetRequiredService<ILogger<LocalBackupService>>()));

        // NOTA: BackupOnCloseService é registrado em Program.cs no namespace Dizimo.Services
        // Não pode ser registrado aqui sem criar dependência circular
        // services.AddSingleton<BackupOnCloseService>();

        return services;
    }
}



