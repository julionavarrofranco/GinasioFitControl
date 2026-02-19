namespace TTFWebsite.Models.DTOs
{
    public class SubscriptionDto
    {
        public int IdSubscricao { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public decimal Preco { get; set; }
        public string Tipo { get; set; } = string.Empty; // Mensal, Trimestral, Anual
    }
}
