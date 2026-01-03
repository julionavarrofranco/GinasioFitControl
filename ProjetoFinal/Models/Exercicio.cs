namespace ProjetoFinal.Models
{
    public enum GrupoMuscular
    {
        Peito,
        Costas,
        Ombros,
        Bracos,
        Pernas,
        Abdominais,
        CorpoInteiro
    }

    public class Exercicio
    {
        public int IdExercicio { get; set; }

        public string Nome { get; set; } = null!;

        public GrupoMuscular GrupoMuscular { get; set; }

        public string Descricao { get; set; } = null!;

        public string FotoUrl { get; set; } = null!;

        public bool Ativo { get; set; }

        public ICollection<PlanoExercicio> PlanosExercicios { get; set; } = new List<PlanoExercicio>();
    }
}

