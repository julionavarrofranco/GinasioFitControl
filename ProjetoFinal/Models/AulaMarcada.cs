namespace ProjetoFinal.Models
{
    public class AulaMarcada
    {
        public int Id { get; set; }

        public int IdAula { get; set; }

        public DateTime DataAula { get; set; }

        public DateTime? DataDesativacao { get; set; }

        public ICollection<MembroAula> MembrosAulas { get; set; } = new List<MembroAula>();

        public Aula Aula { get; set; } = null!;
    }
}
