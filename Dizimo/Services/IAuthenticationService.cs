namespace Dizimo.Services;

/// <summary>
///     Serviço de autenticação que gerencia login, logout e navegação relacionada
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    ///     Realiza login do usuário
    /// </summary>
    /// <param name="login">Login/usuário</param>
    /// <param name="senha">Senha em texto plano</param>
    /// <returns>true se login bem-sucedido, false caso contrário</returns>
    Task<bool> PerformLoginAsync(string login, string senha);

    /// <summary>
    ///     Define o callback a ser executado após login bem-sucedido
    /// </summary>
    void SetOnLoginSuccess(Func<Task> onLoginSuccess);

    /// <summary>
    ///     Realiza logout e navega para a tela de login
    ///     Usa callbacks para notificar quando o logout está completo
    /// </summary>
    Task PerformLogoutAsync();

    /// <summary>
    ///     Define o callback a ser executado após logout bem-sucedido
    /// </summary>
    void SetOnLogoutComplete(Func<Task> onLogoutComplete);
}