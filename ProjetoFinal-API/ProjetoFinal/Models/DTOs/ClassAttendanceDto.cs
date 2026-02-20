namespace ProjetoFinal.Models.DTOs
{
    public class ClassAttendanceDto
    {
        public int IdAulaMarcada { get; set; }

        public DateTime DataAula { get; set; }

        public string NomeAula { get; set; } = null!;

        public TimeSpan HoraInicio { get; set; }

        public TimeSpan HoraFim { get; set; }

        public int Capacidade { get; set; }

        public int TotalReservas => Reservas.Count;

        public List<MemberReservationDto> Reservas { get; set; } = new List<MemberReservationDto>();
    }
}
