namespace TTFWebsite.Models.DTOs
{
    public class ScheduleClassDto
    {
        public int IdAulaMarcada { get; set; }
        public required string Nome { get; set; }
        public DateTime DataAula { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFim { get; set; }
        public int Capacidade { get; set; }
        public int Reservas { get; set; }
        public string? NomeFuncionario { get; set; }
        public string? Sala { get; set; }
    }

}
