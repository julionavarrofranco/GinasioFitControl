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
        private readonly IEmailService _emailService;

        public AuthService(GinasioDbContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        // 1. REGISTO
        // Regista um novo utilizador (Membro ou Funcionário) e envia email com credenciais
        // Adiciona validações de permissão conforme o utilizador atual (Admin - pode criar todos, Receção - só Membros e Funcionários com função "PT" e "Receção", PT - nenhum)
        // Melhoria: adicionar validações extras (ex: telemóvel único, idade mínima para membros, etc)
        public async Task<User> RegisterAsync(UserRegisterDto request, User currentUser)
        {
            // Email em minúsculas e sem espaços
            var email = request.Email?.Trim().ToLower();

            // Validações de input
            if (string.IsNullOrWhiteSpace(email) || 
                string.IsNullOrWhiteSpace(request.Nome) ||
                string.IsNullOrWhiteSpace(request.Telemovel))
                throw new InvalidOperationException("Por favor, preencha todos os campos obrigatórios.");

            var emailValidator = new EmailAddressAttribute(); // Validador de email do DataAnnotations
            if (!emailValidator.IsValid(email))
                throw new InvalidOperationException("Por favor, insira um email válido.");

            var phoneRegex = new Regex(@"^\+?\d{7,15}$"); // Permite apenas números e + (no ínicio), mínimo 7, máximo 15 dígitos
            if (!phoneRegex.IsMatch(request.Telemovel))
                throw new InvalidOperationException("Por favor, insira um Nº de telemóvel válido.");

            if (await _context.Users.AnyAsync(u => u.Email == email))
                throw new InvalidOperationException("Este email já está registado.");

            if (!Enum.TryParse<Tipo>(request.Tipo, true, out var tipoEnum))
                throw new InvalidOperationException("Tipo de utilizador inválido.");

            // Validações de permissão
            if (currentUser.Tipo == Tipo.Funcionario)
            {
                var funcao = currentUser.Funcionario?.Funcao;

                if (funcao == Funcao.Rececao)
                {
                    if (tipoEnum == Tipo.Funcionario)
                    {
                        if (!Enum.TryParse<Funcao>(request.Funcao, true, out var funcaoNovo))
                            throw new InvalidOperationException("Função do funcionário inválida.");

                        if (funcaoNovo == Funcao.Admin)
                            throw new UnauthorizedAccessException();
                    }              
                }
            }

            // Operação assíncrona na bd, garante que todas as operações dentro do bloco sejam atômicas (tudo ou nada) e revertidas automaticamente em caso de erro.
            await using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                // Criação do User - id, email, tipo, password hash, (Ativo e PrimeiraVez por default = true)
                var user = new User
                {
                    Email = email,
                    Tipo = tipoEnum
                };

                var tempPassword = GenerateRandomPassword();
                user.PasswordHash = new PasswordHasher<User>().HashPassword(user, tempPassword);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Cria Membro ou Funcionário, com as informações detalhadas de cada um
                if (tipoEnum == Tipo.Membro)
                {
                    if (request.IdSubscricao == null)
                        throw new InvalidOperationException("Membros precisam de uma subscrição.");

                    if (request.DataNascimento == null)
                        throw new InvalidOperationException("Membros precisam de uma data de nascimento.");

                    if (!await _context.Subscricoes.AnyAsync(s => s.IdSubscricao == request.IdSubscricao))
                        throw new InvalidOperationException("A subscrição indicada não existe.");

                    _context.Membros.Add(new Membro
                    {
                        IdUser = user.IdUser,
                        Nome = request.Nome,
                        Telemovel = request.Telemovel,
                        DataNascimento = request.DataNascimento.Value,
                        IdSubscricao = request.IdSubscricao.Value,
                        DataRegisto = DateTime.UtcNow
                    });
                }
                else if (tipoEnum == Tipo.Funcionario)
                {
                    if (string.IsNullOrWhiteSpace(request.Funcao) || !Enum.TryParse<Funcao>(request.Funcao, true, out var funcaoEnum))
                        throw new InvalidOperationException("Função de funcionário inválida.");
                     
                    _context.Funcionarios.Add(new Funcionario
                    {
                        IdUser = user.IdUser,
                        Nome = request.Nome,
                        Telemovel = request.Telemovel,
                        Funcao = funcaoEnum
                    });
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync(); 

                // Enviar email
                await _emailService.SendEmailAsync(
                    user.Email,
                    "Credenciais do sistema",
                    $"Olá {user.Email}, aqui tens as tuas credenciais \n Username: {user.Email} \n Palavra-passe temporária: {tempPassword}\n" +
                    "Precisas de trocar a palavra-passe no primeiro login."
                );
                return user;
            }
            catch
            {
                await tx.RollbackAsync(); // Reverte todas as operações em caso de erro
                throw;
            }
        }

        // 2. LOGIN
        // Verifica credenciais e retorna JWT + refresh token
        // Se for o primeiro login, indica que é necessário alterar a password       
        public async Task<TokenResponseDto> LoginAsync(UserLoginDto request)
        {
            var user = await _context.Users
                                     .Include(u => u.RefreshTokens)
                                     .Include(u => u.Funcionario)
                                     .Include(u => u.Membro)
                                     .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                throw new UnauthorizedAccessException("Credenciais inválidas."); 

            if (!user.Ativo)
                throw new UnauthorizedAccessException("Conta desativada.");

            var result = new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Credenciais inválidas.");

            var tokenResponse = await CreateTokenResponse(user);

            if (user.PrimeiraVez)
            {
                tokenResponse.NeedsPasswordChange = true;
                tokenResponse.Message = "Por favor, altera a tua palavra-passe.";
            }

            return tokenResponse;
        }

        // 3. LOGOUT
        // Invalida todos os refresh tokens do utilizador
        /* Obs: Após cancelar os tokens, o access token ainda é válido até expirar (1 hora), no entanto não é mais renovado.
         * Cabe ao front-end eliminar o access token do armazenamento local e redirecionar para a página de login.
         * Assim, mesmo que o access token ainda esteja ativo, o utilizador não poderá renovar a sessão. */
        public async Task LogoutAsync(int userId)
        {
            var tokens = _context.RefreshTokens.Where(t => t.IdUser == userId && !t.Cancelado).ToList();
            foreach (var t in tokens)
            {
                t.Cancelado = true;
            }

            await _context.SaveChangesAsync();
        }

        // 4. REFRESH TOKEN
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
                    // Replay detectado — revoga todos os tokens do utilizador
                    var tokens = _context.RefreshTokens.Where(t => t.IdUser == reused.IdUser && !t.Cancelado).ToList();
                    foreach (var t in tokens)
                    {
                        t.Cancelado = true;
                    }
                    await _context.SaveChangesAsync();

                    throw new UnauthorizedAccessException("Refresh token reutilizado (replay) detectado.");
                }

                throw new UnauthorizedAccessException("Refresh token inválido.");
            }

            token.Cancelado = true;
            var user = await _context.Users
                                     .Include(u => u.Funcionario)
                                     .Include(u => u.Membro)
                                     .FirstOrDefaultAsync(u => u.IdUser == token.IdUser);
            if (user == null)
                throw new UnauthorizedAccessException("Utilizador não encontrado.");

            var newTokens = await CreateTokenResponse(user);
            token.SubstituidoPor = HashToken(newTokens.RefreshToken);

            await _context.SaveChangesAsync();

            return newTokens;
        }

        // 5. REDEFINIÇÃO DE PASSWORD
        // Redefinir a palavra-passe de um utilizador, enviando uma nova password temporária por email
        // Cancela todos os refresh tokens ativos e força re-login
        public async Task ResetPasswordAsync(ResetPasswordDto email)
        {
            var emailUser = email.Email.Trim().ToLower();
            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(emailUser))
                throw new InvalidOperationException("Por favor, insira um email válido.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailUser);
            if(user == null) 
                throw new InvalidOperationException("Email não foi encontrado.");

            var tempPassword = GenerateRandomPassword();
            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, tempPassword);
            user.PrimeiraVez = true; // Força alteração da password no próximo login

            foreach (var t in user.RefreshTokens.Where(t => !t.Cancelado).ToList())
            {
                t.Cancelado = true;
            }

            await _context.SaveChangesAsync();

            await _emailService.SendEmailAsync(
                  user.Email,
                  "Credenciais do sistema",
                  $"Olá {user.Email}, aqui tens a tua nova palavra-passe temporária: {tempPassword}\n" +
                  "Precisas de trocar a palavra-passe novamente após login."
              );
        }

        // 6. ALTERAÇÃO DE PASSWORD
        // Permite ao utilizador alterar a sua password, valida a password atual e aplica as regras de complexidade na nova password
        // Cancela todos os refresh tokens ativos e força re-login
        public async Task ChangePasswordAsync(int idUser,ChangePasswordDto request)
        {
            //Validações de input
            if (string.IsNullOrWhiteSpace(request.PasswordAtual))
                throw new InvalidOperationException("A palavra-passe atual é obrigatória.");

            if (string.IsNullOrWhiteSpace(request.NovaPassword))
                throw new InvalidOperationException("A nova palavra-passe é obrigatória.");

            if (request.PasswordAtual == request.NovaPassword)
                throw new InvalidOperationException("A nova palavra-passe não pode ser igual à atual.");

            if (!PasswordComplexity(request.NovaPassword))
                throw new InvalidOperationException("A nova palavra-passe não cumpre os requisitos de complexidade.");

            var user = await _context.Users.FindAsync(idUser);
            if (user == null)
                throw new InvalidOperationException("Utilizador não encontrado.");

            var result = new PasswordHasher<User>().VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.PasswordAtual
            );

            if (result == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Palavra-passe incorreta.");

            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.NovaPassword);
            user.PrimeiraVez = false;

            var tokens = _context.RefreshTokens.Where(t => t.IdUser == idUser && !t.Cancelado).ToList();
            foreach (var t in tokens)
            {
                t.Cancelado = true;
            }
            await _context.SaveChangesAsync();
        }

        // Alteração de password por administradores (por email)
        public async Task AdminChangePasswordAsync(AdminChangePasswordDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new InvalidOperationException("Email é obrigatório.");

            if (string.IsNullOrWhiteSpace(request.NovaPassword))
                throw new InvalidOperationException("A nova palavra-passe é obrigatória.");

            if (!PasswordComplexity(request.NovaPassword))
                throw new InvalidOperationException("A nova palavra-passe não cumpre os requisitos de complexidade.");

            var email = request.Email.Trim().ToLower();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                throw new InvalidOperationException("Utilizador não encontrado.");

            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.NovaPassword);
            user.PrimeiraVez = false;

            var tokens = _context.RefreshTokens.Where(t => t.IdUser == user.IdUser && !t.Cancelado).ToList();
            foreach (var token in tokens)
            {
                token.Cancelado = true;
            }

            await _context.SaveChangesAsync();
        }

        //Métodos complementares

        // Método para gerar password temporária aleatória
        // Garante complexidade mínima (maiúsculas, minúsculas, números, símbolos) e comprimento padrão de 12 caracteres
        // Usa algoritmo Fisher–Yates para baralhar os caracteres (evita padrões previsíveis)
        private string GenerateRandomPassword(int length = 12)
        {
            const string maiusculas = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string minusculas = "abcdefghijklmnopqrstuvwxyz";
            const string numeros = "0123456789";
            const string simbolos = "!@#$%^&*?_~-";
            string caracteres = maiusculas + minusculas + numeros + simbolos;

            var randomPassword = new List<char>(length);

            // Garante a complexidade mínima
            randomPassword.Add(maiusculas[RandomNumberGenerator.GetInt32(maiusculas.Length)]);
            randomPassword.Add(minusculas[RandomNumberGenerator.GetInt32(minusculas.Length)]);
            randomPassword.Add(numeros[RandomNumberGenerator.GetInt32(numeros.Length)]);
            randomPassword.Add(simbolos[RandomNumberGenerator.GetInt32(simbolos.Length)]);

            // Preenche o resto da password
            for (int i = randomPassword.Count; i < length; i++)
            {
                int indiceCaracteres = RandomNumberGenerator.GetInt32(caracteres.Length);
                randomPassword.Add(caracteres[indiceCaracteres]);
            }

            // Baralha os caracteres (Fisher–Yates)
            for (int i = randomPassword.Count - 1; i > 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);
                char temp = randomPassword[i];
                randomPassword[i] = randomPassword[j];
                randomPassword[j] = temp;
            }

            return new string(randomPassword.ToArray());
        }

        // Método para validação de complexidade da password (12 caracteres, maiúsculas, minúsculas, números, símbolos) 
        private bool PasswordComplexity(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 12)
                return false;

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSymbol = password.Any(ch => "!@#$%^&*?_~-".Contains(ch));

            return hasUpper && hasLower && hasDigit && hasSymbol;
        }

        // Método de criação do JWT
        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.IdUser.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("Tipo", user.Tipo.ToString())
            };

            // Nome amigável (se existir Membro/Funcionario) – caso contrário, usa email
            string displayName = user.Email;
            if (user.Tipo == Tipo.Membro && user.Membro != null)
                displayName = user.Membro.Nome;
            else if (user.Tipo == Tipo.Funcionario && user.Funcionario != null)
                displayName = user.Funcionario.Nome;

            claims.Add(new Claim(ClaimTypes.Name, displayName));

            // Telemóvel (se existir)
            string? telemovel = null;
            if (user.Tipo == Tipo.Membro && user.Membro != null)
                telemovel = user.Membro.Telemovel;
            else if (user.Tipo == Tipo.Funcionario && user.Funcionario != null)
                telemovel = user.Funcionario.Telemovel;

            if (!string.IsNullOrWhiteSpace(telemovel))
            {
                claims.Add(new Claim("Telemovel", telemovel));
            }

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