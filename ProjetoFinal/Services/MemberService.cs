using Azure.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Data;
using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;
using System.Text.RegularExpressions;

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
            return await _context.Membros
                .AsNoTracking()
                .Select(m => new MemberDto
                {
                    IdUser = m.IdUser,
                    IdMembro = m.IdMembro,
                    Nome = m.Nome,
                    Email = m.User.Email,
                    Telemovel = m.Telemovel,
                    DataNascimento = m.DataNascimento,
                    DataRegisto = m.DataRegisto,
                    Subscricao = m.Subscricao != null ? m.Subscricao.Nome : "Sem subscrição",
                    PlanoTreino = m.PlanoTreino != null ? m.PlanoTreino.Nome : "Sem plano",
                    DataDesativacao = m.User.Ativo ? "Ativo" : (m.User.DataDesativacao.HasValue
                                                   ? m.User.DataDesativacao.Value.ToString("dd/MM/yyyy") : "Indefinido"),
                    Ativo = m.User.Ativo
                })
                .ToListAsync();
        }

        public async Task<Membro> CreateMemberAsync(int idUser, UserRegisterDto request)
        {
            await ValidateMember(request.DataNascimento, request.IdSubscricao);
            var dataNascimento = request.DataNascimento!.Value;
            var idSubscricao = request.IdSubscricao!.Value;

            var membro = new Membro
            {
                IdUser = idUser,
                Nome = request.Nome,
                Telemovel = request.Telemovel,
                DataNascimento = dataNascimento,
                IdSubscricao = idSubscricao,
                DataRegisto = DateTime.UtcNow
            };

            _context.Membros.Add(membro);    
            return membro;
        }

        public async Task<string> UpdateMemberAsync(int idMembro, UpdateMemberDto request)
        {
            var membro = await _context.Membros.FirstOrDefaultAsync(m => m.IdMembro == idMembro);

            if (membro == null)
                throw new KeyNotFoundException("Membro não encontrado.");

            bool alterado = false;

            if (!string.IsNullOrWhiteSpace(request.Telemovel))
            {
                var phoneRegex = new Regex(@"^\+\d{7,15}$");
                if (!phoneRegex.IsMatch(request.Telemovel))
                    throw new InvalidOperationException("Por favor, insira um Nº de telemóvel válido.");
            }
            if (request.DataNascimento.HasValue || request.IdSubscricao.HasValue)
            {
                var dataNascimento = request.DataNascimento ?? membro.DataNascimento;
                var idSubscricao = request.IdSubscricao ?? membro.IdSubscricao;
                await ValidateMember(dataNascimento, idSubscricao);
            }

            if (!string.IsNullOrWhiteSpace(request.Nome) && request.Nome != membro.Nome)
            {
                membro.Nome = request.Nome;
                alterado = true;
            }

            if (!string.IsNullOrWhiteSpace(request.Telemovel) && request.Telemovel != membro.Telemovel)
            {
                membro.Telemovel = request.Telemovel;
                alterado = true;
            }

            if (request.DataNascimento.HasValue && request.DataNascimento.Value != membro.DataNascimento)
            {
                membro.DataNascimento = request.DataNascimento.Value;
                alterado = true;
            }

            if (request.IdSubscricao.HasValue && request.IdSubscricao.Value != membro.IdSubscricao)
            {
                membro.IdSubscricao = request.IdSubscricao.Value;
                alterado = true;
            }

            if (alterado)
                await _context.SaveChangesAsync();

            return alterado ? "Membro atualizado com sucesso." : "Nenhuma alteração realizada.";
        }

        private async Task ValidateMember(DateTime? dataNascimento, int? idSubscricao)
        {
            if (!dataNascimento.HasValue)
                throw new InvalidOperationException("Membros precisam de uma data de nascimento.");

            var data = dataNascimento.Value;

            if (data > DateTime.UtcNow)
                throw new InvalidOperationException("A data de nascimento não pode ser futura.");

            if (data > DateTime.UtcNow.AddYears(-14))
                throw new InvalidOperationException("O membro deve ter pelo menos 14 anos.");

            if (idSubscricao == null)
                throw new InvalidOperationException("Membros precisam de uma subscrição.");

            if (!await _context.Subscricoes.AnyAsync(s => s.IdSubscricao == idSubscricao))
                throw new InvalidOperationException("A subscrição indicada não existe.");
        }
    }
}
