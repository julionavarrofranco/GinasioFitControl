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
            .FirstOrDefaultAsync(a => a.Id == idAulaMarcada);
    }

    public async Task<AulaMarcada> CreateAsync(ScheduleClassDto dto)
    {
        var aula = await _context.Aulas
            .FirstOrDefaultAsync(a => a.IdAula == dto.IdAula && a.DataDesativacao == null)
            ?? throw new InvalidOperationException("Aula inválida ou desativada.");

        // Usar apenas Year/Month/Day para evitar problemas de timezone na deserialização JSON
        var dataCalendar = new DateTime(dto.DataAula.Year, dto.DataAula.Month, dto.DataAula.Day);
        var diaDaData = DiaSemanaHelper.FromDayOfWeek(dataCalendar.DayOfWeek);
        if (aula.DiaSemana != diaDaData)
            throw new InvalidOperationException("A data da aula marcada não coincide com o dia da semana da aula.");

        if (await _context.AulasMarcadas.AnyAsync(a =>
                a.IdAula == dto.IdAula &&
                a.DataAula == dto.DataAula.Date &&
                a.DataDesativacao == null))
            throw new InvalidOperationException("Já existe uma aula marcada para esta data.");

        var marcada = new AulaMarcada { IdAula = dto.IdAula, DataAula = dto.DataAula.Date };
        _context.AulasMarcadas.Add(marcada);
        await _context.SaveChangesAsync();

        return marcada;
    }

    public async Task<int> GenerateScheduledClassesForPtAsync(int idPt)
    {
        // 1. Obter aulas base ativas do PT
        var aulasDoPt = await _context.Aulas
            .Where(a => a.DataDesativacao == null && a.IdFuncionario == idPt)
            .ToListAsync();

        if (!aulasDoPt.Any())
            throw new InvalidOperationException("Nenhuma aula base encontrada para este PT.");

        int aulasCriadas = 0;
        DateTime hoje = DateTime.UtcNow.Date;
        DateTime limite = hoje.AddDays(15);

        // 2. Percorrer cada dia até 15 dias à frente
        for (var data = hoje; data <= limite; data = data.AddDays(1))
        {
            foreach (var aula in aulasDoPt)
            {
                // Verifica se o dia da semana bate
                if (DiaSemanaHelper.FromDayOfWeek(data.DayOfWeek) != aula.DiaSemana)
                    continue;

                // Verifica se já existe uma aula marcada
                bool existe = await _context.AulasMarcadas
                    .AnyAsync(am => am.IdAula == aula.IdAula && am.DataAula == data && am.DataDesativacao == null);

                if (existe)
                    continue;

                // Cria a aula marcada
                var marcada = new AulaMarcada
                {
                    IdAula = aula.IdAula,
                    DataAula = data
                };
                _context.AulasMarcadas.Add(marcada);
                aulasCriadas++;
            }
        }

        await _context.SaveChangesAsync();
        return aulasCriadas; // retorna quantas aulas foram criadas
    }

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

    public async Task<List<ScheduledClassResponseDto>> ListAvailableAsync()
    {
        var hoje = DateTime.UtcNow.Date;

        return await _context.AulasMarcadas
            .AsNoTracking()
            .Include(a => a.Aula)
                .ThenInclude(a => a.Funcionario)
            .Where(a =>
                a.DataDesativacao == null &&
                a.DataAula >= hoje)
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
                ReservasAtuais = a.MembrosAulas
                    .Count(m => m.Presenca == Presenca.Reservado),
                NomeInstrutor = a.Aula.Funcionario.Nome
            })
            .ToListAsync();
    }

}
