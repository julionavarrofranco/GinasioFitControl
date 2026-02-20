namespace ProjetoFinal.Models.DTOs
{
    public class ClassReservationDto
    {
        public int IdMembro { get; set; }

        public int IdAulaMarcada { get; set; }

        public string NomeAula { get; set; } = null!;

        public string NomeMembro { get; set; } = null!;

        public string Instrutor { get; set; } = null!;

        public DateTime DataAula { get; set; }

        public TimeSpan HoraInicio { get; set; }

        public TimeSpan HoraFim { get; set; }

        public DateTime DataReserva { get; set; }

        public int Sala { get; set; }
    }


}
