// ============================================================================
// MODELO DE PERFIL DO MEMBRO - VIEWMODEL PARA ÁREA DE MEMBROS
// Este modelo define a estrutura de dados para apresentação
// do perfil do membro na área privada
// ============================================================================

namespace TTFWebsite.Models
{
    /// <summary>
    /// ViewModel que representa o perfil completo de um membro.
    /// Utilizado para transferir dados do controlador para as vistas
    /// da área de membros (dashboard, perfil, etc.).
    /// </summary>
    public class MemberProfileViewModel
    {
        /// <summary>
        /// Nome completo do membro
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Endereço de email do membro (utilizado para login)
        /// </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Número de telefone ou telemóvel do membro
        /// </summary>
        public string Phone { get; set; } = string.Empty;
        
        /// <summary>
        /// Data de nascimento do membro
        /// </summary>
        public DateTime BirthDate { get; set; }
        
        /// <summary>
        /// Número de identificação único do membro no sistema
        /// </summary>
        public string MembershipNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Nome do ginásio a que o membro está associado
        /// </summary>
        public string Gym { get; set; } = string.Empty;
        
        /// <summary>
        /// Nome do plano de subscrição do membro (ex: "FitControl GO")
        /// </summary>
        public string Plan { get; set; } = string.Empty;
        
        /// <summary>
        /// Data de início da associação do membro ao ginásio
        /// </summary>
        public DateTime MembershipStartDate { get; set; }
    }
}
