namespace ProjetoFinal.Models.DTOs
{
    /// <summary>
    /// DTO de resposta para POST /api/MemberClass/reserve.
    /// Evita serialização circular (MembroAula tem navegações AulaMarcada e Membro que geram ciclos).
    /// </summary>
    public class ReserveClassResponseDto
    {
        public int IdMembro { get; set; }
        public int IdAulaMarcada { get; set; }
        public DateTime DataReserva { get; set; }
        public string Presenca { get; set; } = "Reservado";
    }
}
