namespace FitControlAdmin.Models
{
    // DTO para criar plano de treino
    public class TrainingPlanDto
    {
        public string Nome { get; set; } = null!;
        public string? Observacoes { get; set; }
    }

    // Resposta resumida de plano (lista por estado)
    public class TrainingPlanSummaryDto
    {
        public int IdPlano { get; set; }
        public string Nome { get; set; } = null!;
        public DateTime DataCriacao { get; set; }
        public bool Ativo { get; set; }
    }

    // DTO para atualizar plano de treino
    public class UpdateTrainingPlanDto
    {
        public string? Nome { get; set; }
        public string? Observacoes { get; set; }
    }

    // Detalhes completos do plano (com exercícios) para visualização e edição
    public class TrainingPlanDetailDto
    {
        public int IdPlano { get; set; }
        public int IdFuncionario { get; set; }
        public string Nome { get; set; } = null!;
        public DateTime DataCriacao { get; set; }
        public string? Observacoes { get; set; }
        public bool Ativo { get; set; }
        public string? NomeFuncionario { get; set; }
        public List<TrainingPlanExerciseDto> Exercicios { get; set; } = new();
    }

    // Exercício dentro de um plano (exibição)
    public class TrainingPlanExerciseDto
    {
        public int IdExercicio { get; set; }
        public string NomeExercicio { get; set; } = null!;
        public GrupoMuscular GrupoMuscular { get; set; }
        public string? Descricao { get; set; }
        public string? FotoUrl { get; set; }
        public int Series { get; set; }
        public int Repeticoes { get; set; }
        public decimal Carga { get; set; }
        public int Ordem { get; set; }
    }

    // DTO para adicionar exercício ao plano
    public class ExercisePlanDto
    {
        public int IdExercicio { get; set; }
        public int Series { get; set; }
        public int Repeticoes { get; set; }
        public decimal Carga { get; set; }
        public int? Ordem { get; set; }
    }

    // DTO para atualizar exercício no plano
    public class UpdateExercisePlanDto
    {
        public int? Series { get; set; }
        public int? Repeticoes { get; set; }
        public decimal? Carga { get; set; }
        public int? Ordem { get; set; }
    }

    // Plano de treino do membro (resposta current/history)
    public class MemberTrainingPlanDto
    {
        public string NomePlano { get; set; } = null!;
        public string? Observacoes { get; set; }
        public DateTime DataCriacao { get; set; }
        public string CriadoPor { get; set; } = null!;
        public List<TrainingPlanExerciseDto> Exercicios { get; set; } = new();
    }
}
