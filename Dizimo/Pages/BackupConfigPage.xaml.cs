using Dizimo.ViewModels;
using System.Runtime.Versioning;

namespace Dizimo.Pages
{
    public partial class BackupConfigPage : ContentPage
    {
        [SupportedOSPlatform("windows10.0.17763.0")]
        public BackupConfigPage(LocalBackupViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}
