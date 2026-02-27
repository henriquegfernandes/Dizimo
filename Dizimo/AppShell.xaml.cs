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
            Routing.RegisterRoute("setup", typeof(Dizimo.Pages.SetupPage));
            Routing.RegisterRoute("login", typeof(Dizimo.Pages.LoginPage));
            App? app = Microsoft.Maui.Controls.Application.Current as App ?? throw new InvalidOperationException("Application.Current não está inicializado ou não é do tipo App.");

            // Carrega o tema salvo anteriormente
            var savedTheme = ThemeService.GetSavedThemePreference();
            ThemeService.ApplyTheme(savedTheme);
            ThemeSegmentedControl.SelectedIndex = ThemeService.GetThemeIndex(savedTheme);

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
            CancellationTokenSource cancellationTokenSource = new();

            var snackbarOptions = new SnackbarOptions
            {
                BackgroundColor = Color.FromArgb("#FF0000"),
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
            var index = e.NewIndex ?? 0;
            var newTheme = ThemeService.GetThemeFromIndex(index);

            Microsoft.Maui.Controls.Application.Current?.UserAppTheme = newTheme;

            // Salva a preferência de tema
            ThemeService.SaveThemePreference(newTheme);
        }
    }
}
