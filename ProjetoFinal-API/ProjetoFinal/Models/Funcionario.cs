namespace ProjetoFinal.Models
{
    public enum Funcao
    {
        Admin,
        Rececao,
        PT
    }

    public class Funcionario
    {
        public int IdFuncionario { get; set; } 

        public int IdUser { get; set; }

        public string Nome { get; set; } = null!;

        public string Telemovel { get; set; } = null!;

        public Funcao Funcao { get; set; }

        public User User { get; set; } = null!;

        public ICollection<PlanoTreino> PlanosTreino { get; set; } = new List<PlanoTreino>();

        public ICollection<Aula> Aulas { get; set; } = new List<Aula>();

        public ICollection<AvaliacaoFisica> AvaliacoesFisicas { get; set; } = new List<AvaliacaoFisica>();
    }
}
