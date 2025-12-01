// ============================================================================
// MODELO DE RESERVA - REPRESENTA UMA RESERVA DE AULA
// Este modelo define a estrutura de dados para as reservas
// de aulas feitas pelos membros
// ============================================================================

namespace TTFWebsite.Models
{
    /// <summary>
    /// Representa uma reserva de aula feita por um membro.
    /// Liga o membro a uma aula específica numa determinada data.
    /// </summary>
    public class Reservation
    {
        /// <summary>
        /// Identificador único da reserva
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Identificador do membro que fez a reserva
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// Identificador da aula reservada
        /// </summary>
        public int ClassId { get; set; }
        
        /// <summary>
        /// Data e hora da aula reservada
        /// </summary>
        public DateTime ReservationDate { get; set; }
        
        /// <summary>
        /// Data e hora em que a reserva foi criada
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Indica se a reserva foi cancelada pelo membro
        /// </summary>
        public bool IsCancelled { get; set; }
        
        /// <summary>
        /// Referência para o objeto da aula reservada (navegação)
        /// Pode ser null se não for carregado (lazy loading)
        /// </summary>
        public Class? Class { get; set; }
    }
}
