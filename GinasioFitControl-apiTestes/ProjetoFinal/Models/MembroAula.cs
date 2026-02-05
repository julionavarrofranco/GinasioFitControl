namespace ProjetoFinal.Models
{
    public enum Presenca
    {
        Reservado,
        Presente,
        Faltou,
        Cancelado
    }

    public class MembroAula
    {
        public int IdMembro { get; set; }

        public int IdAulaMarcada { get; set; }

        public DateTime DataReserva { get; set; }

        public Presenca Presenca { get; set; }

        public Membro Membro { get; set; } = null!;

        public AulaMarcada AulaMarcada { get; set; } = null!;
    }
}
