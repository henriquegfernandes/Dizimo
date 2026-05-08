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
#endif
            var serviceProvider = services.BuildServiceProvider();
            using (var scope = serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DizimoDbContext>();
                db.Database.Migrate();
                
                // Inicializa o SessaoService com o IPreferencesService
                var preferencesService = scope.ServiceProvider.GetRequiredService<IPreferencesService>();
                SessaoService.Initialize(preferencesService);
            }
            Ioc.Default.ConfigureServices(serviceProvider);
            return serviceProvider;
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
