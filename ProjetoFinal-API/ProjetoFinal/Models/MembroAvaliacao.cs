namespace ProjetoFinal.Models
{
    public enum EstadoAvaliacao
    {
        Reservado,
        Presente,
        Faltou,
        Cancelado
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

        public Membro Membro { get; set; } = null!;

        public AvaliacaoFisica? AvaliacaoFisica { get; set; }
    }
}
