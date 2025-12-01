// ============================================================================
// INTERFACE DO SERVIÇO DE API - CONTRATO PARA COMUNICAÇÃO COM BACKEND
// Define o contrato para o serviço de API e o modelo de resposta de tokens
// ============================================================================

using TTFWebsite.Models;

namespace TTFWebsite.Services
{
    /// <summary>
    /// Interface que define o contrato para o serviço de comunicação com a API.
    /// Permite desacoplamento e facilita testes unitários através de mocks.
    /// </summary>
    public interface IApiService
    {
        /// <summary>
        /// Autentica o utilizador e obtém tokens JWT.
        /// </summary>
        /// <param name="email">Email do utilizador</param>
        /// <param name="password">Palavra-passe do utilizador</param>
        /// <returns>Resposta com tokens ou null se falhar</returns>
        Task<TokenResponse?> LoginAsync(string email, string password);
        
        /// <summary>
        /// Termina a sessão do utilizador na API.
        /// </summary>
        /// <param name="token">Token de acesso a invalidar</param>
        /// <returns>True se bem-sucedido</returns>
        Task<bool> LogoutAsync(string token);
        
        /// <summary>
        /// Renova o token de acesso usando o refresh token.
        /// </summary>
        /// <param name="refreshToken">Token de atualização</param>
        /// <returns>Novos tokens ou null se falhar</returns>
        Task<TokenResponse?> RefreshTokenAsync(string refreshToken);
        
        /// <summary>
        /// Altera a palavra-passe do utilizador autenticado.
        /// </summary>
        /// <param name="token">Token de acesso</param>
        /// <param name="currentPassword">Palavra-passe atual</param>
        /// <param name="newPassword">Nova palavra-passe</param>
        /// <returns>True se bem-sucedido</returns>
        Task<bool> ChangePasswordAsync(string token, string currentPassword, string newPassword);
        
        /// <summary>
        /// Inicia o processo de recuperação de palavra-passe.
        /// </summary>
        /// <param name="email">Email do utilizador</param>
        /// <returns>True se o pedido foi enviado</returns>
        Task<bool> ResetPasswordAsync(string email);
    }

    /// <summary>
    /// Modelo de resposta de autenticação da API.
    /// Contém os tokens JWT e informações adicionais.
    /// </summary>
    public class TokenResponse
    {
        /// <summary>
        /// Token de acesso JWT para autenticar pedidos à API.
        /// Tem validade curta (tipicamente 15-60 minutos).
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;
        
        /// <summary>
        /// Token de atualização para renovar o access token.
        /// Tem validade mais longa (tipicamente dias ou semanas).
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
        
        /// <summary>
        /// Indica se o utilizador precisa de alterar a palavra-passe.
        /// Tipicamente true no primeiro login após criação de conta.
        /// </summary>
        public bool NeedsPasswordChange { get; set; }
        
        /// <summary>
        /// Mensagem adicional da API (ex: instruções para alterar palavra-passe).
        /// </summary>
        public string? Message { get; set; }
    }
}
