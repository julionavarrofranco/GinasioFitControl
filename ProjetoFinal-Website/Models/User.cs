namespace TTFWebsite.Models
{
    /// <summary>
    /// Representa um usuário do sistema.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Obtém ou define o identificador único do usuário.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Obtém ou define o identificador do membro associado a este usuário, se disponível.
        /// </summary>
        // Id do Membro (quando disponível via API)
        public int? IdMembro { get; set; }
        
        /// <summary>
        /// Obtém ou define o endereço de email do usuário.
        /// </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Obtém ou define a senha do usuário. Em produção, deve-se utilizar um hash da senha.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Obtém ou define o nome do usuário.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Obtém ou define o número de telefone do usuário.
        /// </summary>
        public string Phone { get; set; } = string.Empty;
        
        /// <summary>
        /// Obtém ou define a data de nascimento do usuário.
        /// </summary>
        public DateTime BirthDate { get; set; }
        
        /// <summary>
        /// Obtém ou define a academia do usuário.
        /// </summary>
        public string Gym { get; set; } = string.Empty;
        
        /// <summary>
        /// Obtém ou define o número da matrícula do usuário.
        /// </summary>
        public string MembershipNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Obtém ou define a data de início da matrícula do usuário.
        /// </summary>
        public DateTime MembershipStartDate { get; set; }
        
        /// <summary>
        /// Obtém ou define o plano do usuário.
        /// </summary>
        public string Plan { get; set; } = string.Empty;
    }
}

