namespace ProjetoFinal.Models.DTOs
{
    public class ReserveClassResponseDto
    {
        public int IdMembro { get; set; }

        public int IdAulaMarcada { get; set; }

        public DateTime DataReserva { get; set; }

        public string Presenca { get; set; } = "Reservado";
    }
}
