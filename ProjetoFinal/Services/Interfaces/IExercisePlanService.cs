using ProjetoFinal.Models.DTOs;

namespace ProjetoFinal.Services.Interfaces
{
    public interface IExercisePlanService
    {
        Task AddAsync(int idPlano, ExercisePlanDto dto);
        Task<string> UpdateAsync(int idPlano, int idExercicio, UpdateExercisePlanDto dto);
        Task DeleteAsync(int idPlano, int idExercicio);
    }
}
