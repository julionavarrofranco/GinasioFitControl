using ProjetoFinal.Models;

namespace ProjetoFinal.Models.DTOs
{
    public class PhysicalEvaluationHistoryDto
    {
        public int IdAvaliacao { get; set; }

        public int IdMembro { get; set; }

        public string NomeMembro { get; set; } = null!;

        public string? TelemovelMembro { get; set; }

        public int IdFuncionario { get; set; }

        public string NomeFuncionario { get; set; } = null!;

        public DateTime DataAvaliacao { get; set; }

        public decimal Peso { get; set; }

        public decimal Altura { get; set; }

        public decimal Imc { get; set; }

        public decimal MassaMuscular { get; set; }

        public decimal MassaGorda { get; set; }

        public string? Observacoes { get; set; }

        public bool Ativo { get; set; }
    }
}
