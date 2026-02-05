using ProjetoFinal.Models;

namespace ProjetoFinal.Models.DTOs
{
    public class UserResponseDto
    {
        public int IdUser { get; set; }
        public string Email { get; set; } = null!;
        public string Tipo { get; set; } = null!;
        public string? Nome { get; set; }
        public string? Telemovel { get; set; }
        public bool Ativo { get; set; }
        public string? Funcao { get; set; }
        public int? IdSubscricao { get; set; }
        public DateTime? DataNascimento { get; set; }

        // Propriedades para funcionário
        public int? IdFuncionario { get; set; }
        public string? NomeFuncionario { get; set; }
        public string? EmailFuncionario { get; set; }
        public string? TelemovelFuncionario { get; set; }
        public Funcao? FuncaoFuncionario { get; set; }

        // Propriedades para membro
        public int? IdMembro { get; set; }
    }
}
