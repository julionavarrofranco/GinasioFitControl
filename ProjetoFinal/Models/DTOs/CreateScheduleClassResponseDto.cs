namespace ProjetoFinal.Models.DTOs
{
    /// <summary>
    /// DTO de resposta para POST /api/ScheduleClass/create.
    /// Evita serialização circular (entidade AulaMarcada tem navegações Aula e MembrosAulas).
    /// </summary>
    public class CreateScheduleClassResponseDto
    {
        public int Id { get; set; }
        public int IdAula { get; set; }
        public DateTime DataAula { get; set; }
    }
}
