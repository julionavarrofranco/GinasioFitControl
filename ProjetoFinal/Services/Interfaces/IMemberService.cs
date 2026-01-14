using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;

namespace ProjetoFinal.Services.Interfaces
{
    public interface IMemberService
    {
       Task<List<MemberDto>> GetAllMembersAsync();

       Task<Membro> CreateMemberAsync(int idUser, UserRegisterDto request);
        
       Task CancelMemberAsync(int idMembro);

       Task ReactivateMemberAsync(int idMembro, MetodoPagamento metodo);

       Task<string> UpdateMemberAsync(int idMembro, UpdateMemberDto request);
       Task<MemberProfileDto> GetMemberProfileAsync(int idMembro);

       Task<List<MemberPhysicalEvaluationDto>> GetMemberEvaluationsAsync(int idMembro);

       Task<MemberTrainingPlanDto?> GetMemberTrainingPlanAsync(int idMembro);
    }
}
