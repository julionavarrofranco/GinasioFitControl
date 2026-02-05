using ProjetoFinal.Models;

namespace ProjetoFinal.Models.DTOs
{
    public class ClassResponseDto
    {
        public int IdAula { get; set; }
        public int? IdFuncionario { get; set; }
        public string Nome { get; set; } = null!;
        public DiaSemana DiaSemana { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFim { get; set; }
        public int Capacidade { get; set; }
        public DateTime? DataDesativacao { get; set; }
        
        // Incluir dados do funcionário sem navegação circular
        public FuncionarioSimpleDto? Funcionario { get; set; }
    }

    public class FuncionarioSimpleDto
    {
        public int IdUser { get; set; }
        public int IdFuncionario { get; set; }
        public string Nome { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Telemovel { get; set; } = null!;
        public Funcao Funcao { get; set; }
    }
}
