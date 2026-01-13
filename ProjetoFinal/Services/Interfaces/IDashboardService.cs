using ProjetoFinal.Models.DTOs;

namespace ProjetoFinal.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetSummaryAsync();
    }
}
