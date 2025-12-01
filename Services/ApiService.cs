// ============================================================================
// SERVIÇO DE API - COMUNICAÇÃO COM O BACKEND FITCONTROL
// Este serviço gere todas as chamadas HTTP à API REST do backend
// para autenticação e gestão de utilizadores
// ============================================================================

using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using TTFWebsite.Models;

namespace TTFWebsite.Services
{
    /// <summary>
    /// Implementação do serviço de comunicação com a API do backend.
    /// Trata de todas as operações de autenticação: login, logout,
    /// renovação de tokens e alteração de palavra-passe.
    /// </summary>
    public class ApiService : IApiService
    {
        // Cliente HTTP para fazer pedidos à API
        private readonly HttpClient _httpClient;
        // Configurações da aplicação (para obter URL da API)
        private readonly IConfiguration _configuration;
        // Sistema de registo de logs para diagnóstico
        private readonly ILogger<ApiService> _logger;

        /// <summary>
        /// Construtor do serviço com injeção de dependências.
        /// Configura o cliente HTTP com o URL base da API.
        /// </summary>
        /// <param name="httpClient">Cliente HTTP injetado</param>
        /// <param name="configuration">Configurações da aplicação</param>
        /// <param name="logger">Sistema de logs</param>
        public ApiService(HttpClient httpClient, IConfiguration configuration, ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            // Obter o URL base da API a partir das configurações
            // Usa localhost:5295 como valor por defeito
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5295";
            _httpClient.BaseAddress = new Uri(baseUrl);
            // Configurar cabeçalho para aceitar respostas JSON
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        /// <summary>
        /// Realiza a autenticação do utilizador através da API.
        /// </summary>
        /// <param name="email">Email do utilizador</param>
        /// <param name="password">Palavra-passe do utilizador</param>
        /// <returns>Resposta com tokens JWT ou null se falhar</returns>
        public async Task<TokenResponse?> LoginAsync(string email, string password)
        {
            try
            {
                // Criar objeto com as credenciais para enviar à API
                var loginDto = new { Email = email, Password = password };
                // Fazer pedido POST para o endpoint de login
                var response = await _httpClient.PostAsJsonAsync("/api/Auth/login", loginDto);

                if (response.IsSuccessStatusCode)
                {
                    // Desserializar a resposta para obter os tokens
                    var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
                    return tokenResponse;
                }
                else
                {
                    // Registar falha no login para diagnóstico
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Login failed: {StatusCode} - {Content}", response.StatusCode, errorContent);
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Registar erro de exceção
                _logger.LogError(ex, "Error during login");
                return null;
            }
        }

        /// <summary>
        /// Termina a sessão do utilizador na API.
        /// Invalida o token de acesso no servidor.
        /// </summary>
        /// <param name="token">Token de acesso atual</param>
        /// <returns>True se o logout foi bem-sucedido</returns>
        public async Task<bool> LogoutAsync(string token)
        {
            try
            {
                // Criar pedido com o cabeçalho de autorização Bearer
                var request = new HttpRequestMessage(HttpMethod.Post, "/api/Auth/logout");
                request.Headers.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Enviar pedido de logout
                var response = await _httpClient.SendAsync(request);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return false;
            }
        }

        /// <summary>
        /// Renova o token de acesso usando o token de atualização.
        /// Utilizado quando o token de acesso expira.
        /// </summary>
        /// <param name="refreshToken">Token de atualização válido</param>
        /// <returns>Novos tokens ou null se falhar</returns>
        public async Task<TokenResponse?> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                // Criar objeto com o refresh token
                var refreshDto = new { RefreshToken = refreshToken };
                // Fazer pedido POST para renovar o token
                var response = await _httpClient.PostAsJsonAsync("/api/Auth/refresh", refreshDto);

                if (response.IsSuccessStatusCode)
                {
                    // Retornar os novos tokens
                    return await response.Content.ReadFromJsonAsync<TokenResponse>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return null;
            }
        }

        /// <summary>
        /// Altera a palavra-passe do utilizador.
        /// Requer a palavra-passe atual para verificação.
        /// </summary>
        /// <param name="token">Token de acesso do utilizador</param>
        /// <param name="currentPassword">Palavra-passe atual</param>
        /// <param name="newPassword">Nova palavra-passe</param>
        /// <returns>True se a alteração foi bem-sucedida</returns>
        public async Task<bool> ChangePasswordAsync(string token, string currentPassword, string newPassword)
        {
            try
            {
                // NOTA: O backend espera os nomes PasswordAtual e NovaPassword
                var changePasswordDto = new { PasswordAtual = currentPassword, NovaPassword = newPassword };
                
                // Criar pedido com autenticação Bearer
                var request = new HttpRequestMessage(HttpMethod.Post, "/api/Auth/change-password");
                request.Headers.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                // Adicionar corpo do pedido em JSON
                request.Content = JsonContent.Create(changePasswordDto);

                // Enviar pedido
                var response = await _httpClient.SendAsync(request);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password change");
                return false;
            }
        }

        /// <summary>
        /// Solicita a recuperação de palavra-passe.
        /// Envia um email com instruções para redefinir a palavra-passe.
        /// </summary>
        /// <param name="email">Email do utilizador</param>
        /// <returns>True se o pedido foi enviado com sucesso</returns>
        public async Task<bool> ResetPasswordAsync(string email)
        {
            try
            {
                // Criar objeto com o email
                var resetDto = new { Email = email };
                // Fazer pedido POST para iniciar recuperação
                var response = await _httpClient.PostAsJsonAsync("/api/Auth/reset-password", resetDto);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                return false;
            }
        }
    }
}
