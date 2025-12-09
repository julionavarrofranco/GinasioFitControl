using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;

namespace ProjetoFinal.Services
{
    public interface IMemberService
    {
       Task<List<MemberDto>> GetAllMembersAsync();

       Task<Membro> CreateMemberAsync(int idUser, UserRegisterDto request);

       Task<string> UpdateMemberAsync(int idMembro, UpdateMemberDto request);
    }
}
