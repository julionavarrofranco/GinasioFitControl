namespace ProjetoFinal.Models.DTOs
{
    public class ClassReservationSummaryDto
    {
        public int IdAulaMarcada { get; set; }
        public DateTime DataAula { get; set; }
        public string NomeAula { get; set; } = null!;
        public int Sala { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFim { get; set; }
        public int Capacidade { get; set; }
        public int TotalReservas { get; set; }
    }
}
