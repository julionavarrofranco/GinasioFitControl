namespace ProjetoFinal.Models.DTOs
{
    public class MemberDto
    {
        public int IdUser { get; set; } 
        
        public int IdMembro { get; set; }   

        public string Nome { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Telemovel { get; set; } = null!;

        public DateTime DataNascimento { get; set; }

        public DateTime DataRegisto { get; set; }

        public string Subscricao { get; set; } = null!;

        public string PlanoTreino { get; set; } = null!;

        public string DataDesativacao { get; set; } = null!;

        public bool Ativo { get; set; }
    }
}


