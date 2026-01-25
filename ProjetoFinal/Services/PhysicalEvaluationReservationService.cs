using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Data;
using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;
using ProjetoFinal.Services.Interfaces;

namespace ProjetoFinal.Services
{
    public class PhysicalEvaluationReservationService: IPhysicalEvaluationReservationService
    {
        private readonly GinasioDbContext _context;

        private readonly IPhysicalEvaluationService _physicalEvaluationService;

        public PhysicalEvaluationReservationService(GinasioDbContext context, IPhysicalEvaluationService physicalEvaluationService)
        {
            _context = context;
            _physicalEvaluationService = physicalEvaluationService;
        }

        private async Task<MembroAvaliacao?> GetReservationByIdAsync(int idMembro, int idMembroAvaliacao)
        {
            return await _context.MembrosAvaliacoes
                .Include(r => r.AvaliacaoFisica)
                .FirstOrDefaultAsync(r => r.IdMembro == idMembro && r.IdMembroAvaliacao == idMembroAvaliacao && r.DataDesativacao == null);
        }

        public async Task<MembroAvaliacao> CreateReservationAsync(int idMembro, DateTime dataReserva)
        {
            if (dataReserva < DateTime.UtcNow.AddDays(7))
                throw new InvalidOperationException("A reserva deve ser feita com pelo menos 7 dias de antecedência.");

            bool hasActive = await _context.MembrosAvaliacoes
                .AnyAsync(r => r.IdMembro == idMembro && r.Estado == EstadoAvaliacao.Reservado && r.DataDesativacao == null);

            if (hasActive)
                throw new InvalidOperationException("O membro já possui uma reserva ativa.");

            var reserva = new MembroAvaliacao
            {
                IdMembro = idMembro,
                DataReserva = dataReserva,
                Estado = EstadoAvaliacao.Reservado
            };

            _context.MembrosAvaliacoes.Add(reserva);
            await _context.SaveChangesAsync();

            return reserva;
        }

        public async Task<bool> CancelReservationAsync(int idMembro, int idAvaliacao)
        {
            var reserva = await GetReservationByIdAsync(idMembro, idAvaliacao);

            if (reserva == null || reserva.Estado != EstadoAvaliacao.Reservado)
                return false;

            if ((reserva.DataReserva - DateTime.UtcNow).TotalDays < 7)
                throw new InvalidOperationException("Não é possível cancelar a reserva com menos de 7 dias de antecedência.");

            reserva.Estado = EstadoAvaliacao.Cancelado;
            reserva.DataDesativacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ConfirmReservationAsync(int idMembro, int idAvaliacao, int idFuncionario)
        {
            var reserva = await GetReservationByIdAsync(idMembro, idAvaliacao);
            if (reserva == null || reserva.Estado != EstadoAvaliacao.Reservado)
                return false;

            // Apenas muda o estado para Presente, sem criar avaliação física
            reserva.Estado = EstadoAvaliacao.Presente;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAttendanceAsync(int idMembro, int idAvaliacao, MarkAttendanceDto request)
        {
            var reserva = await GetReservationByIdAsync(idMembro, idAvaliacao);
            if (reserva == null || reserva.Estado != EstadoAvaliacao.Presente)
                return false;

            if (reserva.AvaliacaoFisica != null)
                throw new InvalidOperationException("Esta reserva já tem uma avaliação física associada.");

            if (request.Presente)
            {
                var avaliacao = await _physicalEvaluationService.CreatePhysicalEvaluationAsync(
                    new PhysicalEvaluationDto
                    {
                        IdMembro = idMembro,
                        IdFuncionario = request.IdFuncionario,
                        DataAvaliacao = reserva.DataReserva,
                        Peso = request.Peso,
                        Altura = request.Altura,
                        Imc = request.Imc,
                        MassaMuscular = request.MassaMuscular,
                        MassaGorda = request.MassaGorda,
                        Observacoes = request.Observacoes
                    });

                reserva.IdAvaliacaoFisica = avaliacao.IdAvaliacao;
                reserva.DataDesativacao = DateTime.UtcNow;
            }
            else
            {
                reserva.Estado = EstadoAvaliacao.Faltou;
                reserva.DataDesativacao = DateTime.UtcNow;
                if (request.Peso != 0 || request.Altura != 0 || request.Imc != 0 || request.MassaMuscular != 0 || request.MassaGorda != 0 || !string.IsNullOrWhiteSpace(request.Observacoes))
                {
                    throw new InvalidOperationException("Não é permitido enviar dados físicos quando o membro não esteve presente.");
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<MemberEvaluationReservationSummaryDto>> GetReservationsAsync()
        {
            return await _context.MembrosAvaliacoes
                .AsNoTracking()
                .Include(r => r.Membro) // necessário para Nome e Telemovel
                .Include(r => r.AvaliacaoFisica) // carrega AvaliacaoFisica, Funcionario será acessado no Select
                .Where(r => r.Estado == EstadoAvaliacao.Reservado && r.DataDesativacao == null)
                .OrderBy(r => r.DataReserva)
                .Select(r => new MemberEvaluationReservationSummaryDto
                {
                    IdMembroAvaliacao = r.IdMembroAvaliacao,
                    IdMembro = r.IdMembro,
                    NomeMembro = r.Membro != null ? r.Membro.Nome : string.Empty,
                    TelemovelMembro = r.Membro != null ? r.Membro.Telemovel : string.Empty,
                    IdFuncionario = r.AvaliacaoFisica != null ? r.AvaliacaoFisica.IdFuncionario : (int?)null,
                    NomeFuncionario = r.AvaliacaoFisica != null && r.AvaliacaoFisica.Funcionario != null
                        ? r.AvaliacaoFisica.Funcionario.Nome
                        : string.Empty,
                    DataReserva = r.DataReserva,
                    EstadoString = r.Estado.ToString()
                })
                .ToListAsync();
        }

        public async Task<List<MemberEvaluationReservationSummaryDto>> GetCompletedReservationsAsync()
        {
            return await _context.MembrosAvaliacoes
                .AsNoTracking()
                .Include(r => r.Membro)
                .Include(r => r.AvaliacaoFisica)
                .Where(r => r.Estado == EstadoAvaliacao.Presente && r.IdAvaliacaoFisica == null)
                .OrderByDescending(r => r.DataReserva)
                .Select(r => new MemberEvaluationReservationSummaryDto
                {
                    IdMembroAvaliacao = r.IdMembroAvaliacao,
                    IdMembro = r.IdMembro,
                    NomeMembro = r.Membro != null ? r.Membro.Nome : string.Empty,
                    TelemovelMembro = r.Membro != null ? r.Membro.Telemovel : string.Empty,
                    IdFuncionario = r.AvaliacaoFisica != null ? r.AvaliacaoFisica.IdFuncionario : (int?)null,
                    NomeFuncionario = r.AvaliacaoFisica != null && r.AvaliacaoFisica.Funcionario != null
                        ? r.AvaliacaoFisica.Funcionario.Nome
                        : string.Empty,
                    DataReserva = r.DataReserva,
                    EstadoString = r.Estado.ToString(),
                    TemAvaliacaoFisica = r.IdAvaliacaoFisica != null
                })
                .ToListAsync();
        }


    }
}
