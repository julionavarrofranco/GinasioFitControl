namespace FitControlAdmin.Models
{
    public class UserDto
    {
        public int IdUser { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Telemovel { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public string? Funcao { get; set; }
        public int? IdSubscricao { get; set; }
        public DateTime? DataNascimento { get; set; }

        // Propriedades para incluir informações do funcionário
        public int? IdFuncionario { get; set; }
        public string? NomeFuncionario { get; set; }
        public string? EmailFuncionario { get; set; }
        public string? TelemovelFuncionario { get; set; }
        public Funcao? FuncaoFuncionario { get; set; }
    }


    public class UserUpdateDto
    {
        public string? Nome { get; set; }
        public string? Telemovel { get; set; }
        public bool? Ativo { get; set; }
        public DateTime? DataNascimento { get; set; }
        public int? IdSubscricao { get; set; }
        public string? Funcao { get; set; }
    }

    public class UserRegisterDto
    {
        public string Email { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Telemovel { get; set; } = string.Empty;
        public DateTime? DataNascimento { get; set; }
        public int? IdSubscricao { get; set; }
        public string? Funcao { get; set; }
        public MetodoPagamento? MetodoPagamento { get; set; }
    }

    public class MemberDto
    {
        public int IdUser { get; set; }
        public int IdMembro { get; set; }
        public string Nome { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Telemovel { get; set; } = default!;
        public DateTime DataNascimento { get; set; }
        public DateTime DataRegisto { get; set; }
        public string Subscricao { get; set; } = default!;    // Nome da subscri��o
        public string PlanoTreino { get; set; } = default!;   // Nome do plano
        public string DataDesativacao { get; set; } = "Ativo"; // "Ativo" ou data
        public bool Ativo { get; set; }
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public bool NeedsPasswordChange { get; set; }
        public string? Message { get; set; }
    }

    public class ResetPasswordDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ChangePasswordDto
    {
        public string PasswordAtual { get; set; } = string.Empty;
        public string NovaPassword { get; set; } = string.Empty;
        public string ConfirmarPassword { get; set; } = string.Empty;
    }

    public class UserStatusDto
    {
        public int IdUser { get; set; }
        public bool IsActive { get; set; }
    }
}

