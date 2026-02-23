namespace ProjetoFinal.Models
{
    public class PlanoExercicio
    {
        public int IdPlano { get; set; }

        public int IdExercicio { get; set; }

        public int Series { get; set; }

        public int Repeticoes { get; set; }

        public decimal Carga { get; set; }

        public int Ordem { get; set; }

        public PlanoTreino PlanoTreino { get; set; } = null!;

        public Exercicio Exercicio { get; set; } = null!;
    }
}
