using System;

namespace ProjetoFinal.Models
{
    public enum Tipo
    {
        Funcionario,
        Membro
    }

    public class User
    {
        public int IdUser { get; set; }

        public string Email { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public Tipo Tipo { get; set; }

        public bool PrimeiraVez { get; set; } = true;

        public bool Ativo { get; set; } = true;

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        public Membro? Membro { get; set; }

        public Funcionario? Funcionario { get; set; }
    }
}
