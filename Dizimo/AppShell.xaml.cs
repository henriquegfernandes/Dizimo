using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Dizimo.ViewModels;
using Font = Microsoft.Maui.Font;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace Dizimo
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("login", typeof(Dizimo.Pages.LoginPage));
            var app = Microsoft.Maui.Controls.Application.Current as App;
            if (app is null)
                throw new InvalidOperationException("Application.Current não está inicializado ou não é do tipo App.");

            // Força modo claro ao iniciar
            if (Microsoft.Maui.Controls.Application.Current != null)
            {
                Microsoft.Maui.Controls.Application.Current.UserAppTheme = AppTheme.Light;
            }

            var currentTheme = Microsoft.Maui.Controls.Application.Current?.RequestedTheme ?? AppTheme.Light;
            ThemeSegmentedControl.SelectedIndex = 0; // Sempre inicia no claro
            var mainVm = app.Services.GetService<MainViewModel>();
            var backupVm = app.Services.GetService<LocalBackupViewModel>();
            if (mainVm is null)
                throw new InvalidOperationException("MainViewModel não foi registrado ou está nulo.");
            if (backupVm is null)
                throw new InvalidOperationException("LocalBackupViewModel não foi registrado ou está nulo.");
            BindingContext = new ShellViewModel(mainVm, backupVm);
        }
        public static async Task DisplaySnackbarAsync(string message)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            var snackbarOptions = new SnackbarOptions
            {
                BackgroundColor = Color.FromArgb("#FF3300"),
                TextColor = Colors.White,
                ActionButtonTextColor = Colors.Yellow,
                CornerRadius = new CornerRadius(0),
                Font = Font.SystemFontOfSize(18),
                ActionButtonFont = Font.SystemFontOfSize(14)
            };

            var snackbar = Snackbar.Make(message, visualOptions: snackbarOptions);

            await snackbar.Show(cancellationTokenSource.Token);
        }

        public static async Task DisplayToastAsync(string message)
        {
            // Toast is currently not working in MCT on Windows
            if (OperatingSystem.IsWindows())
                return;

            var toast = Toast.Make(message, textSize: 18);

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await toast.Show(cts.Token);
        }

        private void SfSegmentedControl_SelectionChanged(object? sender, Syncfusion.Maui.Toolkit.SegmentedControl.SelectionChangedEventArgs e)
        {
            if (Microsoft.Maui.Controls.Application.Current?.UserAppTheme != null)
                Microsoft.Maui.Controls.Application.Current.UserAppTheme = e.NewIndex == 0 ? AppTheme.Light : AppTheme.Dark;
        }
    }
}
