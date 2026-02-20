namespace ProjetoFinal.Models.DTOs
{
    public class UpdatePhysicalEvaluationDto
    {
        public decimal? Peso { get; set; }

        public decimal? Altura { get; set; }

        public decimal? Imc { get; set; }

        public decimal? MassaMuscular { get; set; }

        public decimal? MassaGorda { get; set; }

        public string? Observacoes { get; set; } 
    }
}
