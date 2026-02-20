using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace TTFWebsite.Services
{
    public class JwtService
    {
        private readonly string _jwtSecret;
        private readonly string? _issuer;
        private readonly string? _audience;

        public JwtService(IConfiguration configuration)
        {
            _jwtSecret = configuration["Jwt:Key"]
                         ?? throw new InvalidOperationException("JWT Key não encontrada nas configurações.");
            _issuer = configuration["Jwt:Issuer"];
            _audience = configuration["Jwt:Audience"];
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
                    ValidateIssuer = !string.IsNullOrEmpty(_issuer),
                    ValidateAudience = !string.IsNullOrEmpty(_audience),
                    ValidateLifetime = true, // verifica expiração
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidIssuer = _issuer,
                    ValidAudience = _audience,
                    ClockSkew = TimeSpan.Zero // sem tolerância extra
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
              
                if (validatedToken is not JwtSecurityToken jwtToken)
                    return null;

                return principal;
            }
            catch
            {
                // Token inválido ou expirado
                return null;
            }
        }

        // Método auxiliar para checar se token está expirado
        public bool IsTokenValid(string token) => DecodeToken(token) != null;
    }
}
