// ============================================================================
// SERVIÇO JWT - DESCODIFICAÇÃO DE TOKENS JWT
// Este serviço trata da descodificação de tokens JWT para
// extrair as claims (informações) do utilizador
// ============================================================================

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TTFWebsite.Services
{
    /// <summary>
    /// Serviço para manipulação de tokens JWT (JSON Web Tokens).
    /// Permite descodificar tokens para extrair as claims do utilizador
    /// sem validar a assinatura (a validação é feita pela API).
    /// </summary>
    public class JwtService
    {
        /// <summary>
        /// Descodifica um token JWT e extrai as claims como ClaimsPrincipal.
        /// NOTA: Este método não valida a assinatura do token.
        /// A validação é responsabilidade da API que emite o token.
        /// </summary>
        /// <param name="token">Token JWT em formato string</param>
        /// <returns>ClaimsPrincipal com as claims do token ou null se inválido</returns>
        public ClaimsPrincipal? DecodeToken(string token)
        {
            try
            {
                // Criar handler para processar tokens JWT
                var handler = new JwtSecurityTokenHandler();
                
                // Ler e descodificar o token (sem validar assinatura)
                var jsonToken = handler.ReadJwtToken(token);

                // Extrair todas as claims do token
                // Claims típicas: sub (id), email, name, role, exp, etc.
                var claims = jsonToken.Claims.ToList();
                
                // Criar identidade com as claims extraídas
                // O tipo de autenticação "JWT" identifica a origem das claims
                var claimsIdentity = new ClaimsIdentity(claims, "JWT");
                
                // Retornar o principal com a identidade
                return new ClaimsPrincipal(claimsIdentity);
            }
            catch
            {
                // Se ocorrer qualquer erro na descodificação, retornar null
                // Isto pode acontecer se o token estiver malformado
                return null;
            }
        }
    }
}
