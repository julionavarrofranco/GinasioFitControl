using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Data;
using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;
using ProjetoFinal.Services.Interfaces;

namespace ProjetoFinal.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly GinasioDbContext _context;

        public PaymentService(GinasioDbContext context)
        {
            _context = context;
        }

        // =========================
        // CREATE
        // =========================
        public async Task CreatePaymentAsync(PaymentDto request)
        {
            var membro = await _context.Membros
                .Include(m => m.Subscricao)
                .FirstOrDefaultAsync(m => m.IdMembro == request.IdMembro);

            if (membro == null)
                throw new KeyNotFoundException("Membro não encontrado.");

            var subscricao = await _context.Subscricoes
                .FirstOrDefaultAsync(s => s.IdSubscricao == request.IdSubscricao && s.Ativo);

            if (subscricao == null)
                throw new KeyNotFoundException("Subscrição não encontrada ou inativa.");

            // Normalizar mês referente (dia 1)
            var hoje = DateTime.UtcNow;

            var mesReferente = hoje.Day > 25
                ? new DateTime(hoje.Year, hoje.Month, 1).AddMonths(1)
                : new DateTime(hoje.Year, hoje.Month, 1);

            // ❌ Bloquear pagamentos retroativos
            var mesAtual = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            if (mesReferente < mesAtual)
                throw new InvalidOperationException("Não é permitido criar pagamentos retroativos.");

            if (await PaymentExistsForPeriodAsync(request.IdMembro, mesReferente, subscricao.Tipo))
                throw new InvalidOperationException("Já existe um pagamento ativo para este período.");

            // Data de pagamento = dia 8 do mês referente
            var dataPagamentoPlaneada = new DateTime(
                mesReferente.Year,
                mesReferente.Month,
                8
            );

            // Se já passou do dia 8, assume pagamento imediato
            var dataPagamento = DateTime.UtcNow > dataPagamentoPlaneada
                ? DateTime.UtcNow
                : dataPagamentoPlaneada;

            var pagamento = new Pagamento
            {
                IdMembro = request.IdMembro,
                IdSubscricao = subscricao.IdSubscricao,
                ValorPago = subscricao.Preco, // valor vem SEMPRE da subscrição
                MetodoPagamento = request.MetodoPagamento,
                EstadoPagamento = EstadoPagamento.Pago,
                MesReferente = mesReferente,
                DataPagamento = dataPagamento,
                DataRegisto = DateTime.UtcNow
            };

            _context.Pagamentos.Add(pagamento);
            await _context.SaveChangesAsync();
        }

        // =========================
        // UPDATE
        // =========================
        public async Task<string> UpdatePaymentAsync(int idPagamento, UpdatePaymentDto request)
        {
            var pagamento = await GetPaymentByIdAsync(idPagamento);

            if (pagamento == null)
                throw new KeyNotFoundException("Pagamento não encontrado.");

            bool alterado = false;

            if (request.ValorPago.HasValue)
            {
                if (request.ValorPago.Value <= 0)
                    throw new InvalidOperationException("O valor do pagamento deve ser superior a zero.");

                if (request.ValorPago.Value != pagamento.ValorPago)
                {
                    pagamento.ValorPago = request.ValorPago.Value;
                    alterado = true;
                }
            }

            if (request.MetodoPagamento.HasValue &&
                request.MetodoPagamento.Value != pagamento.MetodoPagamento)
            {
                pagamento.MetodoPagamento = request.MetodoPagamento.Value;
                alterado = true;
            }

            if (request.EstadoPagamento.HasValue &&
                request.EstadoPagamento.Value != pagamento.EstadoPagamento)
            {
                pagamento.EstadoPagamento = request.EstadoPagamento.Value;

                if (request.EstadoPagamento == EstadoPagamento.Pago)
                    pagamento.DataPagamento = DateTime.UtcNow;

                alterado = true;
            }

            if (alterado)
            {
                await _context.SaveChangesAsync();
                return "Pagamento atualizado com sucesso.";
            }

            return "Nenhuma alteração realizada.";
        }


        // =========================
        // ATIVAR / DESATIVAR
        // =========================
        public async Task ChangePaymentActiveStateAsync(int idPagamento, bool ativo)
        {
            var pagamento = await GetPaymentByIdAsync(idPagamento);

            if (pagamento == null)
                throw new KeyNotFoundException("Pagamento não encontrado.");

            if (ativo)
            {
                pagamento.DataDesativacao = null;
            }
            else
            {
                pagamento.DataDesativacao = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }


        // =========================
        // LISTAGENS
        // =========================
        public async Task<List<PaymentResponseDto>> GetPaymentsByActiveStateAsync(bool ativo)
        {
            return await _context.Pagamentos
                .AsNoTracking()
                .Include(p => p.Membro)
                .Include(p => p.Subscricao)
                .Where(p => ativo ? p.DataDesativacao == null : p.DataDesativacao != null)
                .OrderByDescending(p => p.DataRegisto)
                .Select(p => new PaymentResponseDto
                {
                    IdPagamento = p.IdPagamento,
                    ValorPago = p.ValorPago,
                    EstadoPagamento = p.EstadoPagamento.ToString(),
                    MesReferente = p.MesReferente,
                    DataPagamento = p.DataPagamento,
                    MetodoPagamento = p.MetodoPagamento.ToString(),
                    IdMembro = p.Membro.IdMembro,
                    NomeMembro = p.Membro.Nome,
                    Subscricao = p.Subscricao.Tipo.ToString()
                })
                .ToListAsync();
        }


        public async Task<List<PaymentResponseDto>> GetPaymentsByDateAsync(DateTime inicio, DateTime fim)
        {
            return await _context.Pagamentos
                .AsNoTracking()
                .Include(p => p.Membro)
                .Include(p => p.Subscricao)
                .Where(p => p.DataPagamento >= inicio && p.DataPagamento <= fim)
                .OrderByDescending(p => p.DataPagamento)
                .Select(p => new PaymentResponseDto
                {
                    IdPagamento = p.IdPagamento,
                    ValorPago = p.ValorPago,
                    EstadoPagamento = p.EstadoPagamento.ToString(),
                    MesReferente = p.MesReferente,
                    DataPagamento = p.DataPagamento,
                    MetodoPagamento = p.MetodoPagamento.ToString(),
                    IdMembro = p.Membro.IdMembro,
                    NomeMembro = p.Membro.Nome,
                    Subscricao = p.Subscricao.Tipo.ToString()
                })
                .ToListAsync();
        }


        public async Task<List<PaymentResponseDto>> GetPaymentsByPaymentStateAsync(EstadoPagamento estado)
        {
            return await _context.Pagamentos
                .AsNoTracking()
                .Include(p => p.Membro)
                .Include(p => p.Subscricao)
                .Where(p => p.EstadoPagamento == estado && p.DataDesativacao == null)
                .OrderByDescending(p => p.DataPagamento)
                .Select(p => new PaymentResponseDto
                {
                    IdPagamento = p.IdPagamento,
                    ValorPago = p.ValorPago,
                    EstadoPagamento = p.EstadoPagamento.ToString(),
                    MesReferente = p.MesReferente,
                    DataPagamento = p.DataPagamento,
                    MetodoPagamento = p.MetodoPagamento.ToString(),
                    IdMembro = p.Membro.IdMembro,
                    NomeMembro = p.Membro.Nome,
                    Subscricao = p.Subscricao.Tipo.ToString()
                })
                .ToListAsync();
        }


        // =========================
        // DASHBOARD
        // =========================
        public async Task<decimal> GetMonthlyRevenueAsync(int ano, int mes)
        {
            var inicio = new DateTime(ano, mes, 1);
            var fim = inicio.AddMonths(1);

            return await _context.Pagamentos
                .Where(p =>
                    p.EstadoPagamento == EstadoPagamento.Pago &&
                    p.DataPagamento >= inicio &&
                    p.DataPagamento < fim &&
                    p.DataDesativacao == null)
                .SumAsync(p => p.ValorPago);
        }

        // =========================
        // AUXILIARES
        // =========================
        private async Task<Pagamento?> GetPaymentByIdAsync(int idPagamento)
        {
            return await _context.Pagamentos
                .Include(p => p.Membro)
                .Include(p => p.Subscricao)
                .FirstOrDefaultAsync(p => p.IdPagamento == idPagamento);
        }

        private async Task<bool> PaymentExistsForPeriodAsync(int idMembro, DateTime mesReferente, TipoSubscricao tipo, int? excludePagamentoId = null)
        {
            int meses = tipo switch
            {
                TipoSubscricao.Mensal => 1,
                TipoSubscricao.Trimestral => 3,
                TipoSubscricao.Anual => 12,
                _ => throw new InvalidOperationException("Tipo de subscrição inválido.")
            };

            var fim = mesReferente.AddMonths(meses);

            return await _context.Pagamentos.AnyAsync(p =>
                p.IdMembro == idMembro &&
                p.DataDesativacao == null &&
                p.MesReferente >= mesReferente &&
                p.MesReferente < fim &&
                (!excludePagamentoId.HasValue || p.IdPagamento != excludePagamentoId.Value)
            );
        }

    }
}
