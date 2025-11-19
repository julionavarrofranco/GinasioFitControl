namespace ProjetoFinal.Models
{
    public class RefreshToken
    {
        public int IdRefresh { get; set; } // PK

        public int IdUser { get; set; } // FK User

        public string Token { get; set; } = null!; // refresh token

        public string? SubstituidoPor { get; set; }

        public DateTime Validade { get; set; }

        public bool Cancelado { get; set; }

        public User User { get; set; } = null!;
    }
}
