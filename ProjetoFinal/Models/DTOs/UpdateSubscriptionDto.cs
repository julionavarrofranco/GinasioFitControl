namespace ProjetoFinal.Models.DTOs
{
    public class UpdateSubscriptionDto
    {
        public string? Nome { get; set; } = null!;

        public TipoSubscricao? Tipo { get; set; }

        public decimal? Preco { get; set; }

        public string? Descricao { get; set; } = null!;
    }
}
