// ============================================================================
// MODELO DE AULA - REPRESENTA UMA AULA DE GRUPO DO GINÁSIO
// Este modelo define a estrutura de dados para as aulas disponíveis
// para reserva pelos membros
// ============================================================================

namespace TTFWebsite.Models
{
    /// <summary>
    /// Representa uma aula de grupo do ginásio FitControl.
    /// Inclui informações sobre horário, instrutor, sala e disponibilidade.
    /// </summary>
    public class Class
    {
        /// <summary>
        /// Identificador único da aula
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Nome da aula (ex: "Pilates", "Yoga", "HIIT", "Spinning")
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Descrição detalhada da aula e dos seus benefícios
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Nome do instrutor responsável pela aula
        /// </summary>
        public string Instructor { get; set; } = string.Empty;
        
        /// <summary>
        /// Data e hora de início da aula
        /// </summary>
        public DateTime StartTime { get; set; }
        
        /// <summary>
        /// Data e hora de fim da aula
        /// </summary>
        public DateTime EndTime { get; set; }
        
        /// <summary>
        /// Capacidade máxima de participantes na aula
        /// </summary>
        public int MaxCapacity { get; set; }
        
        /// <summary>
        /// Número atual de reservas para esta aula
        /// </summary>
        public int CurrentBookings { get; set; }
        
        /// <summary>
        /// Nome do ginásio onde a aula decorre
        /// </summary>
        public string Gym { get; set; } = string.Empty;
        
        /// <summary>
        /// Sala ou espaço onde a aula é realizada
        /// </summary>
        public string Room { get; set; } = string.Empty;
    }
}
