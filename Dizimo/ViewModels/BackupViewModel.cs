using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Infrastructure.Backup.Services;
using Dizimo.Services;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using AppLifetime = Avalonia.Controls.ApplicationLifetimes.ClassicDesktopStyleApplicationLifetime;

namespace Dizimo.ViewModels;

public partial class BackupViewModel : ObservableObject
{
    private readonly LocalBackupService _localBackupService;
    private readonly IDialogService _dialogService;

    public BackupViewModel(LocalBackupService localBackupService, IDialogService dialogService)
    {
        _localBackupService = localBackupService ?? throw new ArgumentNullException(nameof(localBackupService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
    }

    [RelayCommand]
    public async Task BackupAsync()
    {
        try
        {
            await _localBackupService.BackupAsync();
            await _dialogService.ShowSuccessAsync("Backup realizado com sucesso!");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Erro ao fazer backup: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task RestoreAsync()
    {
        try
        {
            // Abrir file picker para o usuário selecionar o arquivo de backup
            if (global::Avalonia.Application.Current?.ApplicationLifetime is AppLifetime desktop && desktop.MainWindow != null)
            {
                var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
                if (topLevel?.StorageProvider is null)
                {
                    await _dialogService.ShowErrorAsync("Não foi possível acessar o gerenciador de arquivos do sistema");
                    return;
                }

                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new()
                {
                    Title = "Selecionar Arquivo de Backup para Restaurar",
                    AllowMultiple = false,
                    FileTypeFilter = new[] { new FilePickerFileType("Arquivo SQLite") { Patterns = new[] { "*.db" } } }
                });

                if (files.Count == 0)
                    return;

                var selectedFile = files[0];
                var backupFilePath = selectedFile.Path.LocalPath;

                // Confirmar antes de restaurar (será destrutivo)
                bool confirm = await _dialogService.ShowConfirmAsync(
                    "Confirmar Restauração",
                    $"Tem certeza que deseja restaurar o backup de:\n{Path.GetFileName(backupFilePath)}\n\n" +
                    "Todos os dados atuais serão substituídos! O aplicativo será encerrado e reiniciado automaticamente.",
                    "Sim, Restaurar", "Cancelar");

                if (!confirm)
                    return;

                // Restaurar o banco
                System.Diagnostics.Debug.WriteLine("[INFO] Iniciando restauração do banco...");
                await _localBackupService.RestoreFromFileAsync(backupFilePath);
                System.Diagnostics.Debug.WriteLine("[INFO] Banco restaurado com sucesso");

                // Aguardar para garantir limpeza completa
                await Task.Delay(1000);

                await _dialogService.ShowSuccessAsync("Restauração realizada com sucesso! O aplicativo será encerrado e reiniciado.");
                System.Diagnostics.Debug.WriteLine("[INFO] Dialogo de sucesso exibido");

                // Aguardar mais um pouco
                await Task.Delay(500);

                // Reiniciar a aplicação completamente
                System.Diagnostics.Debug.WriteLine("[INFO] Encerrando aplicação para reiniciar...");
                
                // Limpar a sessão do usuário ANTES de sair
                SessaoService.Logout();
                System.Diagnostics.Debug.WriteLine("[AUTH] Sessão do usuário limpa");
                
                var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
                var exePath = currentProcess.MainModule?.FileName;
                
                if (!string.IsNullOrEmpty(exePath))
                {
                    // Iniciar nova instância
                    System.Diagnostics.Process.Start(exePath);
                    System.Diagnostics.Debug.WriteLine($"[INFO] Nova instância iniciada: {exePath}");
                }
                
                // Encerrar aplicação atual
                System.Diagnostics.Debug.WriteLine("[INFO] Encerrando aplicação atual...");
                Environment.Exit(0);
            }
            else
            {
                await _dialogService.ShowErrorAsync("Não foi possível acessar a janela principal");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao restaurar: {ex.Message}");
            await _dialogService.ShowErrorAsync($"Erro ao restaurar: {ex.Message}");
        }
    }
}
