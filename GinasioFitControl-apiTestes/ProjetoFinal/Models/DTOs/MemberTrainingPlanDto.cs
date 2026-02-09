namespace ProjetoFinal.Models.DTOs
{
    public class MemberTrainingPlanDto
    {
        public string NomePlano { get; set; } = null!;
        public string? Observacoes { get; set; }
        public DateTime DataCriacao { get; set; }
        public string CriadoPor { get; set; } = null!;
        public List<TrainingPlanExerciseDto> Exercicios { get; set; } = new();
    }

    public class TrainingPlanExerciseDto
    {
        public int IdExercicio { get; set; }
        public string NomeExercicio { get; set; } = null!;
        public string GrupoMuscular { get; set; } = null!;
        public string Descricao { get; set; } = null!;
        public string FotoUrl { get; set; } = null!;
        public int Series { get; set; }
        public int Repeticoes { get; set; }
        public decimal Carga { get; set; }
        public int Ordem { get; set; }
    }
}
