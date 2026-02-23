namespace ProjetoFinal.Models.DTOs
{
    public class ScheduledClassResponseDto
    {
        public int IdAulaMarcada { get; set; }

        public int IdAula { get; set; }

        public string Nome { get; set; } = null!;

        public DateTime DataAula { get; set; }

        public TimeSpan HoraInicio { get; set; }

        public TimeSpan HoraFim { get; set; }

        public int Capacidade { get; set; }

        public int Sala { get; set; }

        public int ReservasAtuais { get; set; }

        public string? NomeInstrutor { get; set; }
    }

}
