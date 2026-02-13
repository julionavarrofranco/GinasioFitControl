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

        // Propriedade oficial vindo do servidor: reservas atuais
        public int ReservasAtuais { get; set; }

        public string? NomeInstrutor { get; set; }
        public int? Sala { get; set; }
    }
}
