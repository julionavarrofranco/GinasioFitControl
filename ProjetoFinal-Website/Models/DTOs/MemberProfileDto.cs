namespace TTFWebsite.Models.DTOs
{
    public class MemberProfileDto
    {
        public int IdMembro { get; set; }
        public string Nome { get; set; } = "";
        public string Email { get; set; } = "";
        public string Telemovel { get; set; } = "";
        public DateTime DataNascimento { get; set; }
        public DateTime DataRegisto { get; set; }
        public string Subscricao { get; set; } = "";
        public string? PersonalTrainer { get; set; }  // se existir
    }

}
