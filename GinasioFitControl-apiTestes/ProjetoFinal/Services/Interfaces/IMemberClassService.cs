using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;

namespace ProjetoFinal.Services.Interfaces
{
    public interface IMemberClassService
    {
        Task<MembroAula> ReservarAsync(int idMembro, int idAulaMarcada);
        Task<string> CancelarReservaAsync(int idMembro, int idAulaMarcada);
        Task<List<ClassReservationDto>> ListarReservasDoMembroAsync(int idMembro);
        Task<string> MarcarPresencasAsync(int idAulaMarcada, List<int> idsMembrosPresentes);
        Task<ClassAttendanceDto> ObterAulaParaPresencaAsync(int idAulaMarcada);
        Task<List<ClassReservationSummaryDto>> ListarTodasReservasAsync();
        Task<List<ClassReservationSummaryDto>> ListarReservasPorPtAsync(int idFuncionario);
    }
}
