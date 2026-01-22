namespace FitControlAdmin.Models
{
    public enum MetodoPagamento
    {
        Cartao,
        MBWay,
        Dinheiro
    }

    public enum EstadoPagamento
    {
        Pendente,
        Pago,
        Cancelado
    }

    public class PaymentDto
    {
        public int IdMembro { get; set; }
        public int IdSubscricao { get; set; }
        public MetodoPagamento MetodoPagamento { get; set; }
        public DateTime MesReferente { get; set; }
    }

    public class UpdatePaymentDto
    {
        public decimal? ValorPago { get; set; }

        public int? IdSubscricao { get; set; }
        public MetodoPagamento? MetodoPagamento { get; set; }
        public EstadoPagamento? EstadoPagamento { get; set; }
    }



    public class PaymentResponseDto
    {
        public int IdPagamento { get; set; }
        public int IdMembro { get; set; }
        public DateTime DataPagamento { get; set; }
        public decimal ValorPago { get; set; }

        public string MetodoPagamento { get; set; } = default!;
        public string EstadoPagamento { get; set; } = default!;

        public DateTime MesReferente { get; set; }
        public DateTime DataRegisto { get; set; }
        public DateTime? DataDesativacao { get; set; }

        public int IdSubscricao { get; set; }

        public string Subscricao { get; set; } = default!;
    }


}
