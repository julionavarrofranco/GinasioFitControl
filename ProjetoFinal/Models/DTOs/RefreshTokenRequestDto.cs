namespace ProjetoFinal.Models.DTOs
{
    public class RefreshTokenRequestDto
    {
        public int IdUser { get; set; }

        public string RefreshToken { get; set; } = null!;
    }
}
