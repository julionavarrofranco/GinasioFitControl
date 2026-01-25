namespace FitControlAdmin.Models
{
    public class MemberEvaluationReservationSummaryDto
    {
        public int IdMembroAvaliacao { get; set; }    // ID da reserva
        public int IdMembro { get; set; }             // ID do membro
        public string NomeMembro { get; set; } = null!; // Nome do membro
        public string? TelemovelMembro { get; set; }
        public DateTime DataReserva { get; set; }     // Data da reserva
        public string EstadoString { get; set; } = null!; // Estado da reserva
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

    public class PhysicalEvaluationDto
    {
        public int IdMembro { get; set; }
        public int IdFuncionario { get; set; }
        public DateTime DataAvaliacao { get; set; }
        public decimal Peso { get; set; }
        public decimal Altura { get; set; }
        public decimal Imc { get; set; }
        public decimal MassaMuscular { get; set; }
        public decimal MassaGorda { get; set; }
        public string? Observacoes { get; set; }
    }

    public class UpdatePhysicalEvaluationDto
    {
        public decimal? Peso { get; set; }
        public decimal? Altura { get; set; }
        public decimal? Imc { get; set; }
        public decimal? MassaMuscular { get; set; }
        public decimal? MassaGorda { get; set; }
        public string? Observacoes { get; set; }
    }

    public class PhysicalEvaluationResponseDto
    {
        public int IdAvaliacao { get; set; }
        public int IdMembro { get; set; }
        public int IdFuncionario { get; set; }
        public DateTime DataAvaliacao { get; set; }
        public decimal Peso { get; set; }
        public decimal Altura { get; set; }
        public decimal Imc { get; set; }
        public decimal MassaMuscular { get; set; }
        public decimal MassaGorda { get; set; }
        public string Observacoes { get; set; } = null!;
        public DateTime? DataDesativacao { get; set; }
    }

    public class MarkAttendanceDto
    {
        public bool Presente { get; set; }
        public int IdFuncionario { get; set; }
        public decimal Peso { get; set; }
        public decimal Altura { get; set; }
        public decimal Imc { get; set; }
        public decimal MassaMuscular { get; set; }
        public decimal MassaGorda { get; set; }
        public string? Observacoes { get; set; }
    }

    public enum EstadoAvaliacao
    {
        Reservado,
        Presente,
        Cancelado,
        Faltou
    }

    public class MembroAvaliacao
    {
        public int IdMembroAvaliacao { get; set; }
        public int IdMembro { get; set; }
        public int? IdAvaliacaoFisica { get; set; }
        public DateTime DataReserva { get; set; }
        public EstadoAvaliacao Estado { get; set; }
        public DateTime? DataCancelamento { get; set; }
        public DateTime? DataDesativacao { get; set; }
        public Membro? Membro { get; set; }
        public AvaliacaoFisica? AvaliacaoFisica { get; set; }
    }

    public class Membro
    {
        public int IdMembro { get; set; }
        public int IdUser { get; set; }
        public string Nome { get; set; } = null!;
        public string Telemovel { get; set; } = null!;
        public int IdSubscricao { get; set; }
        public DateTime DataRegisto { get; set; }
        public bool Ativo { get; set; }
    }

    public class AvaliacaoFisica
    {
        public int IdAvaliacao { get; set; }
        public int IdMembro { get; set; }
        public int IdFuncionario { get; set; }
        public DateTime DataAvaliacao { get; set; }
        public decimal Peso { get; set; }
        public decimal Altura { get; set; }
        public decimal Imc { get; set; }
        public decimal MassaMuscular { get; set; }
        public decimal MassaGorda { get; set; }
        public string? Observacoes { get; set; }
        public DateTime? DataDesativacao { get; set; }
        public Funcionario? Funcionario { get; set; }
    }

    public class Funcionario
    {
        public int IdFuncionario { get; set; }
        public int IdUser { get; set; }
        public string Nome { get; set; } = null!;
        public string Telemovel { get; set; } = null!;
        public Funcao Funcao { get; set; }
        public bool Ativo { get; set; }
    }

    public enum Funcao
    {
        Admin,
        Rececao,
        PT
    }

    public class PhysicalEvaluationReservationResponseDto
    {
        public int IdAvaliacao { get; set; }
        public int IdMembro { get; set; }
        public int? IdFuncionario { get; set; }
        public DateTime DataAvaliacao { get; set; }
        public string Estado { get; set; } = null!;
        public string? NomeMembro { get; set; }
        public string? NomeFuncionario { get; set; }
    }

    public class PhysicalEvaluationHistoryDto
    {
        public int IdAvaliacao { get; set; }
        public int IdMembro { get; set; }
        public string NomeMembro { get; set; } = null!;
        public string? TelemovelMembro { get; set; }
        public int IdFuncionario { get; set; }
        public string NomeFuncionario { get; set; } = null!;
        public DateTime DataAvaliacao { get; set; }
        public decimal Peso { get; set; }
        public decimal Altura { get; set; }
        public decimal Imc { get; set; }
        public decimal MassaMuscular { get; set; }
        public decimal MassaGorda { get; set; }
        public string? Observacoes { get; set; }
        public bool Ativo { get; set; }
    }
}
