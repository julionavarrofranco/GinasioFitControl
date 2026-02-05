using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;

namespace ProjetoFinal.Services.Interfaces
{
    public interface IExerciseService
    {
        Task<Exercicio> CreateExerciseAsync(ExerciseDto request);

        Task<string> UpdateExerciseAsync(int idExercicio, UpdateExerciseDto request);

        Task ChangeExerciseActiveStatusAsync(int idExercicio, bool ativo);

        Task<List<Exercicio>> GetExercisesByStateAsync(bool ativo, bool ordenarAsc = true);

        Task<List<Exercicio>> GetExercisesByMuscleGroupAsync(GrupoMuscular grupo, bool ordenarAsc = true);

        Task<List<Exercicio>> GetExercisesByNameAsync(string nome, bool ordenarAsc = true);
    }
}
