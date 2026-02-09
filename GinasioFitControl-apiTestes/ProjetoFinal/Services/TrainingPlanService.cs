using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Data;
using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;
using ProjetoFinal.Services.Interfaces;

namespace ProjetoFinal.Services
{
    public class TrainingPlanService: ITrainingPlanService
    {
        private readonly GinasioDbContext _context;
        private readonly IUserService _userService;

        public TrainingPlanService(GinasioDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        private async Task<PlanoTreino> GetPlanoAsync(int idPlano)
        {
            return await _context.Planos
                .Include(p => p.PlanosExercicios)
                .FirstOrDefaultAsync(p => p.IdPlano == idPlano)
                ?? throw new KeyNotFoundException("Plano de treino não encontrado.");
        }

        public async Task<PlanoTreino> CreateAsync(int idFuncionario, TrainingPlanDto dto)
        {
            var plano = new PlanoTreino
            {
                IdFuncionario = idFuncionario,
                Nome = dto.Nome,
                Observacoes = dto.Observacoes,
                DataCriacao = DateTime.UtcNow
            };

            _context.Planos.Add(plano);
            await _context.SaveChangesAsync();
            return plano;
        }

        public async Task<string> UpdateAsync(int idPlano, UpdateTrainingPlanDto dto)
        {
            var plano = await GetPlanoAsync(idPlano);

            bool alterado = false;

            if (!string.IsNullOrWhiteSpace(dto.Nome) && dto.Nome != plano.Nome)
            {
                plano.Nome = dto.Nome;
                alterado = true;
            }

            if (dto.Observacoes != null && dto.Observacoes != plano.Observacoes)
            {
                plano.Observacoes = dto.Observacoes;
                alterado = true;
            }

            if (alterado)
            {
                await _context.SaveChangesAsync();
                return "Plano de treino atualizado com sucesso.";
            }

            return "Nenhuma alteração realizada.";
        }


        public async Task AtribuirPlanoAoMembroAsync(int idMembro, int idPlano)
        {
            var membro = await _context.Membros
                .Include(m => m.PlanoTreino)
                .FirstOrDefaultAsync(m => m.IdMembro == idMembro)
                ?? throw new KeyNotFoundException("Membro não encontrado.");

            var plano = await _context.Planos
                .FirstOrDefaultAsync(p => p.IdPlano == idPlano && p.DataDesativacao == null)
                ?? throw new InvalidOperationException("Plano inexistente ou inativo.");

            // se já tem este plano, não faz nada
            if (membro.IdPlanoTreino == idPlano)
                return;

            membro.IdPlanoTreino = idPlano;
            await _context.SaveChangesAsync();
        }

        public async Task RemoverPlanoDoMembroAsync(int idMembro)
        {
            var membro = await _context.Membros
                .FirstOrDefaultAsync(m => m.IdMembro == idMembro)
                ?? throw new KeyNotFoundException("Membro não encontrado.");

            membro.IdPlanoTreino = null;
            await _context.SaveChangesAsync();
        }

        public async Task ChangeActiveStateAsync(int idPlano, bool ativo)
        {
            var plano = await GetPlanoAsync(idPlano);
            plano.DataDesativacao = ativo ? null : DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<TrainingPlanDetailDto?> GetPlanoAtualDoMembroAsync(int idUser)
        {
            var user = await _userService.GetUserByIdAsync(
               idUser,
               includeMembro: true
           );


            if (user?.Membro == null)
                throw new InvalidOperationException("O utilizador não é um membro.");

            var idMembro = user.Membro.IdMembro;

            var idPlanoAtual = await _context.Membros
                .Where(m => m.IdMembro == idMembro)
                .Select(m => m.IdPlanoTreino)
                .FirstOrDefaultAsync();

            if (idPlanoAtual == null)
                return null;

            var plano = await GetPlanoDetalheAsync(idPlanoAtual.Value);

            if (plano != null && !plano.Ativo)
                return null;

            return plano;
        }

        public async Task<List<PlanoTreino>> GetPlanosByEstadoAsync(bool ativo)
        {
            return await _context.Planos
                .Where(p => ativo ? p.DataDesativacao == null : p.DataDesativacao != null)
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();
        }

        public async Task<List<TrainingPlanSummaryDto>> GetPlanosResumoAsync(bool? ativo = null)
        {
            IQueryable<PlanoTreino> query = _context.Planos.AsNoTracking();

            if (ativo.HasValue)
            {
                query = ativo.Value
                    ? query.Where(p => p.DataDesativacao == null)
                    : query.Where(p => p.DataDesativacao != null);
            }

            return await query
                .OrderByDescending(p => p.DataCriacao)
                .Select(p => new TrainingPlanSummaryDto
                {
                    IdPlano = p.IdPlano,
                    Nome = p.Nome,
                    DataCriacao = p.DataCriacao,
                    Ativo = p.DataDesativacao == null
                })
                .ToListAsync();
        }

        public async Task<TrainingPlanDetailDto?> GetPlanoDetalheAsync(int idPlano)
        {
            var plano = await _context.Planos
                .AsNoTracking()
                .Include(p => p.PlanosExercicios)
                    .ThenInclude(pe => pe.Exercicio)
                .Include(p => p.Funcionario)
                .FirstOrDefaultAsync(p => p.IdPlano == idPlano);

            if (plano == null)
                return null;

            return new TrainingPlanDetailDto
            {
                IdPlano = plano.IdPlano,
                IdFuncionario = plano.IdFuncionario,
                Nome = plano.Nome,
                DataCriacao = plano.DataCriacao,
                Observacoes = plano.Observacoes,
                Ativo = plano.DataDesativacao == null,
                NomeFuncionario = plano.Funcionario?.Nome,
                Exercicios = plano.PlanosExercicios
                    .OrderBy(pe => pe.Ordem) // garante a ordem correta
                    .Select(pe => new TrainingPlanExerciseDto
                    {
                        IdExercicio = pe.IdExercicio,
                        NomeExercicio = pe.Exercicio.Nome,
                        GrupoMuscular = pe.Exercicio.GrupoMuscular.ToString(), // converte enum para string
                        Descricao = pe.Exercicio.Descricao ?? "",
                        FotoUrl = pe.Exercicio.FotoUrl ?? "",
                        Series = pe.Series,
                        Repeticoes = pe.Repeticoes,
                        Carga = pe.Carga,
                        Ordem = pe.Ordem
                    })
                    .ToList()
            };
        }

    }
}
