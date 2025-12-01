// ============================================================================
// MODELO DE AVALIAÇÃO FÍSICA - REPRESENTA UMA AVALIAÇÃO CORPORAL
// Este modelo define a estrutura de dados para as avaliações físicas
// realizadas pelos personal trainers aos membros
// ============================================================================

namespace TTFWebsite.Models
{
    /// <summary>
    /// Representa uma avaliação física realizada a um membro.
    /// Contém medidas corporais e notas do personal trainer.
    /// </summary>
    public class PhysicalAssessment
    {
        /// <summary>
        /// Identificador único da avaliação
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Identificador do membro avaliado
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// Data em que a avaliação foi realizada
        /// </summary>
        public DateTime AssessmentDate { get; set; }
        
        /// <summary>
        /// Peso do membro em quilogramas (kg)
        /// </summary>
        public decimal Weight { get; set; }
        
        /// <summary>
        /// Altura do membro em metros (m)
        /// </summary>
        public decimal Height { get; set; }
        
        /// <summary>
        /// Percentagem de gordura corporal (%)
        /// </summary>
        public decimal BodyFat { get; set; }
        
        /// <summary>
        /// Massa muscular do membro em quilogramas (kg)
        /// </summary>
        public decimal MuscleMass { get; set; }
        
        /// <summary>
        /// Índice de Massa Corporal (IMC/BMI)
        /// Calculado como: peso / altura²
        /// </summary>
        public decimal BMI { get; set; }
        
        /// <summary>
        /// Observações e recomendações do personal trainer
        /// </summary>
        public string Notes { get; set; } = string.Empty;
        
        /// <summary>
        /// Nome do personal trainer que realizou a avaliação
        /// </summary>
        public string TrainerName { get; set; } = string.Empty;
    }
}
