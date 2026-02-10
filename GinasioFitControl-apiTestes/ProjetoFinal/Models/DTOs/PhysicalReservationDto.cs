namespace ProjetoFinal.Models.DTOs
{
    public class PhysicalReservationDto
    {
        public int IdMembroAvaliacao { get; set; }
        public int IdMembro { get; set; }
        public int? IdAvaliacaoFisica { get; set; }
        public DateTime DataReserva { get; set; }   // corresponde ao JSON
        public EstadoAvaliacao Estado { get; set; }
        public DateTime? DataCancelamento { get; set; }
    }
}
