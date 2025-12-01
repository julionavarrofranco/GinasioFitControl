namespace ProjetoFinal.Models.DTOs
{
    public class AdminChangePasswordDto
    {
        public string Email { get; set; } = null!;

        public string NovaPassword { get; set; } = null!;
    }
}

