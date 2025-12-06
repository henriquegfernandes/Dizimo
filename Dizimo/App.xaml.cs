using Microsoft.Maui.Controls;
using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;
using Dizimo.Infrastructure.Repositories;
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

        public App(IServiceProvider services)
        {
            Services = services;
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());
            window.Created += async (s, e) =>
            {
                await EnsureDefaultAdminUserAsync();
                GoToLoginPage();
            };
            return window;
        }

        private async Task EnsureDefaultAdminUserAsync()
        {
            using var scope = Services.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetService<IUnitOfWork>();
            if (unitOfWork != null)
            {
                var usuarios = await unitOfWork.Usuarios.GetAllAsync();
                if (!usuarios.Any())
                {
                    var admin = new Usuario
                    {
                        Id = Guid.NewGuid(),
                        Nome = "Administrador",
                        Login = "admin",
                        SenhaHash = HashSenhaBase64("password"),
                        Perfil = PerfilUsuario.Admin,
                        Ativo = true
                    };
                    await unitOfWork.Usuarios.AddAsync(admin);
                    await unitOfWork.SaveChangesAsync();
                }
            }
        }

        private static string HashSenhaBase64(string senha)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(senha));
            return Convert.ToBase64String(bytes);
        }

        private void GoToLoginPage()
        {
            Shell.Current.GoToAsync("login");
        }
    }
}