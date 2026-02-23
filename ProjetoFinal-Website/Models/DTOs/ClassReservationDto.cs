namespace TTFWebsite.Models.DTOs
{
    public class ClassReservationDto
    {
        public int idMembroAula { get; set; }
        public int idMembro { get; set; }
        public int idAulaMarcada { get; set; }
        public string nomeAula { get; set; } = null!;
        public string nomeMembro { get; set; } = null!;
        public string instrutor { get; set; } = null!;
        public DateTime DataAula { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFim { get; set; }
        public DateTime dataReserva { get; set; }
        public int sala { get; set; }
    }
}
