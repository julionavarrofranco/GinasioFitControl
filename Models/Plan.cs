// ============================================================================
// MODELO DE PLANO - REPRESENTA UM PLANO DE SUBSCRIÇÃO DO GINÁSIO
// Este modelo define a estrutura de dados para os planos de adesão
// apresentados na página de preços
// ============================================================================

namespace TTFWebsite.Models
{
    /// <summary>
    /// Representa um plano de subscrição do ginásio FitControl.
    /// Contém informações sobre preço, descrição e funcionalidades incluídas.
    /// </summary>
    public class Plan
    {
        /// <summary>
        /// Identificador único do plano
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Nome do plano (ex: "FitControl Plus", "FitControl GO")
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Preço mensal do plano em euros (€)
        /// </summary>
        public decimal Price { get; set; }
        
        /// <summary>
        /// Descrição resumida do plano
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Lista de funcionalidades incluídas no plano
        /// (ex: "Débito Direto", "Plano de treino", etc.)
        /// </summary>
        public List<string> Features { get; set; } = new();
        
        /// <summary>
        /// Indica se este é o plano mais popular/recomendado
        /// Usado para destacar visualmente na página de preços
        /// </summary>
        public bool IsPopular { get; set; }
    }
}
