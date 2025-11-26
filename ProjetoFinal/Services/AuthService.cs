using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjetoFinal.Data;
using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ProjetoFinal.Services
{
    public class AuthService : IAuthService
    {
        private readonly GinasioDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(GinasioDbContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }
        //mudar depois os nomes das exceptions para portugues
        // Registro
        // Registro presencial pelo funcionário da recepção
        public async Task<User> RegisterAsync(UserRegisterDto request, User currentUser)
        {
            // 1. Permissões
            if (currentUser.Tipo != Tipo.Funcionario ||
                (currentUser.Funcionario!.Funcao != Funcao.Admin && currentUser.Funcionario.Funcao != Funcao.Rececao))
            {
                throw new UnauthorizedAccessException("Você não tem permissão para registrar usuários.");
            }

            // 2. Validações obrigatórias
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new InvalidOperationException("O email é obrigatório.");

            if (string.IsNullOrWhiteSpace(request.Nome))
                throw new InvalidOperationException("O nome é obrigatório.");

            if (string.IsNullOrWhiteSpace(request.Telemovel))
                throw new InvalidOperationException("O telemóvel é obrigatório.");

            // 3. Email duplicado
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                throw new InvalidOperationException("Email já cadastrado.");

            // 4. Tipo
            if (!Enum.TryParse<Tipo>(request.Tipo, true, out var tipoEnum))
                throw new InvalidOperationException("Tipo de usuário inválido.");

            // 5. Criar User
            var user = new User
            {
                Email = request.Email,
                Tipo = tipoEnum
            };

            var tempPassword = GenerateRandomPassword();
            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, tempPassword);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 6. Criar Membro
            if (tipoEnum == Tipo.Membro)
            {
                if (request.IdSubscricao == null)
                    throw new InvalidOperationException("Membros precisam de uma subscrição.");

                if (request.DataNascimento == null)
                    throw new InvalidOperationException("Membros precisam de data de nascimento.");

                var subscricaoExiste = await _context.Subscricoes
                                                     .AnyAsync(s => s.IdSubscricao == request.IdSubscricao);

                if (!subscricaoExiste)
                    throw new InvalidOperationException("A subscrição indicada não existe.");

                var membro = new Membro
                {
                    IdUser = user.IdUser,
                    Nome = request.Nome,
                    Telemovel = request.Telemovel,
                    DataNascimento = request.DataNascimento.Value,
                    IdSubscricao = request.IdSubscricao.Value,
                    DataRegisto = DateTime.UtcNow
                };

                _context.Membros.Add(membro);
            }

            // 7. Criar Funcionário
            if (tipoEnum == Tipo.Funcionario)
            {
                if (string.IsNullOrWhiteSpace(request.Funcao))
                    throw new InvalidOperationException("Funcionários precisam de uma função válida.");

                if (!Enum.TryParse<Funcao>(request.Funcao, true, out var funcaoEnum))
                    throw new InvalidOperationException("Função de funcionário inválida.");

                var funcionario = new Funcionario
                {
                    IdUser = user.IdUser,
                    Nome = request.Nome,
                    Telemovel = request.Telemovel,
                    Funcao = funcaoEnum
                };

                _context.Funcionarios.Add(funcionario);
            }

            // 8. Gravação final
            await _context.SaveChangesAsync();

            // 9. Email
            await _emailService.SendEmailAsync(
                user.Email,
                "Credenciais do sistema",
                $"Olá {user.Email}, sua senha temporária é: {tempPassword}\n" +
                $"Você precisará trocá-la no primeiro login."
            );

            return user;
        }



        // Helper para gerar senha aleatória
        private string GenerateRandomPassword(int length = 12)
        {
            const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_~-";
            var randomBytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            return new string(randomBytes.Select(b => validChars[b % validChars.Length]).ToArray());
        }



        public async Task<TokenResponseDto> LoginAsync(UserLoginDto request)
        {
            var user = await _context.Users.Include(u => u.RefreshTokens)
                                           .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                throw new UnauthorizedAccessException("Credenciais inválidas."); // usuário não encontrado

            var result = new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (result == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Credenciais inválidas."); // senha incorreta

            return await CreateTokenResponse(user);
        }



        // Criação de JWT + refresh token
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

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.IdUser.ToString()),
                new Claim(ClaimTypes.Role, user.Tipo.ToString())
            };

            var keyString = _configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(keyString))
                throw new InvalidOperationException("Jwt:Key não configurada");


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Gerar refresh token
        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            var refreshToken = CreateRefreshToken();

            var tokenEntity = new RefreshToken
            {
                IdUser = user.IdUser,
                Token = refreshToken,
                Validade = DateTime.UtcNow.AddDays(7),
                Cancelado = false
            };

            _context.RefreshTokens.Add(tokenEntity);
            await _context.SaveChangesAsync();
            return refreshToken;
        }

        // Refresh token
        public async Task<TokenResponseDto> RefreshTokensAsync(RefreshTokenRequestDto request)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.IdUser == request.IdUser &&
                                          t.Token == request.RefreshToken &&
                                          !t.Cancelado &&
                                          t.Validade > DateTime.UtcNow);

            if (token == null)
                throw new UnauthorizedAccessException("Refresh token inválido ou expirado."); // ✅ Exceção ao invés de null

            var user = await _context.Users.FindAsync(request.IdUser);
            if (user == null)
                throw new UnauthorizedAccessException("Usuário não encontrado.");

            return await CreateTokenResponse(user);
        }

        // Logout (revoga todos refresh tokens ativos do user)
        public async Task LogoutAsync(int userId)
        {
            var tokens = _context.RefreshTokens.Where(t => t.IdUser == userId && !t.Cancelado);
            foreach (var t in tokens) t.Cancelado = true;

            await _context.SaveChangesAsync();
        }

        // Force password change (primeiro login ou redefinição)
        public async Task ResetPasswordAsync(string email)
        {
            var user = await _context.Users.Include(u => u.RefreshTokens)
                               .FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) throw new InvalidOperationException("Usuário não encontrado.");

            var tempPassword = GenerateRandomPassword();
            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, tempPassword);
            user.PrimeiraVez = true;

            // Cancela refresh tokens
            foreach (var t in user.RefreshTokens.Where(t => !t.Cancelado))
                t.Cancelado = true;

            await _context.SaveChangesAsync();

            // Envia email com a nova senha
            await _emailService.SendEmailAsync(user.Email, "Nova senha",
                $"Olá {user.Email}, sua nova senha é: {tempPassword}");
        }


        // Mudar password
        public async Task ChangePasswordAsync(ChangePasswordDto request)
        {
            if (string.IsNullOrWhiteSpace(request.PasswordAtual))
                throw new InvalidOperationException("A password atual é obrigatória.");

            if (string.IsNullOrWhiteSpace(request.NovaPassword))
                throw new InvalidOperationException("A nova password é obrigatória.");

            if (request.PasswordAtual == request.NovaPassword)
                throw new InvalidOperationException("A nova password não pode ser igual à atual.");

            var user = await _context.Users.FindAsync(request.IdUser);
            if (user == null)
                throw new InvalidOperationException("Utilizador não encontrado.");

            var result = new PasswordHasher<User>().VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.PasswordAtual
            );

            if (result == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Password atual incorreta.");

            // Hash da nova password
            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.NovaPassword);
            user.PrimeiraVez = false;

            await _context.SaveChangesAsync();
        }


        // Rotacionar refresh token
        public async Task<string> RotateRefreshTokenAsync(int userId, string refreshToken)
        {
            var user = await _context.Users
                                     .Include(u => u.RefreshTokens)
                                     .FirstOrDefaultAsync(u => u.IdUser == userId);

            if (user == null) throw new InvalidOperationException("Utilizador não encontrado.");

            var existingToken = user.RefreshTokens
                .FirstOrDefault(t => t.Token == refreshToken && !t.Cancelado);

            if (existingToken == null) throw new UnauthorizedAccessException("Refresh token inválido.");

            // Marcar como cancelado e criar substituídoPor
            var newRefreshToken = CreateRefreshToken();
            existingToken.Cancelado = true;
            existingToken.SubstituidoPor = newRefreshToken;

            var newTokenEntity = new RefreshToken
            {
                IdUser = userId,
                Token = newRefreshToken,
                Validade = DateTime.UtcNow.AddDays(7),
                Cancelado = false
            };

            user.RefreshTokens.Add(newTokenEntity);
            await _context.SaveChangesAsync();

            return newRefreshToken;
        }

        // Helper para criar token (usado no rotate)
        private string CreateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}