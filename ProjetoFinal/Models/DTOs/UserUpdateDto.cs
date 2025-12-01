namespace ProjetoFinal.Models.DTOs
{
    public class UserUpdateDto
    {
        public string? Nome { get; set; }
        public string? Telemovel { get; set; }
        public bool? Ativo { get; set; }
        public DateTime? DataNascimento { get; set; }
        public int? IdSubscricao { get; set; }
        public string? Funcao { get; set; }
    }
}

