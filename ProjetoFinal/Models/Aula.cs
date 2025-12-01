namespace ProjetoFinal.Models
{
    public enum DiaSemana
    {
        Segunda, Terca, Quarta, Quinta, Sexta, Sabado, Domingo
    }

    public class Aula
    {
        public int IdAula { get; set; }

        public int IdFuncionario { get; set; }

        public string Nome { get; set; } = null!;

        public DiaSemana DiaSemana { get; set; }

        public TimeSpan HoraInicio { get; set; }

        public TimeSpan HoraFim { get; set; }

        public int Capacidade { get; set; }

        public DateTime? DataDesativacao { get; set; }

        public Funcionario Funcionario { get; set; } = null!;

        public ICollection<MembroAula> MembrosAulas { get; set; } = new List<MembroAula>();
    }
}
