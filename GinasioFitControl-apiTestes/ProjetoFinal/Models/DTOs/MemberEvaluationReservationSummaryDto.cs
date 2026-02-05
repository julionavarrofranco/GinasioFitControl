namespace ProjetoFinal.Models.DTOs
{
    public class MemberEvaluationReservationSummaryDto
    {
        public int IdMembroAvaliacao { get; set; }    // ID da reserva
        public int IdMembro { get; set; }             // ID do membro
        public string NomeMembro { get; set; } = null!; // Nome do membro
        public string? TelemovelMembro { get; set; }
        public DateTime DataReserva { get; set; }     // Data da reserva  // Estado da reserva
        public string EstadoString { get; set; } = null!;
        public int? IdAvaliacaoFisica { get; set; }   // Avaliação associada, se houver
        public DateTime? DataCancelamento { get; set; } // Data de cancelamento, se aplicável
        public DateTime? DataDesativacao { get; set; }  // Data de desativação, se aplicável

        // Campos resumidos da avaliação física, caso já tenha sido realizada
        public decimal? Peso { get; set; }
        public decimal? Altura { get; set; }
        public decimal? Imc { get; set; }
        public decimal? MassaMuscular { get; set; }
        public decimal? MassaGorda { get; set; }
        public string? Observacoes { get; set; }
        public DateTime? DataAvaliacao { get; set; }

        // Opcional: ID do funcionário que realizou a avaliação
        public int? IdFuncionario { get; set; }
        public string? NomeFuncionario { get; set; }
        
        // Indica se já tem avaliação física criada
        public bool TemAvaliacaoFisica { get; set; }
    }
}
