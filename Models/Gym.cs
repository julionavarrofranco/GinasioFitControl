// ============================================================================
// MODELO DE GINÁSIO - REPRESENTA UM GINÁSIO FITCONTROL
// Este modelo define a estrutura de dados para os ginásios
// apresentados na página inicial
// ============================================================================

namespace TTFWebsite.Models
{
    /// <summary>
    /// Representa um ginásio da rede FitControl.
    /// Contém informações sobre localização e contacto.
    /// </summary>
    public class Gym
    {
        /// <summary>
        /// Identificador único do ginásio
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Nome completo do ginásio (ex: "FitControl Lisboa")
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Cidade onde o ginásio está localizado
        /// </summary>
        public string City { get; set; } = string.Empty;
        
        /// <summary>
        /// Morada completa do ginásio
        /// </summary>
        public string Address { get; set; } = string.Empty;
        
        /// <summary>
        /// URL da imagem do ginásio (para galeria ou mapa)
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;
    }
}
