using ProjetoFinal.Models.DTOs;

namespace ProjetoFinal.Services
{
    public interface IMemberService
    {
       Task<List<MemberDto>> GetAllMembersAsync();
    }
}
