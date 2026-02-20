namespace ProjetoFinal.Models.DTOs
{
    public class UpdateClassDto
    {
        public int? IdFuncionario { get; set; }

        public string? Nome { get; set; }

        public TimeSpan? HoraInicio { get; set; }

        public TimeSpan? HoraFim { get; set; }

        public DiaSemana? DiaSemana { get; set; }

        public int? Capacidade { get; set; }

        public bool ForceSwap { get; set; } = false;
    }
}
