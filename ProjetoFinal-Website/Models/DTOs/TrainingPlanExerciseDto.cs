namespace TTFWebsite.Models.DTOs
{
    public class TrainingPlanExerciseDto
    {
        public int IdExercicio { get; set; }
        public string NomeExercicio { get; set; } = string.Empty;
        public string GrupoMuscular { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string FotoUrl { get; set; } = string.Empty;
        public int Series { get; set; }
        public int Repeticoes { get; set; }
        public decimal? Carga { get; set; }
        public int Ordem { get; set; }
    }
}
