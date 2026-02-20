namespace ProjetoFinal.Models.DTOs
{
    public class PaymentResponseDto
    {
        public int IdPagamento { get; set; }

        public int IdSubscricao { get; set; }

        public decimal ValorPago { get; set; }

        public string EstadoPagamento { get; set; } = null!;

        public DateTime MesReferente { get; set; }

        public DateTime DataPagamento { get; set; }

        public string MetodoPagamento { get; set; } = null!;

        public int IdMembro { get; set; }

        public string? NomeMembro { get; set; }

        public string? Subscricao { get; set; }
    }
}
