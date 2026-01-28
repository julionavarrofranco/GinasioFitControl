namespace TTFWebsite.Models
{
    public class Class
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Instructor { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int MaxCapacity { get; set; }
        public int CurrentBookings { get; set; }
        public string Gym { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
    }
}

