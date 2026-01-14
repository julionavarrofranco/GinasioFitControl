namespace FitControlAdmin.Models
{
    public class MemberPaymentDisplayModel
    {
        public int IdUser { get; set; }
        public int IdMembro { get; set; }
        public string Nome { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Telemovel { get; set; } = default!;
        public DateTime DataNascimento { get; set; }
        public DateTime DataRegisto { get; set; }
        public string Subscricao { get; set; } = default!;
        public string PlanoTreino { get; set; } = default!;
        public string DataDesativacao { get; set; } = "Ativo";
        public bool Ativo { get; set; }
        public string MetodoPagamento { get; set; } = "-";
    }
}
