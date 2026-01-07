namespace FitControlAdmin.Models
{
    public class PhysicalEvaluationDto
    {
        public int IdMembro { get; set; }
        public int IdFuncionario { get; set; }
        public DateTime DataAvaliacao { get; set; }
        public decimal Peso { get; set; }
        public decimal Altura { get; set; }
        public decimal Imc { get; set; }
        public decimal MassaMuscular { get; set; }
        public decimal MassaGorda { get; set; }
        public string? Observacoes { get; set; }
    }

    public class UpdatePhysicalEvaluationDto
    {
        public decimal? Peso { get; set; }
        public decimal? Altura { get; set; }
        public decimal? Imc { get; set; }
        public decimal? MassaMuscular { get; set; }
        public decimal? MassaGorda { get; set; }
        public string? Observacoes { get; set; }
    }

    public class PhysicalEvaluationResponseDto
    {
        public int IdAvaliacao { get; set; }
        public int IdMembro { get; set; }
        public int IdFuncionario { get; set; }
        public DateTime DataAvaliacao { get; set; }
        public decimal Peso { get; set; }
        public decimal Altura { get; set; }
        public decimal Imc { get; set; }
        public decimal MassaMuscular { get; set; }
        public decimal MassaGorda { get; set; }
        public string Observacoes { get; set; } = null!;
        public DateTime? DataDesativacao { get; set; }
    }

    public class MarkAttendanceDto
    {
        public bool Presente { get; set; }
        public int IdFuncionario { get; set; }
        public decimal Peso { get; set; }
        public decimal Altura { get; set; }
        public decimal Imc { get; set; }
        public decimal MassaMuscular { get; set; }
        public decimal MassaGorda { get; set; }
        public string? Observacoes { get; set; }
    }

    public class PhysicalEvaluationReservationResponseDto
    {
        public int IdAvaliacao { get; set; }
        public int IdMembro { get; set; }
        public int? IdFuncionario { get; set; }
        public DateTime DataAvaliacao { get; set; }
        public string Estado { get; set; } = null!;
        public string? NomeMembro { get; set; }
        public string? NomeFuncionario { get; set; }
    }
}

