namespace FitControlAdmin.Models
{

    public class EmployeeDto
    {
        public int IdUser { get; set; }
        public int IdFuncionario { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telemovel { get; set; } = string.Empty;
        public Funcao Funcao { get; set; }

    }
}
