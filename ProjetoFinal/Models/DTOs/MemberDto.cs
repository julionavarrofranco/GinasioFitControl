namespace ProjetoFinal.Models.DTOs
{
    public class MemberDto
    {
        public int IdUser { get; set; }        
        public int IdMembro { get; set; }   
        public string Nome { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Telemovel { get; set; } = default!;
        public DateTime DataNascimento { get; set; }
        public DateTime DataRegisto { get; set; }
        public string Subscricao { get; set; } = default!;    // Nome da subscrição
        public string PlanoTreino { get; set; } = default!;   // Nome do plano
        public string DataDesativacao { get; set; } = "Ativo"; // "Ativo" ou data
        public bool Ativo { get; set; }
    }
}


