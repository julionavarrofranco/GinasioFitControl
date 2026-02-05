using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Data;
using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;
using ProjetoFinal.Services.Interfaces;

namespace ProjetoFinal.Services
{
    public class ExerciseService: IExerciseService
    {
        private readonly GinasioDbContext _context;
        public ExerciseService(GinasioDbContext context)
        {
            _context = context;
        }

        private async Task<Exercicio?> GetExerciseByIdAsync(int idExercicio)
        {
            return await _context.Exercicios
                .FirstOrDefaultAsync(e => e.IdExercicio == idExercicio);
        }

        public async Task<Exercicio> CreateExerciseAsync(ExerciseDto request)
        {
            ValidateExercise(request.Nome, request.Descricao, request.FotoUrl, request.GrupoMuscular, false);

            Exercicio exercicio = new Exercicio();
            exercicio.Nome = request.Nome;
            exercicio.Descricao = request.Descricao;
            exercicio.FotoUrl = request.FotoUrl;
            exercicio.GrupoMuscular = request.GrupoMuscular;
            exercicio.Ativo = true;

            _context.Exercicios.Add(exercicio);
            await _context.SaveChangesAsync();

            return exercicio;
        }

        public async Task<string> UpdateExerciseAsync(int idExercicio, UpdateExerciseDto request)
        {
            var exercicio = await GetExerciseByIdAsync(idExercicio);

            if (exercicio == null)
                throw new KeyNotFoundException("Exercício não encontrado.");

            ValidateExercise(request.Nome, request.Descricao, request.FotoUrl, request.GrupoMuscular, true);

            bool alterado = false;

            if (!string.IsNullOrWhiteSpace(request.Nome))
            {
                if (request.Nome != exercicio.Nome)
                {
                    exercicio.Nome = request.Nome;
                    alterado = true;
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Descricao))
            {
                if (request.Descricao != exercicio.Descricao)
                {
                    exercicio.Descricao = request.Descricao;
                    alterado = true;
                }
            }

            if (!string.IsNullOrWhiteSpace(request.FotoUrl))
            {
                if (request.FotoUrl != exercicio.FotoUrl)
                {
                    exercicio.FotoUrl = request.FotoUrl;
                    alterado = true;
                }
            }

            if (request.GrupoMuscular.HasValue && request.GrupoMuscular.Value != exercicio.GrupoMuscular)
            {
                exercicio.GrupoMuscular = request.GrupoMuscular.Value;
                alterado = true;
            }


            if (alterado)
            {
                await _context.SaveChangesAsync();
                return "Exercício atualizado com sucesso.";
            }
            else
            {
                return "Nenhuma alteração realizada.";
            }
        }

        public async Task ChangeExerciseActiveStatusAsync(int idExercicio, bool ativo)
        {
            var exercicio = await GetExerciseByIdAsync(idExercicio);
            if (exercicio == null)
                throw new KeyNotFoundException("Exercício não encontrado.");

            // Se o status já for o mesmo, não há alteração, mas não lançamos exceção
            if (exercicio.Ativo != ativo)
            {
                exercicio.Ativo = ativo;
                await _context.SaveChangesAsync();
            }
        }


        public async Task<List<Exercicio>> GetExercisesByStateAsync(bool ativo, bool ordenarAsc = true)
        {
            IQueryable<Exercicio> query = _context.Exercicios.AsNoTracking()
                .Where(e => e.Ativo == ativo);

            if (ordenarAsc)
            {
                query = query.OrderBy(e => e.Nome);
            }
            else
            {
                query = query.OrderByDescending(e => e.Nome);
            }

            return await query.ToListAsync();
        }

        public async Task<List<Exercicio>> GetExercisesByMuscleGroupAsync(GrupoMuscular grupo, bool ordenarAsc = true)
        {
            IQueryable<Exercicio> query = _context.Exercicios.AsNoTracking()
                .Where(e => e.GrupoMuscular == grupo);

            if (ordenarAsc)
            {
                query = query.OrderBy(e => e.Nome);
            }
            else
            {
                query = query.OrderByDescending(e => e.Nome);
            }

            return await query.ToListAsync();
        }

        public async Task<List<Exercicio>> GetExercisesByNameAsync(string nome, bool ordenarAsc = true)
        {
            IQueryable<Exercicio> query = _context.Exercicios.AsNoTracking()
                .Where(e => EF.Functions.Like(e.Nome, $"%{nome}%"));

            if (ordenarAsc)
            {
                query = query.OrderBy(e => e.Nome);
            }
            else
            {
                query = query.OrderByDescending(e => e.Nome);
            }

            return await query.ToListAsync();
        }
        private void ValidateExercise(string? nome, string? descricao, string? fotoUrl, GrupoMuscular? grupoMuscular, bool isUpdate)
        {
            if (!isUpdate)
            {
                if (string.IsNullOrWhiteSpace(nome))
                    throw new InvalidOperationException("O nome do exercício é obrigatório.");

                if (string.IsNullOrWhiteSpace(descricao))
                    throw new InvalidOperationException("A descrição do exercício é obrigatória.");

                if (string.IsNullOrWhiteSpace(fotoUrl))
                    throw new InvalidOperationException("A URL da foto é obrigatória.");

                if (!grupoMuscular.HasValue)
                    throw new InvalidOperationException("O grupo muscular é obrigatório.");
            }

            if (!string.IsNullOrWhiteSpace(fotoUrl) && !Uri.IsWellFormedUriString(fotoUrl, UriKind.Absolute))
                throw new InvalidOperationException("A URL da foto não é válida.");
        }


    }
}
