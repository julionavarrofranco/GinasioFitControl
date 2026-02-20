namespace ProjetoFinal.Models.DTOs
{
    public class ChangePasswordDto
    {
        public string PasswordAtual { get; set; } = null!;
        
        public string NovaPassword { get; set; } = null!;
    }
}
