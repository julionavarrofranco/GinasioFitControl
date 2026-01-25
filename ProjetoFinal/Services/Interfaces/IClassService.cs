using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;

namespace ProjetoFinal.Services.Interfaces
{
    public interface IClassService
    {
        Task<Aula> CreateAsync(ClassDto dto);
        Task<string> UpdateAsync(int idAula, UpdateClassDto dto);
        Task SwapClassSlotsAsync(int idAulaA, int idAulaB);
        Task<Aula> AssignPtAsync(int idAula, int idPt);
        Task ChangeActiveStateAsync(int idAula, bool ativo);
        Task<Aula> GetByIdAsync(int idAula);
        Task<List<ClassResponseDto>> ListByStateAsync(bool ativo);
        Task<List<ClassResponseDto>> ListByDayAsync(DiaSemana dia);
        Task<List<ClassResponseDto>> ListByPtAsync(int idFuncionario);
    }
}
