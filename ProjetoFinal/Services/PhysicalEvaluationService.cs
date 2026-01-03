using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Data;
using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;
using ProjetoFinal.Services.Interfaces;

namespace ProjetoFinal.Services
{
    public class PhysicalEvaluationService: IPhysicalEvaluationService
    {
        private readonly GinasioDbContext _context;

        public PhysicalEvaluationService(GinasioDbContext context)
        {
            _context = context;
        }


        private async Task<AvaliacaoFisica?> GetPhysicalEvaluationByIdAsync(int idAvaliacao)
        {
            return await _context.AvaliacoesFisicas
                .FirstOrDefaultAsync(a => a.IdAvaliacao == idAvaliacao && a.DataDesativacao == null);
        }

        public async Task<AvaliacaoFisica> CreatePhysicalEvaluationAsync(PhysicalEvaluationDto request)
        {
            var membro = await _context.Membros.FirstOrDefaultAsync(m => m.IdMembro == request.IdMembro);
            if (membro == null)
                throw new KeyNotFoundException("Membro não encontrado.");

            var funcionario = await _context.Funcionarios.FirstOrDefaultAsync(f => f.IdFuncionario == request.IdFuncionario);
            if (funcionario == null)
                throw new KeyNotFoundException("Funcionário não encontrado.");

            if (request.Peso < 0 || request.Altura < 0 || request.Imc < 0 || request.MassaMuscular < 0 || request.MassaGorda < 0)
            {
                throw new InvalidOperationException("Valores de Peso, Altura, IMC, Massa Muscular ou Massa Gorda não podem ser negativos.");
            }

            var avaliacao = new AvaliacaoFisica
            {
                IdMembro = request.IdMembro,
                IdFuncionario = request.IdFuncionario,
                DataAvaliacao = request.DataAvaliacao,
                Peso = request.Peso,
                Altura = request.Altura,
                Imc = request.Imc,
                MassaMuscular = request.MassaMuscular,
                MassaGorda = request.MassaGorda,
                Observacoes = request.Observacoes ?? string.Empty
            };

            _context.AvaliacoesFisicas.Add(avaliacao);
            await _context.SaveChangesAsync();

            return avaliacao;
        }

        public async Task<string> UpdatePhysicalEvaluationAsync(int idAvaliacao, UpdatePhysicalEvaluationDto request)
        {
            var avaliacao = await GetPhysicalEvaluationByIdAsync(idAvaliacao);

            if (avaliacao == null)
                throw new KeyNotFoundException("Avaliação física não encontrada.");

            bool alterado = false;

            // Validação e atualização de campos
            if (request.Peso.HasValue)
            {
                if (request.Peso.Value < 0)
                    throw new InvalidOperationException("Peso não pode ser negativo.");
                if (request.Peso.Value != avaliacao.Peso)
                {
                    avaliacao.Peso = request.Peso.Value;
                    alterado = true;
                }
            }

            if (request.Altura.HasValue)
            {
                if (request.Altura.Value < 0)
                    throw new InvalidOperationException("Altura não pode ser negativa.");
                if (request.Altura.Value != avaliacao.Altura)
                {
                    avaliacao.Altura = request.Altura.Value;
                    alterado = true;
                }
            }

            if (request.Imc.HasValue)
            {
                if (request.Imc.Value < 0)
                    throw new InvalidOperationException("IMC não pode ser negativo.");
                if (request.Imc.Value != avaliacao.Imc)
                {
                    avaliacao.Imc = request.Imc.Value;
                    alterado = true;
                }
            }

            if (request.MassaMuscular.HasValue)
            {
                if (request.MassaMuscular.Value < 0)
                    throw new InvalidOperationException("Massa muscular não pode ser negativa.");
                if (request.MassaMuscular.Value != avaliacao.MassaMuscular)
                {
                    avaliacao.MassaMuscular = request.MassaMuscular.Value;
                    alterado = true;
                }
            }

            if (request.MassaGorda.HasValue)
            {
                if (request.MassaGorda.Value < 0)
                    throw new InvalidOperationException("Massa gorda não pode ser negativa.");
                if (request.MassaGorda.Value != avaliacao.MassaGorda)
                {
                    avaliacao.MassaGorda = request.MassaGorda.Value;
                    alterado = true;
                }
            }

            if (request.Observacoes != null && request.Observacoes != avaliacao.Observacoes)
            {
                avaliacao.Observacoes = request.Observacoes;
                alterado = true;
            }

            if (alterado)
            {
                await _context.SaveChangesAsync();
                return "Avaliação física atualizada com sucesso.";
            }
            else
            {
                return "Nenhuma alteração realizada.";
            }
        }

        public async Task ChangePhysicalEvaluationActiveStatusAsync(int idAvaliacao, bool ativo)
        {
            var avaliacao = await GetPhysicalEvaluationByIdAsync(idAvaliacao);

            if (avaliacao == null)
                throw new KeyNotFoundException("Avaliação física não encontrada.");

            if (ativo)
            {
                avaliacao.DataDesativacao = null;
            }
            else
            {
                avaliacao.DataDesativacao = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<AvaliacaoFisica>> GetAllEvaluationsFromMemberAsync(int idMembro)
        {
            return await _context.AvaliacoesFisicas
                .Where(a => a.IdMembro == idMembro)
                .OrderByDescending(a => a.DataAvaliacao)
                .ToListAsync();
        }

        public async Task<AvaliacaoFisica?> GetLatestEvaluationFromMemberAsync(int idMembro)
        {
            return await _context.AvaliacoesFisicas
                .Where(a => a.IdMembro == idMembro && a.DataDesativacao == null)
                .OrderByDescending(a => a.DataAvaliacao)
                .FirstOrDefaultAsync();
        }
    }
}
