namespace TTFWebsite.Models
{
    public class Plan
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<string> Features { get; set; } = new();
        public bool IsPopular { get; set; }
    }
}

