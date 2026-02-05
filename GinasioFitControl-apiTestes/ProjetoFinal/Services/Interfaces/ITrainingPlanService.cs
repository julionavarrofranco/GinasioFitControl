using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;

namespace ProjetoFinal.Services.Interfaces
{
    public interface ITrainingPlanService
    {
        Task<PlanoTreino> CreateAsync(int idFuncionario, TrainingPlanDto dto);
        Task<string> UpdateAsync(int idPlano, UpdateTrainingPlanDto dto);
        Task AtribuirPlanoAoMembroAsync(int idMembro, int idPlano);
        Task RemoverPlanoDoMembroAsync(int idMembro);
        Task ChangeActiveStateAsync(int idPlano, bool ativo);
        Task<PlanoTreino?> GetPlanoAtualDoMembroAsync(int idMembro);
        Task<List<PlanoTreino>> GetHistoricoPlanosDoMembroAsync(int idMembro);
        Task<List<PlanoTreino>> GetPlanosByEstadoAsync(bool ativo);
        Task<List<TrainingPlanSummaryDto>> GetPlanosResumoAsync(bool? ativo = null);
        Task<TrainingPlanDetailDto?> GetPlanoDetalheAsync(int idPlano);
    }
}
