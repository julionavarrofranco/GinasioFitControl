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

    /// <summary>
    /// Modelo para exibir funcionários na grid (inclui Ativo via User).
    /// DataRegisto: a API não expõe data de registo para funcionários; usar "—" quando não disponível.
    /// </summary>
    public class EmployeeDisplayDto
    {
        public int IdUser { get; set; }
        public int IdFuncionario { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telemovel { get; set; } = string.Empty;
        public Funcao Funcao { get; set; }
        public bool Ativo { get; set; }
        public DateTime? DataRegisto { get; set; }

        /// <summary>Para binding na grid; exibe "—" quando DataRegisto não existe.</summary>
        public string DataRegistoDisplay => DataRegisto.HasValue ? DataRegisto.Value.ToString("dd/MM/yyyy") : "—";
    }
}
