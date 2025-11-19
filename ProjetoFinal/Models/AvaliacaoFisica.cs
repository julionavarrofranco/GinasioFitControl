namespace ProjetoFinal.Models
{
    public class AvaliacaoFisica
    {
        public int IdAvaliacao { get; set; } // PK

        public int IdMembro { get; set; } // FK

        public int IdFuncionario { get; set; } // FK

        public DateTime DataAvaliacao { get; set; }

        public decimal Peso { get; set; }

        public decimal Altura { get; set; }

        public decimal Imc { get; set; }

        public decimal MassaMuscular { get; set; }

        public decimal MassaGorda { get; set; }

        public string Observacoes { get; set; } = null!;

        public DateTime? DataDesativacao { get; set; }

        public Membro Membro { get; set; } = null!;

        public Funcionario Funcionario { get; set; } = null!;
    }
}
