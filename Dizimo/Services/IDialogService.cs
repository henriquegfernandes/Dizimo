namespace Dizimo.Services;

/// <summary>
/// Interface padrão para serviço de diálogos multi-plataforma
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Exibe um alerta com um único botão
    /// </summary>
    Task ShowAlertAsync(string title, string message, string buttonText = "OK");

    /// <summary>
    /// Exibe uma confirmação com dois botões
    /// </summary>
    /// <returns>true se o usuário clicou em aceitar, false se cancelou</returns>
    Task<bool> ShowConfirmAsync(string title, string message, string acceptText = "Sim", string cancelText = "Não");

    /// <summary>
    /// Exibe uma mensagem de erro
    /// </summary>
    Task ShowErrorAsync(string message);

    /// <summary>
    /// Exibe uma mensagem de sucesso
    /// </summary>
    Task ShowSuccessAsync(string message);

    /// <summary>
    /// Exibe um diálogo de informação
    /// </summary>
    Task ShowInfoAsync(string title, string message);

    /// <summary>
    /// Abre um diálogo para seleção de pasta
    /// </summary>
    /// <param name="title">Título do diálogo</param>
    /// <param name="initialPath">Caminho inicial (opcional)</param>
    /// <returns>Caminho da pasta selecionada ou null se cancelado</returns>
    Task<string?> ShowFolderPickerAsync(string title, string? initialPath = null);

    /// <summary>
    /// Abre um diálogo para seleção de arquivo
    /// </summary>
    /// <param name="title">Título do diálogo</param>
    /// <param name="initialPath">Caminho inicial (opcional)</param>
    /// <param name="filters">Filtros de arquivo (opcional)</param>
    /// <returns>Caminho do arquivo selecionado ou null se cancelado</returns>
    Task<string?> ShowFilePickerAsync(string title, string? initialPath = null, string[]? filters = null);
}
