using Microsoft.Maui.Controls;
using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;
using Dizimo.Infrastructure.Repositories;
using Dizimo.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

namespace Dizimo
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
        public IServiceProvider Services { get; }
        private readonly BackupOnCloseService? _backupOnCloseService;

        public App(IServiceProvider services)
        {
            Services = services;
            InitializeComponent();
            _backupOnCloseService = services.GetService<BackupOnCloseService>();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var appShell = new AppShell();
            var window = new BackupWindow(appShell, this);

            window.Created += async (s, e) =>
            {
                await NavigateToAppropriatePageAsync();
            };

            return window;
        }

        private async Task NavigateToAppropriatePageAsync()
        {
            using var scope = Services.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetService<IUnitOfWork>();
            if (unitOfWork != null)
            {
                var usuarios = await unitOfWork.Usuarios.GetAllAsync();
                if (!usuarios.Any())
                {
                    // Primeira execução - vai para setup
                    await Shell.Current.GoToAsync("setup");
                }
                else
                {
                    // Já tem usuários - vai para login
                    await Shell.Current.GoToAsync("login");
                }
            }
            else
            {
                // Fallback para login se não conseguir acessar unitOfWork
                await Shell.Current.GoToAsync("login");
            }
        }

        internal async Task OnWindowClosingAsync()
        {
            try
            {
                if (_backupOnCloseService != null)
                {
                    var mainPage = Windows.Count > 0 ? Windows[0].Page : null;
                    await _backupOnCloseService.PerformBackupAsync(mainPage);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao processar backup ao fechar: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Custom Window que intercepta o fechamento para realizar backup automaticamente
    /// </summary>
    public partial class BackupWindow : Window
    {
        private readonly App _app;
        private bool _isClosingHandled = false;

        public BackupWindow(Page page, App app) : base(page)
        {
            _app = app;
            this.Destroying += OnWindowDestroying;
        }

        private void OnWindowDestroying(object? sender, EventArgs e)
        {
            if (_isClosingHandled)
                return;

            _isClosingHandled = true;
            this.Destroying -= OnWindowDestroying;
            
            // Executar o backup de forma assíncrona
            Task.Run(async () =>
            {
                await _app.OnWindowClosingAsync();
            });
        }
    }
}