using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Data;
using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace ProjetoFinal.Services
{
    public class UserService: IUserService
    {
        private readonly GinasioDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IMemberService _memberService;
        private readonly IEmployeeService _employeeService;

        public UserService(GinasioDbContext context, IEmailService emailService, IMemberService memberService, IEmployeeService employeeService)
        {
            _context = context;
            _emailService = emailService;
            _memberService = memberService;
            _employeeService = employeeService;
        }

        public async Task<User?> GetUserByIdAsync(int idUser, bool includeFuncionario = false, bool includeMembro = false)
        {
            IQueryable<User> query = _context.Users;

            if (includeFuncionario)
                query = query.Include(u => u.Funcionario);

            if (includeMembro)
                query = query.Include(u => u.Membro);

            return await query.FirstOrDefaultAsync(u => u.IdUser == idUser);
        }

        public async Task<User> CreateUserAsync(UserRegisterDto request, CurrentUserInfo currentUser)
        {
            // Email em minúsculas e sem espaços
            var email = request.Email?.Trim().ToLower();

            // Validações de input
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(request.Nome) ||
                string.IsNullOrWhiteSpace(request.Telemovel))
                throw new InvalidOperationException("Por favor, preencha todos os campos obrigatórios.");

            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(email))
                throw new InvalidOperationException("Por favor, insira um email válido.");

            var phoneRegex = new Regex(@"^\+\d{7,15}$");
            if (!phoneRegex.IsMatch(request.Telemovel))
                throw new InvalidOperationException("Por favor, insira um Nº de telemóvel válido.");

            if (await _context.Users.AnyAsync(u => u.Email == email && u.Ativo))
                throw new InvalidOperationException("Este email já está registado.");


            if (!Enum.TryParse<Tipo>(request.Tipo, true, out var tipoEnum))
                throw new InvalidOperationException("Tipo de utilizador inválido.");

            // Validações de permissão
            if (currentUser.Tipo == Tipo.Funcionario)
            {
                var funcao = currentUser.Funcao;

                if (funcao == Funcao.Rececao && tipoEnum == Tipo.Funcionario)
                {
                    throw new UnauthorizedAccessException("Funcionário da receção não pode criar funcionários.");
                }
            }

            await using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                // Criar entidade User
                var user = new User
                {
                    Email = email,
                    Tipo = tipoEnum
                };

                var tempPassword = GenerateRandomPassword();
                user.PasswordHash = new PasswordHasher<User>().HashPassword(user, tempPassword);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Criar perfil conforme o tipo
                if (tipoEnum == Tipo.Membro)
                {
                    await _memberService.CreateMemberAsync(user.IdUser,request);
                }
                else if (tipoEnum == Tipo.Funcionario)
                {
                    await _employeeService.CreateEmployeeAsync(user.IdUser, request);
                }

                await _context.SaveChangesAsync();

                // Envio de email com credenciais
                await _emailService.SendEmailAsync(
                    user.Email,
                    "Credenciais do sistema",
                    $"Olá {request.Nome}, aqui tens as tuas credenciais \n" +
                    $"Username: {user.Email} \n" +
                    $"Palavra-passe temporária: {tempPassword}\n\n" +
                    $"Precisas de trocar a palavra-passe no primeiro login."
                );
                await tx.CommitAsync();
                return user;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task ChangeUserActiveStatusAsync(UserStatusDto request)
        {
            int idUser = request.IdUser;
            bool isActive = request.IsActive;
            var user = await GetUserByIdAsync(idUser);
            if (user == null)
            {
                throw new KeyNotFoundException("Utilizador não encontrado.");
            }
            if (isActive)
            {
                bool emailAtivo = await _context.Users.AnyAsync(u => u.Email == user.Email && u.IdUser != idUser && u.Ativo);

                if (emailAtivo)
                    throw new InvalidOperationException("Este email já está registado em um utilizador ativo. Desative o utilizador com o email ativo ou altere o email.");
                
                user.DataDesativacao = null;
            }
            else
            {
                user.DataDesativacao = DateTime.UtcNow;
            }

            user.Ativo = isActive;
            await _context.SaveChangesAsync();
        }

        // Revoga todos os refresh tokens ativos de um utilizador
        public async Task CancelarActiveTokensAsync(int userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(t => t.IdUser == userId && !t.Cancelado)
                .ToListAsync();

            foreach (var t in tokens)
            {
                t.Cancelado = true;
            }

            await _context.SaveChangesAsync();
        }

        // Redefinir a palavra-passe de um utilizador, enviando uma nova password temporária por email
        // Cancela todos os refresh tokens ativos e força re-login
        public async Task ResetPasswordAsync(ResetPasswordDto email)
        {
            var emailUser = email.Email.Trim().ToLower();
            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(emailUser))
                throw new InvalidOperationException("Por favor, insira um email válido.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailUser);
            if (user == null)
                throw new KeyNotFoundException("Email não foi encontrado.");

            var tempPassword = GenerateRandomPassword();
            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, tempPassword);
            user.PrimeiraVez = true; // Força alteração da password no próximo login

            await _emailService.SendEmailAsync(
               user.Email,
               "Credenciais do sistema",
               $"Olá {user.Email}, aqui tens a tua nova palavra-passe temporária: {tempPassword}\n \n" +
               $"Precisas de trocar a palavra-passe novamente após login."
           );

            await CancelarActiveTokensAsync(user.IdUser);       
        }

        // Permite ao utilizador alterar a sua password, valida a password atual e aplica as regras de complexidade na nova password
        // Cancela todos os refresh tokens ativos e força re-login
        public async Task ChangePasswordAsync(int idUser, ChangePasswordDto request)
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

            var user = await GetUserByIdAsync(idUser);
            if (user == null)
                throw new KeyNotFoundException("Utilizador não encontrado.");

            var result = new PasswordHasher<User>().VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.PasswordAtual
            );

            if (result == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Palavra-passe incorreta.");

            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.NovaPassword);
            user.PrimeiraVez = false;
            await CancelarActiveTokensAsync(idUser);
        }

        // Atualizar email
        public async Task ChangeEmailAsync(ChangeEmailDto request)
        {
            var newEmail = request.NewEmail.Trim().ToLower();
            var idUser = request.IdUser;

            var validator = new EmailAddressAttribute();
            if (!validator.IsValid(newEmail))
                throw new InvalidOperationException("Email inválido.");

            if (await _context.Users.AnyAsync(u => u.Email == newEmail && u.IdUser != idUser && u.Ativo))
                throw new InvalidOperationException("Este email já está em uso.");

            var user = await GetUserByIdAsync(idUser);
            if (user == null)
                throw new KeyNotFoundException("Utilizador não encontrado.");

            user.Email = newEmail;
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
    }
}
