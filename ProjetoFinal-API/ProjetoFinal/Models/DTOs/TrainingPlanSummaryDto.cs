namespace ProjetoFinal.Models.DTOs
{
    public class TrainingPlanSummaryDto
    {
        public int IdPlano { get; set; }

        public string Nome { get; set; } = null!;

        public DateTime DataCriacao { get; set; }

        public bool Ativo { get; set; }
    }
}
