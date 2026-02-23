namespace ProjetoFinal.Models.DTOs
{
    public class ClassDto
    {
        public int? IdFuncionario { get; set; }

        public string Nome { get; set; } = null!;

        public DiaSemana DiaSemana { get; set; }

        public TimeSpan HoraInicio { get; set; }

        public TimeSpan HoraFim { get; set; }

        public int Capacidade { get; set; }

    }
}
