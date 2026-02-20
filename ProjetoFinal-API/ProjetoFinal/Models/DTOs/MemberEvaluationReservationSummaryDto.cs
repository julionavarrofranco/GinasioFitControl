namespace ProjetoFinal.Models.DTOs
{
    public class MemberEvaluationReservationSummaryDto
    {
        public int IdMembroAvaliacao { get; set; }   

        public int IdMembro { get; set; }            

        public string NomeMembro { get; set; } = null!; 

        public string? TelemovelMembro { get; set; }

        public DateTime DataReserva { get; set; }    

        public string EstadoString { get; set; } = null!;

        public int? IdAvaliacaoFisica { get; set; }  

        public DateTime? DataCancelamento { get; set; } 

        public DateTime? DataDesativacao { get; set; }  

        public decimal? Peso { get; set; }

        public decimal? Altura { get; set; }

        public decimal? Imc { get; set; }

        public decimal? MassaMuscular { get; set; }

        public decimal? MassaGorda { get; set; }

        public string? Observacoes { get; set; }

        public DateTime? DataAvaliacao { get; set; }

        public int? IdFuncionario { get; set; }

        public string? NomeFuncionario { get; set; }
        
        public bool TemAvaliacaoFisica { get; set; }
    }
}
