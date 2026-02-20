using System.Text.Json.Serialization;

namespace ProjetoFinal.Models.DTOs
{
    public class TokenResponseDto
    {
        public string AccessToken { get; set; } = null!;

        public string RefreshToken { get; set; } = null!;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool NeedsPasswordChange { get; set; } = false; // Campo para indicar a necessidade de mudar a palavra-passe

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Message { get; set; } // Mensagem opcional para explicar o motivo
    }
}
