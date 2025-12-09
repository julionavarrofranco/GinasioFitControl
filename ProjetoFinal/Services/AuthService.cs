using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjetoFinal.Data;
using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ProjetoFinal.Services
{
    public class AuthService : IAuthService
    {
        private readonly GinasioDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        public AuthService(GinasioDbContext context, IConfiguration configuration, IUserService userService)
        {
            _context = context;
            _configuration = configuration;
            _userService = userService;
        }

        // Regista um novo utilizador (Membro ou Funcionário) e envia email com credenciais
        // Adiciona validações de permissão conforme o utilizador atual (Admin - pode criar todos, Receção - só Membros e Funcionários com função "PT" e "Receção", PT - nenhum)
        // Melhoria: adicionar validações extras (ex: telemóvel único, idade mínima para membros, etc)
        public Task<User> RegisterAsync(UserRegisterDto dto, CurrentUserInfo currentUser)
        {
            return _userService.CreateUserAsync(dto, currentUser);
        }


        // Verifica credenciais e retorna JWT + refresh token
        // Se for o primeiro login, indica que é necessário alterar a password       
        public async Task<TokenResponseDto> LoginAsync(UserLoginDto request)
        {
            var user = await _context.Users.Include(u => u.RefreshTokens)                                           
                                           .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                throw new UnauthorizedAccessException("Credenciais inválidas."); 

            if (!user.Ativo)
                throw new UnauthorizedAccessException("Conta desativada.");

            var result = new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Credenciais inválidas.");

            if (user.Tipo == Tipo.Funcionario)
                user = await _userService.GetUserByIdAsync(user.IdUser, includeFuncionario: true);
            if (user == null)
                throw new KeyNotFoundException("Erro ao carregar dados do funcionário.");

            var tokenResponse = await CreateTokenResponse(user);

            if (user.PrimeiraVez)
            {
                tokenResponse.NeedsPasswordChange = true;
                tokenResponse.Message = "Por favor, altera a tua palavra-passe.";
            }

            return tokenResponse;
        }

        // Invalida todos os refresh tokens do utilizador
        /* Obs: Após cancelar os tokens, o access token ainda é válido até expirar (1 hora), no entanto não é mais renovado.
         * Cabe ao front-end eliminar o access token do armazenamento local e redirecionar para a página de login.
         * Assim, mesmo que o access token ainda esteja ativo, o utilizador não poderá renovar a sessão. */
        public async Task LogoutAsync(int userId)
        {
            await _userService.CancelarActiveTokensAsync(userId);
        }

        // Valida e renova tokens (rotaciona refresh token)
        // Implementa deteção de replay (revoga toda a sessão do utilizador se um token for reutilizado)
        // Retorna novos tokens (access + refresh)
        // Obs: o front-end deve substituir os tokens antigos pelos novos no armazenamento local
        //Melhoria: Em sql fazer stored procedure para eliminar tokens expirados periodicamente
        public async Task<TokenResponseDto> RefreshTokensAsync(RefreshTokenRequestDto request)
        {
            var incomingHash = HashToken(request.RefreshToken);
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == incomingHash &&
                                                                              !t.Cancelado &&
                                                                              t.Validade > DateTime.UtcNow);


            if (token == null)
            {
                // Detecção de replay: procurar se algum token teve SubstituidoPor == incomingHash.
                var reused = await _context.RefreshTokens
                               .FirstOrDefaultAsync(t => t.SubstituidoPor == incomingHash);

                if (reused != null)
                {
                    await _userService.CancelarActiveTokensAsync(reused.IdUser);
                    throw new UnauthorizedAccessException("Refresh token reutilizado (replay) detectado.");
                }

                throw new UnauthorizedAccessException("Refresh token inválido.");
            }

            token.Cancelado = true;
            var user = await _userService.GetUserByIdAsync(token.IdUser, includeFuncionario: true);

            if (user == null)
                throw new KeyNotFoundException("Utilizador não encontrado.");

            var newTokens = await CreateTokenResponse(user);
            token.SubstituidoPor = HashToken(newTokens.RefreshToken);

            await _context.SaveChangesAsync();

            return newTokens;
        }
    
        //Métodos complementares

        // Método de criação do JWT
        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.IdUser.ToString()),
                new Claim("Tipo", user.Tipo.ToString())
            };

            // Adiciona claims específicas conforme o tipo de utilizador
            if (user.Tipo == Tipo.Funcionario && user.Funcionario != null)
            {
                claims.Add(new Claim("Funcao", user.Funcionario.Funcao.ToString()));
            }


            var keyString = _configuration["Jwt:Key"]!;
            var keyBytes = Encoding.UTF8.GetBytes(keyString);
            // Garante que a chave tem pelo menos 64 bytes para HmacSha512 (mínimo recomendado)
            if (keyBytes.Length < 64)
                throw new InvalidOperationException("A chave JWT deve ter pelo menos 64 bytes para HmacSha512.");

            var key = new SymmetricSecurityKey(keyBytes);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha512)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Método para criar a hash do refresh token antes de guardar na bd
        private static string HashToken(string token)
        {
            var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }

        // Método para criar a resposta com access token e refresh token
        private async Task<TokenResponseDto> CreateTokenResponse(User user)
        {
            var accessToken = CreateToken(user);
            var refreshToken = await GenerateAndSaveRefreshTokenAsync(user);

            return new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        //Método para criar refresh token em texto simples 
        private string CreateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        // Método para gerar o refresh token, guardar a hash na bd e devolver o token em texto simples para o utilizador
        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            var refreshToken = CreateRefreshToken();
            var refreshTokenHash = HashToken(refreshToken);

            var token = new RefreshToken
            {
                IdUser = user.IdUser,
                Token = refreshTokenHash,
                Validade = DateTime.UtcNow.AddDays(7),
                Cancelado = false
            };

            _context.RefreshTokens.Add(token);
            await _context.SaveChangesAsync();
            return refreshToken;
        }
    }
}