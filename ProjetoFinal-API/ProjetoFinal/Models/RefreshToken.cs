namespace ProjetoFinal.Models
{
    public class RefreshToken
    {
        public int IdRefresh { get; set; }

        public int IdUser { get; set; }

        public string Token { get; set; } = null!; // hash do token

        public string? SubstituidoPor { get; set; }

        public DateTime Validade { get; set; }

        public bool Cancelado { get; set; }

        public User User { get; set; } = null!;
    }
}
