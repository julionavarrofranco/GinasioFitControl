using ProjetoFinal.Data;
using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Services.Interfaces;

namespace ProjetoFinal.Services
{
    public class MemberClassService : IMemberClassService
    {
        private readonly GinasioDbContext _context;

        public MemberClassService(GinasioDbContext context)
        {
            _context = context;
        }

        // Membro reserva aula
        private async Task<MembroAula?> GetReservationAsync(int idMembro, int idAulaMarcada)
        {
            return await _context.MembrosAulas
                .Include(r => r.AulaMarcada)
                .FirstOrDefaultAsync(r => r.IdMembro == idMembro && r.IdAulaMarcada == idAulaMarcada);
        }

        public async Task<MembroAula> ReservarAsync(int idMembro, int idAulaMarcada)
        {
            await using var transaction = await _context.Database
                .BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            var aula = await _context.AulasMarcadas
                .Include(a => a.Aula)
                .Include(a => a.MembrosAulas)
                .FirstOrDefaultAsync(a => a.Id == idAulaMarcada)
                ?? throw new KeyNotFoundException("Aula não encontrada.");

            if (aula.DataDesativacao != null)
                throw new InvalidOperationException("A aula foi cancelada.");

            var diasAntecedencia = (aula.DataAula.Date - DateTime.UtcNow.Date).TotalDays;
            if (diasAntecedencia < 1)
                throw new InvalidOperationException("Reserva com mínimo de 1 dia de antecedência.");
            if (diasAntecedencia > 15)
                throw new InvalidOperationException("Não é possível reservar com mais de 15 dias de antecedência.");

            bool jaReservado = await _context.MembrosAulas
                .AnyAsync(m =>
                    m.IdMembro == idMembro &&
                    m.IdAulaMarcada == idAulaMarcada &&
                    m.Presenca == Presenca.Reservado);

            if (jaReservado)
                throw new InvalidOperationException("O membro já possui reserva nesta aula.");

            int reservasAtuais = await _context.MembrosAulas
                .Where(m => m.IdAulaMarcada == idAulaMarcada && m.Presenca == Presenca.Reservado)
                .CountAsync();

            if (reservasAtuais >= aula.Aula.Capacidade)
                throw new InvalidOperationException("Aula cheia.");

            var reserva = new MembroAula
            {
                IdMembro = idMembro,
                IdAulaMarcada = idAulaMarcada,
                DataReserva = DateTime.UtcNow,
                Presenca = Presenca.Reservado
            };

            _context.MembrosAulas.Add(reserva);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return reserva;
        }



        public async Task<string> CancelarReservaAsync(int idMembro, int idAulaMarcada)
        {
            await using var transaction = await _context.Database
                .BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            var reserva = await _context.MembrosAulas
                .FirstOrDefaultAsync(m => m.IdMembro == idMembro && m.IdAulaMarcada == idAulaMarcada)
                ?? throw new KeyNotFoundException("Reserva não encontrada.");

            await _context.Entry(reserva)
                .Reference(r => r.AulaMarcada)
                .LoadAsync();

            if (reserva.AulaMarcada.DataAula.Date <= DateTime.UtcNow.Date)
                throw new InvalidOperationException("Não é possível cancelar com menos de 1 dia de antecedência.");

            reserva.Presenca = Presenca.Cancelado;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return "Reserva cancelada com sucesso.";
        }




        public async Task<List<MembroAula>> ListarReservasDoMembroAsync(int idMembro) =>
            await _context.MembrosAulas
                .AsNoTracking()
                .Include(r => r.AulaMarcada)
                .ThenInclude(a => a.Aula)
                .Where(r => r.IdMembro == idMembro && r.Presenca == Presenca.Reservado)
                .OrderBy(r => r.AulaMarcada.DataAula)
                .ToListAsync();

        public async Task<string> MarcarPresencasAsync(int idAulaMarcada, List<int> idsMembrosPresentes)
        {
            var aula = await _context.AulasMarcadas
                .Include(a => a.MembrosAulas)
                .FirstOrDefaultAsync(a => a.Id == idAulaMarcada)
                ?? throw new KeyNotFoundException("Aula não encontrada.");

            if (aula.DataDesativacao != null)
                throw new InvalidOperationException("A aula foi cancelada.");

            //para testes desativada esta validação, produção deve manter
            //if (aula.DataAula.Date > DateTime.UtcNow.Date)
            //    throw new InvalidOperationException("Não é possível marcar presenças antes da aula ocorrer.");

            var reservas = aula.MembrosAulas
                .Where(r => r.Presenca == Presenca.Reservado || r.Presenca == Presenca.Presente || r.Presenca == Presenca.Faltou)
                .ToList();

            if (!reservas.Any())
                throw new InvalidOperationException("Não existem reservas para esta aula.");

            foreach (var r in reservas)
                r.Presenca = idsMembrosPresentes.Contains(r.IdMembro) ? Presenca.Presente : Presenca.Faltou;

            await _context.SaveChangesAsync();
            return "Presenças marcadas com sucesso.";
        }



        public async Task<ClassAttendanceDto> ObterAulaParaPresencaAsync(int idAulaMarcada)
        {
            var aula = await _context.AulasMarcadas
                .AsNoTracking()
                .Include(a => a.Aula)
                .Include(a => a.MembrosAulas)
                    .ThenInclude(m => m.Membro)
                    .ThenInclude(mem => mem.User)
                .FirstOrDefaultAsync(a => a.Id == idAulaMarcada)
                ?? throw new KeyNotFoundException("Aula não encontrada.");

            var dto = new ClassAttendanceDto
            {
                IdAulaMarcada = aula.Id,
                DataAula = aula.DataAula,
                NomeAula = aula.Aula.Nome,
                HoraInicio = aula.Aula.HoraInicio,
                HoraFim = aula.Aula.HoraFim,
                Capacidade = aula.Aula.Capacidade,
                Reservas = aula.MembrosAulas
                    .Where(r => r.Presenca != Presenca.Cancelado)
                    .Select(r => new MemberReservationDto
                    {
                        IdMembro = r.IdMembro,
                        Nome = r.Membro.Nome,
                        Email = r.Membro.User.Email,
                        Telemovel = r.Membro.Telemovel,
                        Presenca = r.Presenca
                    })
                    .ToList()
            };

            return dto;
        }

        // Listar todas as reservas de aulas (para admin: agendadas + terminadas)
        public async Task<List<ClassReservationSummaryDto>> ListarTodasReservasAsync()
        {
            return await _context.AulasMarcadas
                .AsNoTracking()
                .Include(a => a.Aula)
                .Where(a => a.DataDesativacao == null)
                .OrderBy(a => a.DataAula)
                .Select(a => new ClassReservationSummaryDto
                {
                    IdAulaMarcada = a.Id,
                    DataAula = a.DataAula,
                    Sala = a.Sala,
                    NomeAula = a.Aula.Nome,
                    HoraInicio = a.Aula.HoraInicio,
                    HoraFim = a.Aula.HoraFim,
                    Capacidade = a.Aula.Capacidade,
                    TotalReservas = a.MembrosAulas.Count(r => r.Presenca != Presenca.Cancelado)
                })
                .ToListAsync();
        }

        // Listar todas as reservas de aulas que um PT vai dar
        public async Task<List<ClassReservationSummaryDto>> ListarReservasPorPtAsync(int idFuncionario)
        {
            return await _context.AulasMarcadas
                .AsNoTracking()
                .Include(a => a.Aula)
                .Where(a => a.Aula.IdFuncionario == idFuncionario && a.DataDesativacao == null)
                .OrderBy(a => a.DataAula)
                .Select(a => new ClassReservationSummaryDto
                {
                    IdAulaMarcada = a.Id,
                    DataAula = a.DataAula,
                    NomeAula = a.Aula.Nome,
                    Sala = a.Sala,
                    HoraInicio = a.Aula.HoraInicio,
                    HoraFim = a.Aula.HoraFim,
                    Capacidade = a.Aula.Capacidade,
                    TotalReservas = a.MembrosAulas.Count(r => r.Presenca != Presenca.Cancelado)
                })
                .ToListAsync();
        }

    }
}
