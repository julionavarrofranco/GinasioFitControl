// ============================================================================
// MODELO DE BENEFÍCIO - REPRESENTA UM BENEFÍCIO DO GINÁSIO
// Este modelo define a estrutura de dados para os benefícios apresentados
// na página inicial do website
// ============================================================================

namespace TTFWebsite.Models
{
    /// <summary>
    /// Representa um benefício oferecido pelo ginásio FitControl.
    /// Os benefícios são apresentados na secção de vantagens da página inicial.
    /// </summary>
    public class Benefit
    {
        /// <summary>
        /// Identificador único do benefício
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Título do benefício (ex: "Horário Alargado", "Sem Fidelização")
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// Descrição detalhada do benefício
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Classe CSS do ícone associado ao benefício (para apresentação visual)
        /// </summary>
        public string IconClass { get; set; } = string.Empty;
    }
}
