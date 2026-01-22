using System;

namespace FitControlAdmin.Models
{
    public class PaymentDisplayModel
    {
        public int IdPagamento { get; set; }
        public int IdMembro { get; set; }
        public string NomeMembro { get; set; } = default!;
        public string NomeSubscricao { get; set; } = default!;
        public DateTime DataPagamento { get; set; }
        public decimal ValorPago { get; set; }
        public string MetodoPagamento { get; set; } = default!;
        public string EstadoPagamento { get; set; } = default!;
        public DateTime MesReferente { get; set; }
        public DateTime DataRegisto { get; set; }
        public string StatusAtivo { get; set; } = "Ativo";
        public bool Ativo { get; set; }
    }
}
