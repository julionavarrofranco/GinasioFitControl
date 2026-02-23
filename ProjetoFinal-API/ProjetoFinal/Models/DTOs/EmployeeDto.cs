namespace ProjetoFinal.Models.DTOs
{
    public class EmployeeDto
    {
        public int IdUser { get; set; }

        public int IdFuncionario { get; set; }

        public string Nome { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Telemovel { get; set; } = null!;

        public Funcao Funcao { get; set; }
    }
}
