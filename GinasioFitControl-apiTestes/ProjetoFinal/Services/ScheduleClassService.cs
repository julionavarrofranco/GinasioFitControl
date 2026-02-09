using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Data;
using ProjetoFinal.Helpers;
using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;
using ProjetoFinal.Services.Interfaces;

public class ScheduleClassService : IScheduleClassService
{
    private readonly GinasioDbContext _context;

    public ScheduleClassService(GinasioDbContext context)
    {
        _context = context;
    }

    private async Task<AulaMarcada?> GetScheduledClassByIdAsync(int idAulaMarcada)
    {
        return await _context.AulasMarcadas
            .Include(a => a.MembrosAulas)
            .Include(a => a.Aula)
            .FirstOrDefaultAsync(a => a.Id == idAulaMarcada);
    }

    // 🔹 Obtém sala livre automaticamente (usado na geração automática)
    private async Task<int> ObterSalaDisponivel(DateTime data, Aula aula)
    {
        for (int sala = 1; sala <= 5; sala++)
        {
            bool ocupada = await _context.AulasMarcadas
                .Include(am => am.Aula)
                .AnyAsync(am =>
                    am.DataDesativacao == null &&
                    am.DataAula == data &&
                    am.Sala == sala &&
                    am.Aula.HoraInicio < aula.HoraFim &&
                    am.Aula.HoraFim > aula.HoraInicio);

            if (!ocupada)
                return sala;
        }

        throw new InvalidOperationException("Não existem salas disponíveis para esta aula.");
    }

    public async Task<AulaMarcada> CreateAsync(ScheduleClassDto dto)
    {
        await using var transaction = await _context.Database
            .BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        var aula = await _context.Aulas
            .FirstOrDefaultAsync(a => a.IdAula == dto.IdAula && a.DataDesativacao == null)
            ?? throw new InvalidOperationException("Aula inválida ou desativada.");

        var dataCalendar = dto.DataAula.Date;
        var diaDaData = DiaSemanaHelper.FromDayOfWeek(dataCalendar.DayOfWeek);

        if (aula.DiaSemana != diaDaData)
            throw new InvalidOperationException("A data não coincide com o dia da semana da aula.");

        if (dto.Sala < 1 || dto.Sala > 5)
            throw new InvalidOperationException("Sala inválida. Valores permitidos: 1 a 5.");

        // já existe aula desta base nesta data
        bool jaExiste = await _context.AulasMarcadas
            .AnyAsync(a =>
                a.IdAula == dto.IdAula &&
                a.DataAula == dataCalendar &&
                a.DataDesativacao == null);

        if (jaExiste)
            throw new InvalidOperationException("Já existe uma aula marcada para esta data.");

        // valida conflito de sala sem SQL Server hints
        bool conflitoSala = await _context.AulasMarcadas
            .Include(am => am.Aula)
            .AnyAsync(am =>
                am.DataDesativacao == null &&
                am.DataAula == dataCalendar &&
                am.Sala == dto.Sala &&
                am.Aula.HoraInicio < aula.HoraFim &&
                am.Aula.HoraFim > aula.HoraInicio);

        if (conflitoSala)
            throw new InvalidOperationException("Já existe uma aula nesta sala neste horário.");

        var marcada = new AulaMarcada
        {
            IdAula = dto.IdAula,
            DataAula = dataCalendar,
            Sala = dto.Sala
        };

        _context.AulasMarcadas.Add(marcada);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return marcada;
    }


    // 🔹 Gerar automaticamente aulas para os próximos 15 dias
    public async Task<int> GenerateScheduledClassesForPtAsync(int idPt)
    {
        var aulasDoPt = await _context.Aulas
            .Where(a => a.DataDesativacao == null && a.IdFuncionario == idPt)
            .ToListAsync();

        if (!aulasDoPt.Any())
            throw new InvalidOperationException("Nenhuma aula base encontrada para este PT.");

        int aulasCriadas = 0;
        DateTime hoje = DateTime.UtcNow.Date;
        DateTime limite = hoje.AddDays(15);

        for (var data = hoje; data <= limite; data = data.AddDays(1))
        {
            foreach (var aula in aulasDoPt)
            {
                if (DiaSemanaHelper.FromDayOfWeek(data.DayOfWeek) != aula.DiaSemana)
                    continue;

                bool existe = await _context.AulasMarcadas
                    .AnyAsync(am => am.IdAula == aula.IdAula && am.DataAula == data && am.DataDesativacao == null);

                if (existe)
                    continue;

                int salaDisponivel = await ObterSalaDisponivel(data, aula);

                var marcada = new AulaMarcada
                {
                    IdAula = aula.IdAula,
                    DataAula = data,
                    Sala = salaDisponivel
                };

                _context.AulasMarcadas.Add(marcada);
                aulasCriadas++;
            }
        }

        await _context.SaveChangesAsync();
        return aulasCriadas;
    }

    // 🔹 Cancelar aula pelo PT
    public async Task<string> CancelByPtAsync(int idAulaMarcada)
    {
        var aula = await GetScheduledClassByIdAsync(idAulaMarcada)
            ?? throw new KeyNotFoundException("Aula não encontrada.");

        aula.DataDesativacao = DateTime.UtcNow;

        foreach (var r in aula.MembrosAulas)
            r.Presenca = Presenca.Cancelado;

        await _context.SaveChangesAsync();
        return "Aula cancelada manualmente pelo PT.";
    }

    // 🔹 Listar aulas futuras disponíveis
    public async Task<List<ScheduledClassResponseDto>> ListAvailableAsync()
    {
        var hoje = DateTime.UtcNow.Date;

        return await _context.AulasMarcadas
            .AsNoTracking()
            .Include(a => a.Aula)
                .ThenInclude(a => a.Funcionario)
            .Where(a => a.DataDesativacao == null && a.DataAula >= hoje)
            .OrderBy(a => a.DataAula)
            .Select(a => new ScheduledClassResponseDto
            {
                IdAulaMarcada = a.Id,
                IdAula = a.IdAula,
                Nome = a.Aula.Nome,
                DataAula = a.DataAula,
                HoraInicio = a.Aula.HoraInicio,
                HoraFim = a.Aula.HoraFim,
                Capacidade = a.Aula.Capacidade,
                Sala = a.Sala,
                ReservasAtuais = _context.MembrosAulas
                    .Where(m => m.IdAulaMarcada == a.Id && m.Presenca == Presenca.Reservado)
                    .Count(), // ✅ direto no banco
                NomeInstrutor = a.Aula.Funcionario.Nome
            })
            .ToListAsync();
    }

}
