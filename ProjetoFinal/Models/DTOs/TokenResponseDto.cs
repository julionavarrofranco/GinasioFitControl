namespace ProjetoFinal.Models.DTOs
{
    public class TokenResponseDto
    {
        public string AccessToken { get; set; } = null!;

        public string RefreshToken { get; set; } = null!;
    }
}
