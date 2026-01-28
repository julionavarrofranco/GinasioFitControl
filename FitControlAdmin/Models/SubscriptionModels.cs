namespace FitControlAdmin.Models
{
    public enum TipoSubscricao
    {
        Mensal,
        Trimestral,
        Anual
    }

    public class SubscriptionResponseDto
    {
        public int IdSubscricao { get; set; }
        public string Nome { get; set; } = null!;
        public TipoSubscricao Tipo { get; set; }
        public decimal Preco { get; set; }
        public string? Descricao { get; set; }
        public bool Ativo { get; set; }
    }

    public class UpdateSubscriptionDto
    {
        public string? Nome { get; set; }
        public TipoSubscricao? Tipo { get; set; }
        public decimal? Preco { get; set; }

    }

    public class CreateSubscriptionDto
    {
        public string Nome { get; set; } = null!;
        public TipoSubscricao Tipo { get; set; }
        public decimal Preco { get; set; }
    }
}
