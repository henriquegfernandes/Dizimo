namespace Dizimo.Services;

using System.Diagnostics;

/// <summary>
/// Implementação de debug para IDialogService
/// Use AvaloniaDialogService em produção para diálogos reais
/// </summary>
public class DialogService : IDialogService
{
    public Task ShowAlertAsync(string title, string message, string buttonText = "OK")
    {
        Debug.WriteLine($"[DIALOG] {title}: {message} [{buttonText}]");
        return Task.CompletedTask;
    }

    public Task<bool> ShowConfirmAsync(string title, string message, string acceptText = "Sim", string cancelText = "Não")
    {
        Debug.WriteLine($"[DIALOG] {title}: {message} [{acceptText}/{cancelText}]");
        return Task.FromResult(false);
    }

    public Task ShowErrorAsync(string message)
    {
        Debug.WriteLine($"[DIALOG] ❌ Erro: {message}");
        return Task.CompletedTask;
    }

    public Task ShowSuccessAsync(string message)
    {
        Debug.WriteLine($"[DIALOG] ✓ Sucesso: {message}");
        return Task.CompletedTask;
    }

    public Task ShowInfoAsync(string title, string message)
    {
        Debug.WriteLine($"[DIALOG] ℹ️ {title}: {message}");
        return Task.CompletedTask;
    }

    public Task<string?> ShowFolderPickerAsync(string title, string? initialPath = null)
    {
        Debug.WriteLine($"[DIALOG] Folder Picker: {title} (initialPath: {initialPath})");
        return Task.FromResult<string?>(null);
    }

    public Task<string?> ShowFilePickerAsync(string title, string? initialPath = null, string[]? filters = null)
    {
        Debug.WriteLine($"[DIALOG] File Picker: {title} (initialPath: {initialPath}, filters: {string.Join(",", filters ?? Array.Empty<string>())})");
        return Task.FromResult<string?>(null);
    }
}

