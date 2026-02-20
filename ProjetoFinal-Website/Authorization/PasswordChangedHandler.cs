using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace TTFWebsite.Authorization
{
    public class PasswordChangedHandler : AuthorizationHandler<PasswordChangedRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PasswordChangedHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PasswordChangedRequirement requirement)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            // Verifica se NeedsPasswordChange está na sessão
            var needsPasswordChange = httpContext.Session.GetString("NeedsPasswordChange");
            
            // Se NeedsPasswordChange=true (ou seja, PrimeiraVez=true), bloqueia acesso
            if (needsPasswordChange == "True")
            {
                context.Fail();
            }
            else
            {
                // Se não precisa alterar password ou não está definido, permite acesso
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
