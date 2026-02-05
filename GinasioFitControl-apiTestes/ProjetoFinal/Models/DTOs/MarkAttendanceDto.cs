namespace ProjetoFinal.Models.DTOs
{
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
}
