namespace ProjetoFinal.Models.DTOs
{
    public class UpdateMemberDto
    {
        public string? Nome { get; set; }

        public string? Telemovel { get; set; }

        public DateTime? DataNascimento { get; set; }

        public int? IdSubscricao { get; set; }
    }
}
