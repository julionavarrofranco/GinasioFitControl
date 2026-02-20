namespace ProjetoFinal.Models
{
    public class PlanoTreino
    {
        public int IdPlano { get; set; }

        public int IdFuncionario { get; set; }

        public string Nome { get; set; } = null!;

        public DateTime DataCriacao { get; set; }

        public string? Observacoes { get; set; }

        public DateTime? DataDesativacao { get; set; }

        public Funcionario Funcionario { get; set; } = null!;

        public ICollection<PlanoExercicio> PlanosExercicios { get; set; } = new List<PlanoExercicio>();
    }
}
