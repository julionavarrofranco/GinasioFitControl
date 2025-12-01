// ============================================================================
// MODELO DE TESTEMUNHO - REPRESENTA UM TESTEMUNHO DE CLIENTE
// Este modelo define a estrutura de dados para os testemunhos
// de membros satisfeitos apresentados na página inicial
// ============================================================================

namespace TTFWebsite.Models
{
    /// <summary>
    /// Representa um testemunho de um membro do ginásio.
    /// Os testemunhos são utilizados para criar confiança junto
    /// de potenciais novos membros.
    /// </summary>
    public class Testimonial
    {
        /// <summary>
        /// Identificador único do testemunho
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Nome do membro que deu o testemunho
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Comentário/opinião do membro sobre o ginásio
        /// </summary>
        public string Comment { get; set; } = string.Empty;
        
        /// <summary>
        /// URL da fotografia do membro (opcional)
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;
    }
}
