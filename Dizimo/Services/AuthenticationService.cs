using Dizimo.Infrastructure.Persistence;
using Dizimo.Application.Usuarios.Handlers;
using Dizimo.Application.Usuarios.Queries;

namespace Dizimo.Services;

/// <summary>
/// Implementação do serviço de autenticação
/// Gerencia login, logout, limpeza de sessão e navegação
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly DizimoDbContext _dbContext;
    private readonly GetUsuarioHandlers _getUsuarioHandlers;
    private Func<Task>? _onLoginSuccess;
    private Func<Task>? _onLogoutComplete;

    public AuthenticationService(DizimoDbContext dbContext, GetUsuarioHandlers getUsuarioHandlers)
    {
        _dbContext = dbContext;
        _getUsuarioHandlers = getUsuarioHandlers;
    }

    /// <summary>
    /// Realiza login completo: valida credenciais, inicia sessão e executa callback
    /// </summary>
    public async Task<bool> PerformLoginAsync(string login, string senha)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[AUTH] Login iniciado para usuário: {login}");

            // Busca o usuário no banco de dados
            var usuario = await _getUsuarioHandlers.Handle(new GetUsuarioByLoginQuery(login));
            
            // Valida: usuário existe, está ativo e senha está correta
            if (usuario != null && usuario.Ativo && usuario.SenhaHash == SessaoService.HashSenha(senha))
            {
                // Inicia a sessão
                SessaoService.Login(usuario.Id, usuario.Perfil, usuario.Nome);
                System.Diagnostics.Debug.WriteLine($"[AUTH] Login bem-sucedido: {usuario.Login} ({usuario.Perfil})");

                // Executa callback de sucesso se foi definido
                if (_onLoginSuccess != null)
                {
                    await _onLoginSuccess();
                    System.Diagnostics.Debug.WriteLine("[AUTH] Callback de sucesso de login executado");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[AVISO] Nenhum callback de login bem-sucedido configurado");
                }

                return true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[AUTH] Login falhou - Usuário inválido ou inativo: {login}");
                return false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Exceção ao realizar login: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Define o callback a ser executado após login bem-sucedido
    /// Deve ser configurado por AppRootViewModel no startup
    /// </summary>
    public void SetOnLoginSuccess(Func<Task> onLoginSuccess)
    {
        _onLoginSuccess = onLoginSuccess;
        System.Diagnostics.Debug.WriteLine("[AUTH] Callback de login bem-sucedido configurado");
    }

    /// <summary>
    /// Realiza logout completo: limpa sessão e executa callback
    /// </summary>
    public async Task PerformLogoutAsync()
    {
        try
        {
            // Limpa a sessão do usuário
            SessaoService.Logout();
            System.Diagnostics.Debug.WriteLine("[AUTH] Logout realizado - sessão limpa");

            // Executa callback se foi definido
            if (_onLogoutComplete != null)
            {
                await _onLogoutComplete();
                System.Diagnostics.Debug.WriteLine("[AUTH] Callback de logout executado");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[AVISO] Nenhum callback de logout configurado");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao realizar logout: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Define o callback a ser executado após logout bem-sucedido
    /// Deve ser configurado por AppRootViewModel no startup
    /// </summary>
    public void SetOnLogoutComplete(Func<Task> onLogoutComplete)
    {
        _onLogoutComplete = onLogoutComplete;
        System.Diagnostics.Debug.WriteLine("[AUTH] Callback de logout configurado");
    }
}

