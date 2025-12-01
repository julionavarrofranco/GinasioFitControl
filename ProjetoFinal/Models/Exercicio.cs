namespace ProjetoFinal.Models
{
    public class Exercicio
    {
        public int IdExercicio { get; set; }

        public string Nome { get; set; } = null!;

        public string GrupoMuscular { get; set; } = null!;

        public string Descricao { get; set; } = null!;

        public string FotoUrl { get; set; } = null!;

        public bool Ativo { get; set; }

        public ICollection<PlanoExercicio> PlanosExercicios { get; set; } = new List<PlanoExercicio>();
    }
}

