namespace ProjetoFinal.Models.DTOs
{
    public class ChangePasswordDto
    {
        public int IdUser { get; set; }

        public string PasswordAtual { get; set; } = null!;
        
        public string NovaPassword { get; set; } = null!;
    }
}
