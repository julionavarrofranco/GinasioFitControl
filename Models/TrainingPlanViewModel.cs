namespace TTFWebsite.Models
{
    public class TrainingPlanViewModel
    {
        public string Name { get; set; } = string.Empty;
        public List<Exercise> Exercises { get; set; } = new();
    }

    public class Exercise
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Sets { get; set; }
        public int Reps { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
