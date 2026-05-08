using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Dizimo.Infrastructure.Persistence;
using Dizimo.Infrastructure.Repositories;
using Dizimo.Domain.Repositories;
using Dizimo.ViewModels;
using Dizimo.Application.Dizimistas.Handlers;
using Dizimo.Application.Ofertas.Handlers;
using Dizimo.Application.Dashboard;
using Dizimo.Application.Usuarios.Handlers;
using Dizimo.Application.Reporting.Services;
using Dizimo.Infrastructure.Backup.Services;
using Dizimo.Services;

namespace Dizimo;

class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
    }
    /// <summary>
    /// Aplicação Avalonia com DI configurado
    /// </summary>
    public partial class App : global::Avalonia.Application
    {
        private ServiceProvider? _serviceProvider;
        private BackupOnCloseService? _backupOnCloseService;
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
        public override void OnFrameworkInitializationCompleted()
        {
            _serviceProvider = ConfigureServices();
            
            // Inicializa o serviço de temas com IPreferencesService para persistência
            var preferencesService = _serviceProvider.GetRequiredService<IPreferencesService>();
            ThemeService.Initialize(this, preferencesService);
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                _backupOnCloseService = _serviceProvider.GetService<BackupOnCloseService>();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = _serviceProvider.GetRequiredService<AppRootViewModel>()
                };
                desktop.Exit += async (s, e) => await OnApplicationExitingAsync();
            }
            base.OnFrameworkInitializationCompleted();
        }
        private ServiceProvider ConfigureServices()
        {
            var ptBr = new CultureInfo("pt-BR");
            CultureInfo.DefaultThreadCurrentCulture = ptBr;
            CultureInfo.DefaultThreadCurrentUICulture = ptBr;
            var services = new ServiceCollection();
            services.AddSingleton<IDataPathProvider, DataPathProvider>();
            services.AddSingleton<IDialogService, AvaloniaDialogService>();
            services.AddSingleton<IPreferencesService, LocalPreferencesService>();
            services.AddSingleton<IFilterCacheService, FilterCacheService>();
            services.AddSingleton<IAuthenticationService, AuthenticationService>();
            var dataPathProvider = new DataPathProvider();
            var dbPath = dataPathProvider.GetDatabasePath();
            services.AddDbContext<DizimoDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddSingleton<ModalErrorHandler>();
            services.AddTransient<MainPageViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<LocalBackupViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<SetupViewModel>();
            services.AddTransient<ShellViewModel>();
            services.AddTransient<AppRootViewModel>();
            services.AddScoped<DashboardService>();
            services.AddScoped<GetDizimistaHandlers>();
            services.AddScoped<CreateDizimistaHandler>();
            services.AddScoped<UpdateDizimistaHandler>();
            services.AddScoped<DeleteDizimistaHandler>();
            services.AddScoped<InativarDizimistaHandler>();
            services.AddScoped<GetUsuarioHandlers>();
            services.AddScoped<DizimistaExcelService>();
            services.AddScoped<DizimistaPdfService>();
            services.AddScoped<AniversariantesExcelService>();
            services.AddScoped<AniversariantesPdfService>();
            services.AddTransient<DizimistaListViewModel>();
            services.AddTransient<DizimistaCadastroViewModel>();
            services.AddTransient<DizimistaDetalhesViewModel>();
            services.AddScoped<OfertaExcelService>();
            services.AddScoped<OfertaPdfService>();
            services.AddTransient<OfertaListViewModel>();
            services.AddTransient<OfertaCadastroViewModel>();
            services.AddScoped<CreateOfertaHandler>();
            services.AddScoped<UpdateOfertaHandler>();
            services.AddScoped<DeleteOfertaHandler>();
            services.AddScoped<GetOfertaHandlers>();
            services.AddScoped<CreateUsuarioHandler>();
            services.AddScoped<UpdateUsuarioHandler>();
            services.AddScoped<DeleteUsuarioHandler>();
            services.AddTransient<UsuarioListViewModel>();
            services.AddTransient<UsuarioCadastroViewModel>();
            services.AddSingleton<SessaoService>();
            services.AddSingleton<INavigationService, NavigationService>();
            
            // Registrar serviços de backup com nova arquitetura refatorada
            services.AddSingleton<IFileOperationService, FileOperationService>();
            services.AddSingleton<IBackupPreferencesProvider>(sp =>
                new ReflectionBasedBackupPreferencesProvider(
                    sp.GetRequiredService<IPreferencesService>(),
                    sp.GetRequiredService<ILogger<ReflectionBasedBackupPreferencesProvider>>()));
            services.AddSingleton<LocalBackupService>(sp =>
                new LocalBackupService(
                    dbPath,
                    sp,
                    sp.GetRequiredService<IBackupPreferencesProvider>(),
                    sp.GetRequiredService<IFileOperationService>(),
                    sp.GetRequiredService<ILogger<LocalBackupService>>()));
            
            services.AddSingleton(sp => new BackupOnCloseService(
                sp.GetRequiredService<LocalBackupService>(),
                sp.GetRequiredService<IPreferencesService>(),
                sp.GetRequiredService<IDialogService>()
            ));
#if DEBUG
            services.AddLogging(configure => configure.AddDebug());
#else
            // Em Release, registrar logging para evitar DI errors ao injetar ILogger<T>
            services.AddLogging();
#endif
            var serviceProvider = services.BuildServiceProvider();
            
            // Migrate do banco com tratamento de corrupção
            MigrateDatabase(serviceProvider, dbPath);
            
            Ioc.Default.ConfigureServices(serviceProvider);
            return serviceProvider;
        }

        /// <summary>
        /// Realiza migração do banco com recuperação automática de corrupção
        /// </summary>
        private static void MigrateDatabase(ServiceProvider serviceProvider, string dbPath)
        {
            bool hadCorruption = false;
            
            using (var scope = serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DizimoDbContext>();
                try
                {
                    db.Database.Migrate();
                    System.Diagnostics.Debug.WriteLine("Migração do banco bem-sucedida");
                }
                catch (Exception ex) when (IsCorruptedDatabaseError(ex))
                {
                    System.Diagnostics.Debug.WriteLine($"Banco de dados corrompido detectado: {ex.Message}");
                    hadCorruption = true;
                    
                    // Tentar descartar o DbContext para liberar a conexão
                    try
                    {
                        db.Database.ExecuteSqlRaw("PRAGMA integrity_check;");
                    }
                    catch { }
                    
                    try
                    {
                        db.Database.CloseConnection();
                    }
                    catch { }
                    
                    try
                    {
                        db.Dispose();
                    }
                    catch { }
                    
                    // Aguardar para garantir que todas as conexões foram fechadas
                    Thread.Sleep(800);
                    
                    // Limpar arquivos corrompidos do SQLite
                    CleanupCorruptedDatabase(dbPath);
                    
                    // Aguardar novamente para liberar locks
                    Thread.Sleep(800);
                    
                    System.Diagnostics.Debug.WriteLine("Arquivos corrompidos removidos. Recriando banco...");
                }
                
                // Se houve corrupção, criar novo DbContext para tentar novamente
                if (hadCorruption)
                {
                    bool retrySuccess = false;
                    int retryCount = 0;
                    const int maxRetries = 3;
                    
                    while (!retrySuccess && retryCount < maxRetries)
                    {
                        retryCount++;
                        try
                        {
                            System.Diagnostics.Debug.WriteLine($"Tentativa {retryCount}/{maxRetries} de recriar banco...");
                            
                            // Criar novo scope e novo DbContext
                            using (var retryScope = serviceProvider.CreateScope())
                            {
                                var retryDb = retryScope.ServiceProvider.GetRequiredService<DizimoDbContext>();
                                retryDb.Database.EnsureCreated();
                                retryDb.Database.Migrate();
                                System.Diagnostics.Debug.WriteLine("Banco de dados recriado com sucesso após limpeza!");
                                retrySuccess = true;
                            }
                        }
                        catch (Exception retryEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Tentativa {retryCount} falhou: {retryEx.Message}");
                            
                            if (retryCount < maxRetries)
                            {
                                // Aguardar antes de tentar novamente
                                Thread.Sleep(500);
                                // Tentar limpar de novo
                                CleanupCorruptedDatabase(dbPath);
                                Thread.Sleep(500);
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("Todas as tentativas de recriar banco falharam");
                                throw;
                            }
                        }
                    }
                }
                
                // Inicializa o SessaoService com o IPreferencesService
                var preferencesService = scope.ServiceProvider.GetRequiredService<IPreferencesService>();
                SessaoService.Initialize(preferencesService);
            }
        }

        /// <summary>
        /// Verifica se a exceção é de banco de dados corrompido
        /// </summary>
        private static bool IsCorruptedDatabaseError(Exception? ex)
        {
            if (ex == null)
                return false;
                
            var message = ex.Message ?? "";
            var innerMessage = ex.InnerException?.Message ?? "";
            
            return message.Contains("database disk image is malformed") ||
                   message.Contains("SQLite Error 11") ||
                   message.Contains("SQLITE_CORRUPT") ||
                   innerMessage.Contains("database disk image is malformed") ||
                   innerMessage.Contains("SQLite Error 11");
        }

        /// <summary>
        /// Remove arquivos corrompidos do SQLite (funciona em Windows e Linux)
        /// </summary>
        private static void CleanupCorruptedDatabase(string dbPath)
        {
            try
            {
                var dbDirectory = Path.GetDirectoryName(dbPath);
                
                if (string.IsNullOrEmpty(dbDirectory) || !Directory.Exists(dbDirectory))
                {
                    return;
                }

                // Arquivos de lock/cache do SQLite que podem estar corrompidos
                var filesToDelete = new[]
                {
                    dbPath,                          // Arquivo principal
                    dbPath + "-shm",                 // Shared memory
                    dbPath + "-wal"                  // Write-ahead log
                };

                foreach (var file in filesToDelete)
                {
                    try
                    {
                        if (File.Exists(file))
                        {
                            File.Delete(file);
                            System.Diagnostics.Debug.WriteLine($"Arquivo deletado: {file}");
                            
                            // Aguardar para garantir que o arquivo foi liberado
                            Thread.Sleep(100);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Erro ao deletar {file}: {ex.Message}");
                    }
                }
                
                System.Diagnostics.Debug.WriteLine("Limpeza de banco corrompido concluída");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro durante limpeza do banco: {ex.Message}");
            }
        }
        private async Task OnApplicationExitingAsync()
        {
            try
            {
                if (_backupOnCloseService != null)
                {
                    await _backupOnCloseService.PerformBackupWithConfirmationAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao processar backup ao fechar: {ex.Message}");
            }
        }
    }
