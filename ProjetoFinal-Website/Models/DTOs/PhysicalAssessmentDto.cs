using System;

namespace TTFWebsite.Models.DTOs
{
    public class PhysicalAssessmentDto
    {
        public DateTime dataAvaliacao { get; set; }
        public decimal peso { get; set; }
        public decimal altura { get; set; }
        public decimal imc { get; set; }
        public decimal massaMuscular { get; set; }
        public decimal massaGorda { get; set; }
        public string observacoes { get; set; } = string.Empty;
        public string avaliador { get; set; } = string.Empty;
    }
}
