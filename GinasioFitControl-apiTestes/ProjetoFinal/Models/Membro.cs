namespace ProjetoFinal.Models
{
    public class Membro
    {
        public int IdMembro { get; set; }

        public int IdUser { get; set; }

        public string Nome { get; set; } = null!;

        public string Telemovel { get; set; } = null!;

        public DateTime DataNascimento { get; set; }

        public DateTime DataRegisto { get; set; }

        public int IdSubscricao { get; set; }

        public int? IdPlanoTreino { get; set; }

        public User User { get; set; } = null!;

        public Subscricao Subscricao { get; set; } = null!;

        public PlanoTreino? PlanoTreino { get; set; }

        public ICollection<PlanoTreino> Planos { get; set; } = new List<PlanoTreino>();

        public ICollection<MembroAula> MembroAulas { get; set; } = new List<MembroAula>();

        public ICollection<AvaliacaoFisica> AvaliacoesFisicas { get; set; } = new List<AvaliacaoFisica>();

        public ICollection<MembroAvaliacao> MembroAvaliacoes { get; set; } = new List<MembroAvaliacao>();

        public ICollection<Pagamento> Pagamentos { get; set; } = new List<Pagamento>();
    }
}
