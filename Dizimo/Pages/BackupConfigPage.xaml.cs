using Dizimo.ViewModels;

namespace Dizimo.Pages
{
    public partial class BackupConfigPage : ContentPage
    {
        public BackupConfigPage(LocalBackupViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}
