namespace TTFWebsite.Models
{
    public enum ReservationType
    {
        Class = 0,
        PhysicalAssessment = 1
    }
    
    public class Reservation
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? ClassId { get; set; }
        public int? AssessmentId { get; set; } // novo campo para ligação 
        public DateTime ReservationDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsCancelled { get; set; }
        public ReservationType Type { get; set; }
        public Class? Class { get; set; }
    }
}

