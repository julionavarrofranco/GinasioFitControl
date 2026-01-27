using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;

namespace ProjetoFinal.Services.Interfaces
{
    public interface IPhysicalEvaluationReservationService
    {
        Task<MembroAvaliacao> CreateReservationAsync(int idMembro, DateTime dataReserva);
        Task<bool> CancelReservationAsync(int idMembro, int idAvaliacao);
        Task<bool> ConfirmReservationAsync(int idMembro, int idAvaliacao, int idFuncionario);
        Task<bool> MarkAttendanceAsync(int idMembro, int idAvaliacao, MarkAttendanceDto request);
        Task<List<MemberEvaluationReservationSummaryDto>> GetReservationsAsync();
        Task<List<MemberEvaluationReservationSummaryDto>> GetCompletedReservationsAsync();
    }
}
