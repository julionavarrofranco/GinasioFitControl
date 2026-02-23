using System.Text.Json.Serialization;

namespace FitControlAdmin.Models
{
    public enum DiaSemana
    {
        Segunda, Terca, Quarta, Quinta, Sexta, Sabado, Domingo
    }

    public enum Presenca
    {
        Reservado,
        Presente,
        Faltou,
        Cancelado
    }

    public class AulaDto
    {
        public int? IdFuncionario { get; set; }
        public string Nome { get; set; } = null!;
        public DiaSemana DiaSemana { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFim { get; set; }
        public int Capacidade { get; set; }
    }

    public class AulaResponseDto
    {
        public int IdAula { get; set; }
        public int? IdFuncionario { get; set; }
        public string Nome { get; set; } = null!;
        public DiaSemana DiaSemana { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFim { get; set; }
        public int Capacidade { get; set; }
        public DateTime? DataDesativacao { get; set; }
        public FuncionarioDto? Funcionario { get; set; }
    }

    public class AulaMarcadaDto
    {
        public int Id { get; set; }
        public int IdAula { get; set; }
        public DateTime DataAula { get; set; }
        public DateTime? DataDesativacao { get; set; }
    }

    public class ClassReservationSummaryDto
    {
        public int IdAulaMarcada { get; set; }
        public DateTime DataAula { get; set; }
        public string NomeAula { get; set; } = null!;
        public int Sala { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFim { get; set; }
        public int Capacidade { get; set; }
        public int TotalReservas { get; set; }
    }

    public class ClassAttendanceDto
    {
        public int IdAulaMarcada { get; set; }
        public DateTime DataAula { get; set; }
        public string NomeAula { get; set; } = null!;
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFim { get; set; }
        public int Capacidade { get; set; }
        public int TotalReservas => Reservas.Count;
        public List<MemberReservationDto> Reservas { get; set; } = new List<MemberReservationDto>();
    }

    public class UpdateClassDto
    {
        public int? IdFuncionario { get; set; }
        public string? Nome { get; set; }
        public TimeSpan? HoraInicio { get; set; }
        public TimeSpan? HoraFim { get; set; }
        public DiaSemana? DiaSemana { get; set; }
        public int? Capacidade { get; set; }
        public bool ForceSwap { get; set; } = false;
    }

    public class FuncionarioDto
    {
        public int IdUser { get; set; }
        public int IdFuncionario { get; set; }
        public string Nome { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Telemovel { get; set; } = null!;
        public Funcao Funcao { get; set; }
    }

    public class MemberReservationDto
    {
        public int IdMembro { get; set; }
        [JsonPropertyName("Nome")]
        public string NomeMembro { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Telemovel { get; set; } = null!;
        public Presenca Presenca { get; set; }
        public bool Presente 
        { 
            get => Presenca == Presenca.Presente; 
            set => Presenca = value ? Presenca.Presente : Presenca.Faltou; 
        }
        public DateTime? DataReserva { get; set; }
    }

    public class ScheduleClassDto
    {
        public int IdAula { get; set; }
        public int Sala { get; set; }
        public DateTime DataAula { get; set; }
    }

    public class AulaMarcadaResponseDto
    {
        public int IdAulaMarcada { get; set; }
        public int IdAula { get; set; }
        public string NomeAula { get; set; } = null!;
        public DateTime DataAula { get; set; }
        public int Sala { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFim { get; set; }
        public int Capacidade { get; set; }
        public int TotalReservas { get; set; }
        public string? NomeInstrutor { get; set; }
        public DateTime? DataDesativacao { get; set; }
        
        // Propriedades calculadas para a interface
        public string HorarioFormatado => $"{HoraInicio:hh\\:mm} - {HoraFim:hh\\:mm}";
        public string CapacidadeFormatada => $"{TotalReservas}/{Capacidade}";
        public bool EstaLotada => TotalReservas >= Capacidade;
    }
}