namespace ProjetoFinal.Models.DTOs
{
    public class ExercisePlanDto
    {
        public int IdExercicio { get; set; }

        public int Series { get; set; }

        public int Repeticoes { get; set; }

        public decimal Carga { get; set; }

        public int? Ordem { get; set; }
    }
}
