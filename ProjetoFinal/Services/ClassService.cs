using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Data;
using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;
using ProjetoFinal.Services.Interfaces;

public class ClassService : IClassService
{
    private readonly GinasioDbContext _context;

    public ClassService(GinasioDbContext context)
    {
        _context = context;
    }

    private async Task<Aula?> GetClassByIdAsync(int idAula)
    {
        return await _context.Aulas
            .Include(a => a.Funcionario)
            .FirstOrDefaultAsync(a => a.IdAula == idAula && a.DataDesativacao == null);
    }

    public async Task<Aula> CreateAsync(ClassDto dto)
    {
        if (dto.DiaSemana == DiaSemana.Domingo)
            throw new InvalidOperationException("Não é permitido criar aulas ao domingo.");

        if (dto.HoraInicio >= dto.HoraFim)
            throw new InvalidOperationException("Hora inválida.");

        bool conflitoExiste = await _context.Aulas
            .AnyAsync(a =>
                a.DataDesativacao == null &&
                a.DiaSemana == dto.DiaSemana &&
                a.HoraInicio < dto.HoraFim &&
                a.HoraFim > dto.HoraInicio);

        if (conflitoExiste)
            throw new InvalidOperationException("Já existe uma aula neste horário.");

        var aula = new Aula
        {
            IdFuncionario = dto.IdFuncionario,
            Nome = dto.Nome,
            DiaSemana = dto.DiaSemana,
            HoraInicio = dto.HoraInicio,
            HoraFim = dto.HoraFim,
            Capacidade = dto.Capacidade
        };

        _context.Aulas.Add(aula);
        await _context.SaveChangesAsync();

        return aula;
    }

    public async Task<string> UpdateAsync(int idAula, UpdateClassDto dto)
    {
        var aula = await GetClassByIdAsync(idAula)
            ?? throw new KeyNotFoundException("Aula não encontrada.");

        var novaDia = dto.DiaSemana ?? aula.DiaSemana;
        var novaHoraInicio = dto.HoraInicio ?? aula.HoraInicio;
        var novaHoraFim = dto.HoraFim ?? aula.HoraFim;

        if (novaHoraInicio >= novaHoraFim)
            throw new InvalidOperationException("Hora inválida.");

        var conflito = await _context.Aulas
            .Where(a =>
                a.IdAula != idAula &&
                a.DataDesativacao == null &&
                a.DiaSemana == novaDia &&
                a.HoraInicio < novaHoraFim &&
                a.HoraFim > novaHoraInicio)
            .FirstOrDefaultAsync();

        if (conflito != null)
        {
            bool igual = conflito.HoraInicio == novaHoraInicio &&
                         conflito.HoraFim == novaHoraFim &&
                         conflito.DiaSemana == novaDia;

            if (igual && dto.ForceSwap)
            {
                await SwapClassSlotsAsync(aula.IdAula, conflito.IdAula);
                return "Swap realizado com sucesso.";
            }
            else
            {
                throw new InvalidOperationException("Já existe uma aula neste horário.");
            }
        }

        bool alterado = false;

        if (aula.DiaSemana != novaDia) { aula.DiaSemana = novaDia; alterado = true; }
        if (aula.HoraInicio != novaHoraInicio) { aula.HoraInicio = novaHoraInicio; alterado = true; }
        if (aula.HoraFim != novaHoraFim) { aula.HoraFim = novaHoraFim; alterado = true; }
        if (!string.IsNullOrWhiteSpace(dto.Nome) && dto.Nome != aula.Nome) { aula.Nome = dto.Nome; alterado = true; }
        if (dto.Capacidade.HasValue && dto.Capacidade.Value != aula.Capacidade) { aula.Capacidade = dto.Capacidade.Value; alterado = true; }
        if (dto.IdFuncionario.HasValue && dto.IdFuncionario.Value != aula.IdFuncionario) { aula.IdFuncionario = dto.IdFuncionario.Value; alterado = true; }

        if (alterado)
        {
            await _context.SaveChangesAsync();
            return "Aula atualizada com sucesso.";
        }
        else
        {
            return "Nenhuma alteração realizada.";
        }
    }

    // Swap permanece igual
    public async Task SwapClassSlotsAsync(int idAulaA, int idAulaB)
    {
        if (idAulaA == idAulaB)
            throw new InvalidOperationException("Não é possível trocar a mesma aula.");

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        var aulas = await _context.Aulas
            .Where(a =>
                (a.IdAula == idAulaA || a.IdAula == idAulaB) &&
                a.DataDesativacao == null)
            .ToListAsync();

        if (aulas.Count != 2)
            throw new InvalidOperationException("Aulas inválidas para troca.");

        var aulaA = aulas.First(a => a.IdAula == idAulaA);
        var aulaB = aulas.First(a => a.IdAula == idAulaB);

        // 🚫 Bloquear se houver aulas marcadas futuras
        bool temMarcadasFuturas = await _context.AulasMarcadas.AnyAsync(am =>
            (am.IdAula == idAulaA || am.IdAula == idAulaB) &&
            am.DataAula > DateTime.UtcNow.Date &&
            am.DataDesativacao == null);

        if (temMarcadasFuturas)
            throw new InvalidOperationException(
                "Não é possível trocar aulas com marcações futuras.");

        // Guardar slot A
        var diaTmp = aulaA.DiaSemana;
        var inicioTmp = aulaA.HoraInicio;
        var fimTmp = aulaA.HoraFim;

        // Troca completa do slot
        aulaA.DiaSemana = aulaB.DiaSemana;
        aulaA.HoraInicio = aulaB.HoraInicio;
        aulaA.HoraFim = aulaB.HoraFim;

        aulaB.DiaSemana = diaTmp;
        aulaB.HoraInicio = inicioTmp;
        aulaB.HoraFim = fimTmp;

        // Validação defensiva final
        if (aulaA.HoraInicio >= aulaA.HoraFim ||
            aulaB.HoraInicio >= aulaB.HoraFim)
            throw new InvalidOperationException("Slot inválido após a troca.");

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task<Aula> AssignPtAsync(int idAula, int idPt)
    {
        var aula = await GetClassByIdAsync(idAula)
            ?? throw new KeyNotFoundException("Aula não encontrada.");

        // Validar se PT existe no sistema
        var pt = await _context.Funcionarios.FirstOrDefaultAsync(f => f.IdFuncionario == idPt)
            ?? throw new InvalidOperationException("PT não encontrado.");

        aula.IdFuncionario = idPt;
        await _context.SaveChangesAsync();

        return aula;
    }


    public async Task ChangeActiveStateAsync(int idAula, bool ativo)
    {
        var aula = await GetClassByIdAsync(idAula)
            ?? throw new KeyNotFoundException("Aula não encontrada.");

        if ((aula.DataDesativacao == null) != ativo)
        {
            aula.DataDesativacao = ativo ? null : DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Aula> GetByIdAsync(int idAula)
    {
        return await GetClassByIdAsync(idAula)
            ?? throw new KeyNotFoundException("Aula não encontrada.");
    }

    public async Task<List<Aula>> ListByStateAsync(bool ativo)
    {
        return await _context.Aulas
            .AsNoTracking()
            .Where(a => (ativo && a.DataDesativacao == null) || (!ativo && a.DataDesativacao != null))
            .Include(a => a.Funcionario)
            .OrderBy(a => a.DiaSemana).ThenBy(a => a.HoraInicio)
            .ToListAsync();
    }

    // Listar aulas por dia da semana
    public async Task<List<Aula>> ListByDayAsync(DiaSemana dia)
    {
        return await _context.Aulas
            .AsNoTracking()
            .Where(a => a.DiaSemana == dia && a.DataDesativacao == null)
            .Include(a => a.Funcionario)
            .OrderBy(a => a.HoraInicio)
            .ToListAsync();
    }

    // Listar aulas por PT
    public async Task<List<Aula>> ListByPtAsync(int idFuncionario)
    {
        return await _context.Aulas
            .AsNoTracking()
            .Where(a => a.IdFuncionario == idFuncionario && a.DataDesativacao == null)
            .Include(a => a.Funcionario)
            .OrderBy(a => a.DiaSemana).ThenBy(a => a.HoraInicio)
            .ToListAsync();
    }
}
