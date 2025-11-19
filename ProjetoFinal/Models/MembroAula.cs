namespace ProjetoFinal.Models
{
    public enum Presenca
    {
        Presente,
        Faltou,
        Cancelado
    }//vai servir como soft delete - se cancelar a reserva

    public class MembroAula
    {
        public int IdMembro { get; set; } // PK FK

        public int IdAula { get; set; } // PK FK

        public DateTime DataReserva { get; set; }

        public Presenca Presenca { get; set; }

        public Membro Membro { get; set; } = null!;

        public Aula Aula { get; set; } = null!;
    }
}
