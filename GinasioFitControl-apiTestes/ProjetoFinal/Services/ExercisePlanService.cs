using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Data;
using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;
using ProjetoFinal.Services.Interfaces;

namespace ProjetoFinal.Services
{
    public class ExercisePlanService: IExercisePlanService
    {
        private readonly GinasioDbContext _context;

        public ExercisePlanService(GinasioDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(int idPlano, ExercisePlanDto dto)
        {
            var existe = await _context.PlanosExercicios
                .AnyAsync(p => p.IdPlano == idPlano && p.IdExercicio == dto.IdExercicio);

            if (existe)
                throw new InvalidOperationException("Exercício já existe neste plano.");

            int ordem = dto.Ordem ?? (
                await _context.PlanosExercicios
                    .CountAsync(p => p.IdPlano == idPlano) + 1
            );

            var planoExercicio = new PlanoExercicio
            {
                IdPlano = idPlano,
                IdExercicio = dto.IdExercicio,
                Series = dto.Series,
                Repeticoes = dto.Repeticoes,
                Carga = dto.Carga,
                Ordem = ordem
            };

            _context.PlanosExercicios.Add(planoExercicio);
            await _context.SaveChangesAsync();
        }

        public async Task<string> UpdateAsync(int idPlano, int idExercicio, UpdateExercisePlanDto dto)
        {
            var pe = await _context.PlanosExercicios
                .FirstOrDefaultAsync(p => p.IdPlano == idPlano && p.IdExercicio == idExercicio)
                ?? throw new KeyNotFoundException("Exercício não encontrado no plano.");

            bool alterado = false;

            if (dto.Series.HasValue && dto.Series.Value != pe.Series)
            {
                pe.Series = dto.Series.Value;
                alterado = true;
            }

            if (dto.Repeticoes.HasValue && dto.Repeticoes.Value != pe.Repeticoes)
            {
                pe.Repeticoes = dto.Repeticoes.Value;
                alterado = true;
            }

            if (dto.Carga.HasValue && dto.Carga.Value != pe.Carga)
            {
                pe.Carga = dto.Carga.Value;
                alterado = true;
            }

            if (dto.Ordem.HasValue && dto.Ordem.Value != pe.Ordem)
            {
                pe.Ordem = dto.Ordem.Value;
                alterado = true;
            }

            if (alterado)
            {
                await _context.SaveChangesAsync();
                return "Exercício do plano atualizado com sucesso.";
            }

            return "Nenhuma alteração realizada.";
        }


        public async Task DeleteAsync(int idPlano, int idExercicio)
        {
            var pe = await _context.PlanosExercicios
                .FirstOrDefaultAsync(p => p.IdPlano == idPlano && p.IdExercicio == idExercicio)
                ?? throw new KeyNotFoundException("Exercício não encontrado.");

            _context.PlanosExercicios.Remove(pe);
            await _context.SaveChangesAsync();
        }
    }
}
