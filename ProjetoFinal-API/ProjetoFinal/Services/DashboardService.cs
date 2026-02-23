using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Data;
using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;
using ProjetoFinal.Services.Interfaces;

namespace ProjetoFinal.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly GinasioDbContext _context;

        public DashboardService(GinasioDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardDto> GetSummaryAsync()
        {
            // Membros ativos (estado vem do User)
            var membrosAtivos = await _context.Membros
                .CountAsync(m => m.User.Ativo);

            // PTs ativos
            var ptsAtivos = await _context.Funcionarios
                .CountAsync(f =>
                    f.User.Ativo &&
                    f.Funcao == Funcao.PT
                );

            var mesAtual = DateTime.UtcNow.Month;
            var anoAtual = DateTime.UtcNow.Year;

            // Receita mensal
            var receitaMensal = await _context.Pagamentos
                .Where(p =>
                    p.EstadoPagamento == EstadoPagamento.Pago &&
                    p.DataDesativacao == null &&
                    p.DataPagamento.Month == mesAtual &&
                    p.DataPagamento.Year == anoAtual
                )
                .SumAsync(p => (decimal?)p.ValorPago) ?? 0;

            return new DashboardDto
            {
                MembrosAtivos = membrosAtivos,
                PTsAtivos = ptsAtivos,
                ReceitaMensal = receitaMensal
            };
        }
    }
}
