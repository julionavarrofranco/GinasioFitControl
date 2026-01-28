namespace FitControlAdmin.Models
{
    /// <summary>
    /// DTO para criar plano de treino
    /// </summary>
    public class TrainingPlanDto
    {
        public string Nome { get; set; } = null!;
        public string? Observacoes { get; set; }
    }

    /// <summary>
    /// Resposta resumida de plano (lista por estado)
    /// </summary>
    public class TrainingPlanSummaryDto
    {
        public int IdPlano { get; set; }
        public string Nome { get; set; } = null!;
        public DateTime DataCriacao { get; set; }
        public bool Ativo { get; set; }
    }

    /// <summary>
    /// DTO para atualizar plano de treino
    /// </summary>
    public class UpdateTrainingPlanDto
    {
        public string? Nome { get; set; }
        public string? Observacoes { get; set; }
    }

    /// <summary>
    /// Detalhes completos do plano (com exercícios) - para visualização/edição
    /// </summary>
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

    /// <summary>
    /// Exercício dentro de um plano (para exibição)
    /// </summary>
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

    /// <summary>
    /// DTO para adicionar exercício ao plano
    /// </summary>
    public class ExercisePlanDto
    {
        public int IdExercicio { get; set; }
        public int Series { get; set; }
        public int Repeticoes { get; set; }
        public decimal Carga { get; set; }
        public int? Ordem { get; set; }
    }

    /// <summary>
    /// DTO para atualizar exercício no plano
    /// </summary>
    public class UpdateExercisePlanDto
    {
        public int? Series { get; set; }
        public int? Repeticoes { get; set; }
        public decimal? Carga { get; set; }
        public int? Ordem { get; set; }
    }

    /// <summary>
    /// Plano de treino do membro (resposta de current/history)
    /// </summary>
    public class MemberTrainingPlanDto
    {
        public string NomePlano { get; set; } = null!;
        public string? Observacoes { get; set; }
        public DateTime DataCriacao { get; set; }
        public string CriadoPor { get; set; } = null!;
        public List<TrainingPlanExerciseDto> Exercicios { get; set; } = new();
    }
}
