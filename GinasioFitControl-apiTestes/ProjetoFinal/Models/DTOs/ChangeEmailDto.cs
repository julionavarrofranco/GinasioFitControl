namespace ProjetoFinal.Models.DTOs
{
    public class ChangeEmailDto
    {
        public int IdUser { get; set; }
        public string NewEmail { get; set; } = null!;
    }
}
