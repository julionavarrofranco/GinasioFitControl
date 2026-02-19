namespace TTFWebsite.Models
{
    public class TrainingPlanViewModel
    {
        public string Name { get; set; } = string.Empty;
        public List<Exercise> Exercises { get; set; } = new();
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string? Observations { get; set; }
    }

    public class Exercise
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string MuscleGroup { get; set; } = string.Empty;
        public int Sets { get; set; }
        public int Reps { get; set; }
        public string Notes { get; set; } = string.Empty;
        public decimal? Load { get; set; } // carga do exercício
        public string PhotoUrl { get; set; } = string.Empty; // url da foto ou ícone padrão

    }
}
