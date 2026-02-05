using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;

namespace ProjetoFinal.Services.Interfaces
{
    public interface IPhysicalEvaluationService
    {
        Task<AvaliacaoFisica> CreatePhysicalEvaluationAsync(PhysicalEvaluationDto request);
        Task<string> UpdatePhysicalEvaluationAsync(int idAvaliacao, UpdatePhysicalEvaluationDto request);
        Task ChangePhysicalEvaluationActiveStatusAsync(int idAvaliacao, bool ativo);
        Task<List<PhysicalEvaluationHistoryDto>> GetAllEvaluationsAsync();
        Task<List<AvaliacaoFisica>> GetAllEvaluationsFromMemberAsync(int idMembro);
        Task<AvaliacaoFisica?> GetLatestEvaluationFromMemberAsync(int idMembro);
    }
}
