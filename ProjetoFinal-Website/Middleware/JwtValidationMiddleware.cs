using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using TTFWebsite.Services;

public class JwtValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly JwtService _jwtService;
    private readonly ILogger<JwtValidationMiddleware> _logger;

    public JwtValidationMiddleware(
        RequestDelegate next,
        JwtService jwtService,
        ILogger<JwtValidationMiddleware> logger)
    {
        _next = next;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var jwt = context.User.FindFirst("jwt")?.Value;

                if (string.IsNullOrEmpty(jwt))
                {
                    _logger.LogInformation("JWT não encontrado. Forçando logout.");
                    await ForceLogout(context);
                    return;
                }

                if (_jwtService.IsTokenValid(jwt))
                {
                    await _next(context);
                    return;
                }

                _logger.LogInformation("JWT expirado. Tentando refresh...");

                var refreshToken = context.Session.GetString("RefreshToken");

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var api = context.RequestServices.GetRequiredService<IApiService>();
                    var newTokens = await api.RefreshTokenAsync(refreshToken);

                    if (newTokens != null &&
                        !string.IsNullOrEmpty(newTokens.AccessToken) &&
                        !string.IsNullOrEmpty(newTokens.RefreshToken))
                    {
                        _logger.LogInformation("Refresh token válido. Atualizando sessão.");

                        // Atualiza JWT e refresh token
                        await ReSignInWithNewJwt(context, newTokens.AccessToken);
                        context.Session.SetString("RefreshToken", newTokens.RefreshToken);
                        
                        // Atualiza NeedsPasswordChange se o novo token indicar isso
                        if (newTokens.NeedsPasswordChange)
                        {
                            context.Session.SetString("NeedsPasswordChange", "True");
                        }
                        // Se não precisa mais alterar, remove da sessão (caso tenha sido alterado)
                        else
                        {
                            context.Session.Remove("NeedsPasswordChange");
                        }

                        await _next(context);
                        return;
                    }
                    else
                    {
                        _logger.LogWarning("Refresh falhou ou inválido.");
                    }
                }
                else
                {
                    _logger.LogWarning("Nenhum refresh token disponível.");
                }

                // Se chegou aqui, refresh falhou
                await ForceLogout(context);
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no middleware JWT. Forçando logout.");
            await ForceLogout(context);
            return;
        }

        // Caso não autenticado, apenas continua
        await _next(context);
    }

    private async Task ReSignInWithNewJwt(HttpContext context, string newJwt)
    {
        // Mantém todas as claims originais exceto a antiga JWT e re-assina o cookie com o novo token.
        var claims = context.User.Claims
            .Where(c => c.Type != "jwt")
            .ToList();

        claims.Add(new Claim("jwt", newJwt));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = true
            });
    }

    private async Task ForceLogout(HttpContext context)
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        context.Session.Clear();

        if (IsAjaxRequest(context))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
        else
        {
            // Evita loops de redirect: só redireciona se não estivermos já na página pública
            var path = context.Request.Path.ToString().ToLower();
            if (!path.Contains("/home/index") && !path.Contains("/home"))
            {
                // Quando o membro perde JWT/refresh tokens, volta ao site público (Home/Index).
                context.Response.Redirect("/Home/Index");
            }
        }
    }

    private bool IsAjaxRequest(HttpContext context)
    {
        return context.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
    }
}
