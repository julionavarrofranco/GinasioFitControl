namespace ProjetoFinal.Models
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

    public class Pagamento
    {
        public int IdPagamento { get; set; }

        public int IdMembro { get; set; }

        public int IdSubscricao { get; set; }

        public DateTime DataPagamento { get; set; }

        public decimal ValorPago { get; set; }

        public MetodoPagamento MetodoPagamento { get; set; }

        public EstadoPagamento EstadoPagamento { get; set; }

        public DateTime MesReferente { get; set; }

        public DateTime DataRegisto { get; set; }

        public DateTime? DataDesativacao { get; set; }

        public Membro Membro { get; set; } = null!;

        public Subscricao Subscricao { get; set; } = null!;
    }
}
