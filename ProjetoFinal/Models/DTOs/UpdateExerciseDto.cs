namespace ProjetoFinal.Models.DTOs
{
    public class UpdateExerciseDto
    {
        public string? Nome { get; set; } = null!;

        public string? Descricao { get; set; } = null!;

        public string? FotoUrl { get; set; } = null!;

        public GrupoMuscular? GrupoMuscular { get; set; }
    }
}
