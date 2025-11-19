namespace ProjetoFinal.Models
{
    public class PlanoTreino
    {
        public int IdPlano { get; set; } // PK

        public int? IdMembro { get; set; } // FK Membro

        public int IdFuncionario { get; set; } // FK PT

        public DateTime DataCriacao { get; set; }

        public string Observacoes { get; set; } = null!;

        public DateTime? DataDesativacao { get; set; }

        public Membro Membro { get; set; } = null!;

        public Funcionario Funcionario { get; set; } = null!;

        public ICollection<PlanoExercicio> PlanosExercicios { get; set; } = new List<PlanoExercicio>();

        public ICollection<Membro> Membros { get; set; } = new List<Membro>();
    }
}
