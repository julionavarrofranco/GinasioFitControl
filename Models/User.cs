// ============================================================================
// MODELO DE UTILIZADOR - REPRESENTA UM MEMBRO/UTILIZADOR DO SISTEMA
// Este modelo define a estrutura de dados completa de um utilizador
// registado no sistema FitControl
// ============================================================================

namespace TTFWebsite.Models
{
    /// <summary>
    /// Representa um utilizador/membro registado no sistema FitControl.
    /// Contém todas as informações pessoais e de associação.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Identificador único do utilizador no sistema
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Endereço de email do utilizador (usado para login)
        /// </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Palavra-passe do utilizador
        /// NOTA: Em produção, nunca armazenar em texto simples!
        /// Utilizar sempre hash (bcrypt, Argon2, etc.)
        /// </summary>
        public string Password { get; set; } = string.Empty;
        
        /// <summary>
        /// Nome completo do utilizador
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Número de telefone ou telemóvel
        /// </summary>
        public string Phone { get; set; } = string.Empty;
        
        /// <summary>
        /// Data de nascimento do utilizador
        /// </summary>
        public DateTime BirthDate { get; set; }
        
        /// <summary>
        /// Nome do ginásio a que o utilizador está associado
        /// </summary>
        public string Gym { get; set; } = string.Empty;
        
        /// <summary>
        /// Número de membro único no sistema
        /// </summary>
        public string MembershipNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Data de início da associação ao ginásio
        /// </summary>
        public DateTime MembershipStartDate { get; set; }
        
        /// <summary>
        /// Nome do plano de subscrição ativo
        /// </summary>
        public string Plan { get; set; } = string.Empty;
    }
}
