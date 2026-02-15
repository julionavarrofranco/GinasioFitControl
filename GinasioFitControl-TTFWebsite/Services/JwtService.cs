using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace TTFWebsite.Services
{
    public class JwtService
    {
        private readonly string _jwtSecret;

        public JwtService(IConfiguration configuration)
        {
            // Pega a chave do user secrets / appsettings
            _jwtSecret = configuration["Jwt:Key"]
                         ?? throw new InvalidOperationException("JWT Key não encontrada nas configurações.");
        }


        public ClaimsPrincipal? DecodeToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = System.Text.Encoding.UTF8.GetBytes(_jwtSecret);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false, // adapta conforme tua API
                    ValidateAudience = false,
                    ValidateLifetime = true, // verifica expiração
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero // sem tolerância extra
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                // opcional: garantir que o token é JWT
                if (validatedToken is not JwtSecurityToken jwtToken)
                    return null;

                return principal;
            }
            catch
            {
                // Token inválido ou expirado → retorna null
                return null;
            }
        }

        // Método auxiliar para checar se token está expirado
        public bool IsTokenValid(string token) => DecodeToken(token) != null;
    }
}
