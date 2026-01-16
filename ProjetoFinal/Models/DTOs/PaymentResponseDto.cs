namespace ProjetoFinal.Models.DTOs
{
    public class PaymentResponseDto
    {
        public int IdPagamento { get; set; }
        public decimal ValorPago { get; set; }
        public EstadoPagamento EstadoPagamento { get; set; }
        public DateTime MesReferente { get; set; }
        public DateTime DataPagamento { get; set; }

        public int IdMembro { get; set; }
        public string? NomeMembro { get; set; }

        public string? Subscricao { get; set; }
    }
}
