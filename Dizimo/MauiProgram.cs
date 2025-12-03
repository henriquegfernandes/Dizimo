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
using Dizimo.Application.Relatorios;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Controls;
using Dizimo.Pages;
using Microsoft.Extensions.DependencyInjection;
using Dizimo.Application.Usuarios.Handlers;

namespace Dizimo
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
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

    				Microsoft.Maui.Handlers.ContentViewHandler.Mapper.AppendToMapping(nameof(Pages.Controls.CategoryChart), (handler, view) =>
    				{
    					if (view is Pages.Controls.CategoryChart && handler.PlatformView is Microsoft.Maui.Platform.ContentPanel contentPanel)
    					{
    						contentPanel.IsTabStop = true;
    					}
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

            // Configuração do EF Core com SQLite
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "dizimo.db");
            builder.Services.AddDbContext<DizimoDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}")
            );
            // Repositórios e UoW devem ser Scoped para compartilhar o contexto
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            // Remover Singletons relacionados a dados
            builder.Services.AddScoped<ProjectRepository>();
            builder.Services.AddScoped<TaskRepository>();
            builder.Services.AddScoped<CategoryRepository>();
            builder.Services.AddScoped<TagRepository>();
            builder.Services.AddScoped<SeedDataService>();
            builder.Services.AddSingleton<ModalErrorHandler>();
            builder.Services.AddTransient<MainPageModel>();
            builder.Services.AddTransient<ProjectListPageModel>();
            builder.Services.AddTransient<ManageMetaPageModel>();
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<LocalBackupViewModel>();
            builder.Services.AddTransient<AppShell>();
            builder.Services.AddTransient<DizimistaListPage>();
            builder.Services.AddTransient<UsuarioListPage>();
            builder.Services.AddTransient<OfertaListPage>();
            

            builder.ConfigureLifecycleEvents(events =>
            {
//#if WINDOWS
//                events.AddWindows(windows =>
//                {
//                    windows.OnClosed(() =>
//                    {
//                        var serviceProvider = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services;
//                        if (serviceProvider is not null)
//                        {
//                            var backupVm = serviceProvider.GetService<LocalBackupViewModel>();
//                            if (backupVm is not null)
//                            {
//                                // Executa o backup de forma síncrona, pois OnClosed não suporta async
//                                backupVm.BackupAsync().GetAwaiter().GetResult();
//                            }
//                        }
//                    });
//                });
//#endif
            });

            builder.Services.AddTransientWithShellRoute<ProjectDetailPage, ProjectDetailPageModel>("project");
            builder.Services.AddTransientWithShellRoute<TaskDetailPage, TaskDetailPageModel>("task");

            // Registro dos Handlers e ViewModels para Dizimista
            builder.Services.AddScoped<GetDizimistaHandlers>();
            builder.Services.AddScoped<CreateDizimistaHandler>();
            builder.Services.AddScoped<UpdateDizimistaHandler>();
            builder.Services.AddScoped<DeleteDizimistaHandler>();
            builder.Services.AddScoped<InativarDizimistaHandler>();
            builder.Services.AddScoped<GetUsuarioHandlers>();
            builder.Services.AddTransient<DizimistaListViewModel>();
            builder.Services.AddTransient<DizimistaCadastroViewModel>();
            builder.Services.AddScoped<DizimistaDetalhesViewModel>();
            builder.Services.AddTransientWithShellRoute<DizimistaDetalhesPage, DizimistaDetalhesViewModel>("dizimista-detalhes");

            // Registro das páginas para navegação
            builder.Services.AddTransientWithShellRoute<DizimistaListPage, DizimistaListViewModel>("dizimistas");
            builder.Services.AddTransientWithShellRoute<DizimistaCadastroPage, DizimistaCadastroViewModel>("dizimista-cadastro");

            builder.Services.AddScoped<DizimistaCsvService>();
            builder.Services.AddScoped<OfertaCsvService>();
            builder.Services.AddTransient<OfertaListViewModel>();
            builder.Services.AddTransient<OfertaCadastroViewModel>();
            builder.Services.AddTransientWithShellRoute<OfertaListPage, OfertaListViewModel>("ofertas");
            builder.Services.AddTransientWithShellRoute<OfertaCadastroPage, OfertaCadastroViewModel>("oferta-cadastro");

            builder.Services.AddScoped<CreateOfertaHandler>();
            builder.Services.AddScoped<UpdateOfertaHandler>();
            builder.Services.AddScoped<DeleteOfertaHandler>();
            builder.Services.AddScoped<GetOfertaHandlers>();
            builder.Services.AddScoped<RelatorioOfertasService>();
            builder.Services.AddScoped<RelatorioAniversariantesService>();

            builder.Services.AddScoped<GetUsuarioHandlers>();
            builder.Services.AddScoped<CreateUsuarioHandler>();
            builder.Services.AddScoped<UpdateUsuarioHandler>();
            builder.Services.AddScoped<DeleteUsuarioHandler>();
            builder.Services.AddScoped<InativarUsuarioHandler>();

            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransientWithShellRoute<LoginPage, LoginViewModel>("login");

            builder.Services.AddTransient<UsuarioListViewModel>();
            builder.Services.AddTransientWithShellRoute<UsuarioListPage, UsuarioListViewModel>("usuarios");

            builder.Services.AddSingleton<SessaoService>();

            // Configuração do serviço de backup
            builder.Services.AddSingleton(new LocalBackupService(dbPath));

            builder.ConfigureFonts(fonts =>
            {
                fonts.AddFont("Font Awesome 6 Pro-Regular-400.otf", "FontAwesome");
            });

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
