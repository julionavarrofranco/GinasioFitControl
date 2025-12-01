namespace ProjetoFinal.Models.DTOs
{
    public class MemberProfileDto
    {
        public int IdUser { get; set; }
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public DateTime BirthDate { get; set; }
        public string MembershipNumber { get; set; } = string.Empty;
        public string Gym { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
        public DateTime MembershipStartDate { get; set; }
    }
}

