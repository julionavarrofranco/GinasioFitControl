using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Data;
using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;
using ProjetoFinal.Services.Interfaces;
using System.Text.RegularExpressions;

namespace ProjetoFinal.Services
{
    public class MemberService: IMemberService
    {
        private readonly GinasioDbContext _context;
        private readonly IPaymentService _paymentService;

        public MemberService(GinasioDbContext context, IPaymentService paymentService)
        {
            _context = context;
            _paymentService = paymentService;
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
            await ValidateMemberAsync(request.DataNascimento, request.IdSubscricao, false);
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
            await _context.SaveChangesAsync();
            // === CRIAR PAGAMENTO AUTOMÁTICO ===
            var subscricao = await _context.Subscricoes.FirstAsync(s => s.IdSubscricao == idSubscricao);

            // ⚠ Pontos a considerar:
            // 1. Mês referente do pagamento
            //    Atualmente fixamos como mês atual. Se quiser avançar o plano inicial (ex.: trimestral = 3 meses pagos à frente),
            //    podemos ajustar aqui, criando pagamentos retroativos ou definindo o mês referente inicial do plano.
            var mesAtual = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var mesReferente = mesAtual; // padrão = mês atual

            // 2. Método de pagamento
            //    Atualmente fixo como Cartão. Pode ser definido pela UI recebendo no UserRegisterDto.
            var metodoPagamento = MetodoPagamento.Cartao;

            // 3. Dia de pagamento
            //    Será definido no PaymentService como dia 8 ou imediato se já passou.

            var paymentDto = new PaymentDto
            {
                IdMembro = membro.IdMembro,
                IdSubscricao = subscricao.IdSubscricao,
                MetodoPagamento = metodoPagamento,
                MesReferente = mesReferente
            };

            await _paymentService.CreatePaymentAsync(paymentDto);

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
                await ValidateMemberAsync(dataNascimento, idSubscricao, true);
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

        public async Task CancelMemberAsync(int idMembro)
        {
            var membro = await _context.Membros
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.IdMembro == idMembro);

            if (membro == null)
                throw new KeyNotFoundException("Membro não encontrado.");

            if (!membro.User.Ativo)
                throw new InvalidOperationException("O membro já se encontra inativo.");

            // Desativar utilizador
            membro.User.Ativo = false;
            membro.User.DataDesativacao = DateTime.UtcNow;

            // Desativar todos os pagamentos futuros de uma vez
            var mesAtual = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var pagamentosFuturos = await _context.Pagamentos
                .Where(p => p.IdMembro == idMembro && p.DataDesativacao == null && p.MesReferente >= mesAtual)
                .ToListAsync();

            foreach (var pagamento in pagamentosFuturos)
                pagamento.DataDesativacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task ReactivateMemberAsync(int idMembro, MetodoPagamento metodo)
        {
            var membro = await _context.Membros
                .Include(m => m.User)
                .Include(m => m.Subscricao)
                .FirstOrDefaultAsync(m => m.IdMembro == idMembro);

            if (membro == null)
                throw new KeyNotFoundException("Membro não encontrado.");

            if (membro.User.Ativo)
                throw new InvalidOperationException("O membro já está ativo.");

            // Reativar utilizador
            membro.User.Ativo = true;
            membro.User.DataDesativacao = null;

            // Criar pagamento automático a partir do mês atual
            var mesAtual = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            var paymentDto = new PaymentDto
            {
                IdMembro = membro.IdMembro,
                IdSubscricao = membro.Subscricao!.IdSubscricao,
                MetodoPagamento = metodo,
                MesReferente = mesAtual
            };

            await _paymentService.CreatePaymentAsync(paymentDto);

            await _context.SaveChangesAsync();
        }

        private async Task ValidateMemberAsync(DateTime? dataNascimento, int? idSubscricao, bool isUpdate)
        {
            if (!isUpdate)
            {
                if (!dataNascimento.HasValue)
                    throw new InvalidOperationException("Membros precisam de uma data de nascimento.");

                if (!idSubscricao.HasValue)
                    throw new InvalidOperationException("Membros precisam de uma subscrição.");
            }

            if (dataNascimento.HasValue)
            {
                if (dataNascimento > DateTime.UtcNow)
                    throw new InvalidOperationException("A data de nascimento não pode ser futura.");

                if (dataNascimento > DateTime.UtcNow.AddYears(-14))
                    throw new InvalidOperationException("O membro deve ter pelo menos 14 anos.");
            }

            if (idSubscricao.HasValue)
            {
                if (!await _context.Subscricoes
                    .AnyAsync(s => s.IdSubscricao == idSubscricao))
                    throw new InvalidOperationException("A subscrição indicada não existe.");
            }
        }
    }
}
