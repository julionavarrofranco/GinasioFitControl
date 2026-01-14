namespace FitControlAdmin.Models
{
    public enum GrupoMuscular
    {
        Peito,
        Costas,
        Ombros,
        Bracos,
        Pernas,
        Abdominais,
        CorpoInteiro
    }

    public class ExerciseDto
    {
        public string Nome { get; set; } = null!;
        public string Descricao { get; set; } = null!;
        public string FotoUrl { get; set; } = null!;
        public GrupoMuscular GrupoMuscular { get; set; }
    }

    public class UpdateExerciseDto
    {
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public string? FotoUrl { get; set; }
        public GrupoMuscular? GrupoMuscular { get; set; }
    }

    public class ExerciseResponseDto
    {
        public int IdExercicio { get; set; }
        public string Nome { get; set; } = null!;
        public GrupoMuscular GrupoMuscular { get; set; }
        public string Descricao { get; set; } = null!;
        public string FotoUrl { get; set; } = null!;
        public bool Ativo { get; set; }
    }
}

