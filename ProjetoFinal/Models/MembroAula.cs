namespace ProjetoFinal.Models
{
    public enum Presenca
    {
        Presente,
        Faltou,
        Cancelado
    }

    public class MembroAula
    {
        public int IdMembro { get; set; }

        public int IdAula { get; set; }

        public DateTime DataReserva { get; set; }

        public Presenca Presenca { get; set; }

        public Membro Membro { get; set; } = null!;

        public Aula Aula { get; set; } = null!;
    }
}
