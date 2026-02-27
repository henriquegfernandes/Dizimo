using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;
using Dizimo.Infrastructure.Persistence;
using Dizimo.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Dizimo.Domain.Repositories;
using Dizimo.ViewModels;
using Dizimo.Application.Dizimistas.Handlers;
using Dizimo.Application.Ofertas.Handlers;
using Dizimo.Application.Dashboard;
using Dizimo.Application.Usuarios.Handlers;
using Dizimo.Services;
using System.Globalization;
using Dizimo.Resources.Fonts;

namespace Dizimo
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            // Configurar cultura global para PT-BR
            CultureInfo ptBr = new("pt-BR");
            CultureInfo.DefaultThreadCurrentCulture = ptBr;
            CultureInfo.DefaultThreadCurrentUICulture = ptBr;

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionToolkit()
                .ConfigureMauiHandlers(handlers =>
                {
#if WINDOWS
    				Microsoft.Maui.Controls.Handlers.Items.CollectionViewHandler.Mapper.AppendToMapping("KeyboardAccessibleCollectionView", (handler, view) =>
    				{
    					handler.PlatformView.SingleSelectionFollowsFocus = false;
    				});
#endif
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
                });

#if DEBUG
    		builder.Logging.AddDebug();
    		builder.Services.AddLogging(configure => configure.AddDebug());
#endif

            // Configuração do serviço de caminhos
            builder.Services.AddSingleton<IDataPathProvider, DataPathProvider>();

            // Configuração do EF Core com SQLite
            var dataPathProvider = new DataPathProvider();
            var dbPath = dataPathProvider.GetDatabasePath();
            builder.Services.AddDbContext<DizimoDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}")
            );
            // Repositórios e UoW devem ser Scoped para compartilhar o contexto
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            builder.Services.AddSingleton<ModalErrorHandler>();
            builder.Services.AddTransient<AppShell>();

            // ViewModels
            builder.Services.AddTransient<MainPageViewModel>();
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<LocalBackupViewModel>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<SetupViewModel>();

            // Dashboard Service
            builder.Services.AddScoped<DashboardService>();

            // Registro dos Handlers e ViewModels para Dizimistas
            builder.Services.AddScoped<GetDizimistaHandlers>();
            builder.Services.AddScoped<CreateDizimistaHandler>();
            builder.Services.AddScoped<UpdateDizimistaHandler>();
            builder.Services.AddScoped<DeleteDizimistaHandler>();
            builder.Services.AddScoped<InativarDizimistaHandler>();
            builder.Services.AddScoped<GetUsuarioHandlers>();
            builder.Services.AddScoped<DizimistaExcelService>();
            builder.Services.AddScoped<DizimistaPdfService>();
            builder.Services.AddScoped<AniversariantesExcelService>();
            builder.Services.AddScoped<AniversariantesPdfService>();
            builder.Services.AddTransient<DizimistaListViewModel>();
            builder.Services.AddTransient<DizimistaCadastroViewModel>();
            builder.Services.AddTransient<DizimistaDetalhesViewModel>();
            builder.Services.AddTransientWithShellRoute<DizimistaDetalhesPage, DizimistaDetalhesViewModel>("dizimista-detalhes");
            builder.Services.AddTransientWithShellRoute<DizimistaListPage, DizimistaListViewModel>("dizimistas");
            builder.Services.AddTransientWithShellRoute<DizimistaCadastroPage, DizimistaCadastroViewModel>("dizimista-cadastro");

            // Registro dos Handlers e ViewModels para Ofertas
            builder.Services.AddScoped<OfertaExcelService>();
            builder.Services.AddScoped<OfertaPdfService>();
            builder.Services.AddTransient<OfertaListViewModel>();
            builder.Services.AddTransient<OfertaCadastroViewModel>();
            builder.Services.AddTransientWithShellRoute<OfertaListPage, OfertaListViewModel>("ofertas");
            builder.Services.AddTransientWithShellRoute<OfertaCadastroPage, OfertaCadastroViewModel>("oferta-cadastro");
            builder.Services.AddScoped<CreateOfertaHandler>();
            builder.Services.AddScoped<UpdateOfertaHandler>();
            builder.Services.AddScoped<DeleteOfertaHandler>();
            builder.Services.AddScoped<GetOfertaHandlers>();

            // Registro dos Handlers e ViewModels para Usuários
            builder.Services.AddScoped<CreateUsuarioHandler>();
            builder.Services.AddScoped<UpdateUsuarioHandler>();
            builder.Services.AddScoped<DeleteUsuarioHandler>();
            builder.Services.AddScoped<InativarUsuarioHandler>();
            builder.Services.AddTransient<UsuarioListViewModel>();
            builder.Services.AddTransient<UsuarioCadastroViewModel>();
            builder.Services.AddTransientWithShellRoute<UsuarioListPage, UsuarioListViewModel>("usuarios");
            builder.Services.AddTransientWithShellRoute<UsuarioCadastroPage, UsuarioCadastroViewModel>("usuario-cadastro");

            builder.Services.AddTransientWithShellRoute<LoginPage, LoginViewModel>("login");

            builder.Services.AddSingleton<SessaoService>();

            // Configuração do serviço de backup
            builder.Services.AddSingleton(sp => new LocalBackupService(
                dbPath, 
                sp
            ));
            builder.Services.AddSingleton<BackupOnCloseService>();

            var app = builder.Build();

            // Aplicar migrações automaticamente ao iniciar
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DizimoDbContext>();
                db.Database.Migrate();
            }

            return app;
        }
    }
}
