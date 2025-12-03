namespace ProjetoFinal.Models.DTOs
{
    public class UserRegisterDto
    {
        public string Email { get; set; } = null!;

        public string Tipo { get; set; } = null!; // Membro ou Funcionario

        // Campos comuns
        public string Nome { get; set; } = null!;
        public string Telemovel { get; set; } = null!;

        // Membro
        public DateTime? DataNascimento { get; set; }

        public int? IdSubscricao { get; set; }

        // Funcionário
        public string? Funcao { get; set; } // Admin / Rececao / PT
    }
}
