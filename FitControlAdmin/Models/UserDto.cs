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
}

