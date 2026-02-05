namespace ProjetoFinal.Models.DTOs
{
    public class UpdatePaymentDto
    {
        public decimal? ValorPago { get; set; }
        public int? IdSubscricao { get; set; } 
        public MetodoPagamento? MetodoPagamento { get; set; }
        public EstadoPagamento? EstadoPagamento { get; set; }
    }
}