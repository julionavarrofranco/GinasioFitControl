using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Data;
using ProjetoFinal.Models.DTOs;

namespace ProjetoFinal.Services
{
    public class MemberService: IMemberService
    {
        private readonly GinasioDbContext _context;

        public MemberService(GinasioDbContext context)
        {
            _context = context;
        }

        public async Task<List<MemberDto>> GetAllMembersAsync()
        {
            var membros = await _context.Membros
                .Include(m => m.User)           // para pegar Email
                .Include(m => m.Subscricao)     // para pegar nome da subscrição
                .Include(m => m.PlanoTreino)    // para pegar nome do plano de treino
                .Select(m => new MemberDto
                {
                    IdUser = m.IdUser,
                    IdMembro = m.IdMembro,
                    Nome = m.Nome,
                    Email = m.User.Email,
                    Telemovel = m.Telemovel,
                    DataNascimento = m.DataNascimento,
                    DataRegisto = m.DataRegisto,
                    Subscricao = m.Subscricao.Descricao, //mudar depois para nome tenho que adicionar nome a tabela subscrição
                    PlanoTreino = m.PlanoTreino != null ? m.PlanoTreino.Observacoes : "Não definido", //mudar depois para nome tenho que adicionar nome a tabela plano treino
                    DataDesativacao = m.DataDesativacao.HasValue
                                        ? m.DataDesativacao.Value.ToString("dd/MM/yyyy")
                                        : "Ativo",
                    Ativo = m.User.Ativo
                })
                .ToListAsync();

            return membros;
        }

    }
}
