namespace TTFWebsite.Models
{
    public class MemberProfileViewModel
    {
        public int IdMembro { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public DateTime BirthDate { get; set; }
        public DateTime MembershipStartDate { get; set; }
        public string Plan { get; set; } = "";
        public string? PersonalTrainer { get; set; }
    }


}

