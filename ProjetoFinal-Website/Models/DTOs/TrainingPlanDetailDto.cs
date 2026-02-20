namespace TTFWebsite.Models.DTOs
{
    public class TrainingPlanDetailDto
    {
        public int IdPlano { get; set; }
        public int IdFuncionario { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Observacoes { get; set; }
        public DateTime DataCriacao { get; set; }
        public bool Ativo { get; set; }
        public string? NomeFuncionario { get; set; }
        public List<TrainingPlanExerciseDto> Exercicios { get; set; } = new();
    }

}
