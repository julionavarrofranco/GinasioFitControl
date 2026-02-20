using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;

namespace ProjetoFinal.Services.Interfaces
{
    public interface IScheduleClassService
    {
        Task<AulaMarcada> CreateAsync(ScheduleClassDto dto);

        Task<string> CancelByPtAsync(int idAulaMarcada);

        Task<List<ScheduledClassResponseDto>> ListAvailableAsync();
    }
}
