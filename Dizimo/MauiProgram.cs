using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;
using Dizimo.Infrastructure.Persistence;
using Dizimo.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Dizimo.Domain.Repositories;
using Dizimo.ViewModels;
using Dizimo.Application.Dizimistas.Handlers;
using Dizimo.Infrastructure.Services;
using Dizimo.Application.Relatorios;
using Microsoft.Maui.LifecycleEvents;

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
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddSingleton<ProjectRepository>();
            builder.Services.AddSingleton<TaskRepository>();
            builder.Services.AddSingleton<CategoryRepository>();
            builder.Services.AddSingleton<TagRepository>();
            builder.Services.AddSingleton<SeedDataService>();
            builder.Services.AddSingleton<ModalErrorHandler>();
            builder.Services.AddSingleton<MainPageModel>();
            builder.Services.AddSingleton<ProjectListPageModel>();
            builder.Services.AddSingleton<ManageMetaPageModel>();
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<LocalBackupViewModel>();

            builder.ConfigureLifecycleEvents(events =>
            {
#if WINDOWS
                events.AddWindows(windows =>
                {
                    windows.OnStopped(async app =>
                    {
                        var backupVm = app.Services.GetService<LocalBackupViewModel>();
                        if (backupVm != null)
                        {
                            await backupVm.BackupAsync();
                        }
                    });
                });
#endif
            });

            builder.Services.AddTransientWithShellRoute<ProjectDetailPage, ProjectDetailPageModel>("project");
            builder.Services.AddTransientWithShellRoute<TaskDetailPage, TaskDetailPageModel>("task");

            // Registro dos Handlers e ViewModels para Dizimista
            builder.Services.AddScoped<GetDizimistaHandlers>();
            builder.Services.AddScoped<CreateDizimistaHandler>();
            builder.Services.AddScoped<UpdateDizimistaHandler>();
            builder.Services.AddScoped<DeleteDizimistaHandler>();
            builder.Services.AddScoped<InativarDizimistaHandler>();
            builder.Services.AddTransient<DizimistaListViewModel>();
            builder.Services.AddTransient<DizimistaCadastroViewModel>();

            // Registro das páginas para navegação
            builder.Services.AddTransientWithShellRoute<DizimistaListPage, DizimistaListViewModel>("dizimistas");
            builder.Services.AddTransientWithShellRoute<DizimistaCadastroPage, DizimistaCadastroViewModel>("dizimista-cadastro");

            builder.Services.AddScoped<DizimistaCsvService>();
            builder.Services.AddScoped<OfertaCsvService>();
            builder.Services.AddTransient<OfertaListViewModel>();
            builder.Services.AddTransient<OfertaCadastroViewModel>();
            builder.Services.AddTransientWithShellRoute<OfertaListPage, OfertaListViewModel>("ofertas");
            builder.Services.AddTransientWithShellRoute<OfertaCadastroPage, OfertaCadastroViewModel>("oferta-cadastro");

            builder.Services.AddScoped<UpdateOfertaHandler>();
            builder.Services.AddScoped<DeleteOfertaHandler>();
            builder.Services.AddScoped<GetOfertaHandlers>();
            builder.Services.AddScoped<RelatorioOfertasService>();
            builder.Services.AddScoped<RelatorioAniversariantesService>();

            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransientWithShellRoute<LoginPage, LoginViewModel>("login");

            builder.Services.AddTransient<UsuarioListViewModel>();
            builder.Services.AddTransientWithShellRoute<UsuarioListPage, UsuarioListViewModel>("usuarios");

            builder.Services.AddSingleton<SessaoService>();

            // Configuração do serviço de backup
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "dizimo.db");
            builder.Services.AddSingleton(new LocalBackupService(dbPath));
            builder.Services.AddSingleton<LocalBackupViewModel>();

            return builder.Build();
        }
    }
}
