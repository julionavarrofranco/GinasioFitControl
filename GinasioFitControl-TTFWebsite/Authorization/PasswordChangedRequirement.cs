using Microsoft.AspNetCore.Authorization;

namespace TTFWebsite.Authorization
{
    /// <summary>
    /// Requisito de autorização que verifica se o utilizador já alterou a password inicial.
    /// Bloqueia acesso a rotas protegidas se PrimeiraVez=true (pwd_changed=false no JWT).
    /// </summary>
    public class PasswordChangedRequirement : IAuthorizationRequirement
    {
    }
}
