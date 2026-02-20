namespace ProjetoFinal.Models.DTOs
{
    public class PaymentDto
    {
        public int IdMembro { get; set; }

        public int IdSubscricao { get; set; }

        public MetodoPagamento MetodoPagamento { get; set; }

        public DateTime MesReferente { get; set; }
    }
}
