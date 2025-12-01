namespace ProjetoFinal.Models.DTOs
{
    public class UserListDto
    {
        public int IdUser { get; set; }
        public string Email { get; set; } = null!;
        public string Tipo { get; set; } = null!;
        public string Nome { get; set; } = null!;
        public string Telemovel { get; set; } = null!;
        public bool Ativo { get; set; }
        public string? Funcao { get; set; }
        public int? IdSubscricao { get; set; }
        public DateTime? DataNascimento { get; set; }
    }
}

