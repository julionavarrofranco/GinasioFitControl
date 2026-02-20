namespace ProjetoFinal.Models.DTOs
{
    public class MemberReservationDto
    {
        public int IdMembro { get; set; }

        public string Nome { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Telemovel { get; set; } = null!;

        public Presenca Presenca { get; set; }
    }
}
