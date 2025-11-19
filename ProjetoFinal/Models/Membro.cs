namespace ProjetoFinal.Models
{
    public class Membro
    {
        public int IdMembro { get; set; } // PK

        public int IdUser { get; set; } // FK User

        public string Nome { get; set; } = null!;

        public string Telemovel { get; set; } = null!;

        public DateTime DataNascimento { get; set; }

        public DateTime DataRegisto { get; set; }

        public int IdSubscricao { get; set; } // FK Subscricao

        public int? IdPlanoTreino { get; set; }

        public DateTime? DataDesativacao { get; set; } // soft delete

        public User User { get; set; } = null!;

        public Subscricao Subscricao { get; set; } = null!;

        public PlanoTreino? PlanoTreino { get; set; }

        public ICollection<PlanoTreino> Planos { get; set; } = new List<PlanoTreino>();

        public ICollection<MembroAula> MembroAulas { get; set; } = new List<MembroAula>();

        public ICollection<AvaliacaoFisica> AvaliacoesFisicas { get; set; } = new List<AvaliacaoFisica>();

        public ICollection<Pagamento> Pagamentos { get; set; } = new List<Pagamento>();
    }
}
