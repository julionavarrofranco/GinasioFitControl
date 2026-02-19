namespace TTFWebsite.Models
{
    public class PhysicalAssessment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime AssessmentDate { get; set; }
        public decimal Weight { get; set; }
        public decimal Height { get; set; }
        public decimal BodyFat { get; set; }
        public decimal MuscleMass { get; set; }
        public decimal BMI { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string TrainerName { get; set; } = string.Empty;
    }
}

