using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Data;
using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;
using ProjetoFinal.Services.Interfaces;
using System.Linq;

public class ClassService : IClassService
{
    private readonly GinasioDbContext _context;

    public ClassService(GinasioDbContext context)
    {
        _context = context;
    }

    private async Task<Aula?> GetClassByIdAsync(int idAula, bool apenasAtivas = true)
    {
        var query = _context.Aulas
            .Include(a => a.Funcionario)
            .ThenInclude(f => f.User)
            .AsQueryable();

        if (apenasAtivas)
            query = query.Where(a => a.DataDesativacao == null);

        return await query.FirstOrDefaultAsync(a => a.IdAula == idAula);
    }

    // Criar aula
    public async Task<Aula> CreateAsync(ClassDto dto)
    {
        if (dto.DiaSemana == DiaSemana.Domingo)
            throw new InvalidOperationException("Não é permitido criar aulas ao domingo.");

        if (dto.HoraInicio >= dto.HoraFim)
            throw new InvalidOperationException("Hora inválida.");

        bool conflitoExiste = await _context.Aulas
        .AnyAsync(a =>
            a.DataDesativacao == null &&
            a.IdFuncionario == dto.IdFuncionario &&
            a.DiaSemana == dto.DiaSemana &&
            a.HoraInicio < dto.HoraFim &&
            a.HoraFim > dto.HoraInicio);

        if (conflitoExiste)
            throw new InvalidOperationException("O PT já tem uma aula neste horário.");

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

    // Atualização de aula com validação de conflitos e opção de swap
    public async Task<string> UpdateAsync(int idAula, UpdateClassDto dto)
    {
        var aula = await GetClassByIdAsync(idAula)
            ?? throw new KeyNotFoundException("Aula não encontrada.");

        // Novos valores considerando possíveis nulos
        var novaDia = dto.DiaSemana ?? aula.DiaSemana;
        var novaHoraInicio = dto.HoraInicio ?? aula.HoraInicio;
        var novaHoraFim = dto.HoraFim ?? aula.HoraFim;

        if (novaHoraInicio >= novaHoraFim)
            throw new InvalidOperationException("Hora inválida.");

        // Novo PT a checar
        var ptParaChecar = dto.IdFuncionario ?? aula.IdFuncionario;

        var conflito = await _context.Aulas
            .Where(a =>
                a.IdAula != idAula &&
                a.DataDesativacao == null &&
                a.IdFuncionario == ptParaChecar && 
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
                await SwapClassSlotsAsync(aula.IdAula, conflito.IdAula, true);
                return "Swap realizado com sucesso.";
            }
            else
            {
                throw new InvalidOperationException("Já existe uma aula neste horário.");
            }
        }

        bool alterado = false;

        if (aula.DiaSemana != novaDia) 
        { 
            aula.DiaSemana = novaDia; 
            alterado = true;     
        }

        if (aula.HoraInicio != novaHoraInicio) 
        {
            aula.HoraInicio = novaHoraInicio; 
            alterado = true; 
        }

        if (aula.HoraFim != novaHoraFim) 
        { 
            aula.HoraFim = novaHoraFim; 
            alterado = true; 
        }

        if (!string.IsNullOrWhiteSpace(dto.Nome) && dto.Nome != aula.Nome) 
        { 
            aula.Nome = dto.Nome;
            alterado = true; 
        }

        if (dto.Capacidade.HasValue && dto.Capacidade.Value != aula.Capacidade) 
        { 
            aula.Capacidade = dto.Capacidade.Value; 
            alterado = true; 
        }

        // Alteração de PT via AssignPtAsync
        if (dto.IdFuncionario.HasValue && dto.IdFuncionario.Value != aula.IdFuncionario)
        {
            await AssignPtAsync(aula.IdAula, dto.IdFuncionario.Value);
            alterado = true;
        }

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
    public async Task SwapClassSlotsAsync(int idAulaA, int idAulaB, bool forceSwap = false)
    {
        if (idAulaA == idAulaB)
            throw new InvalidOperationException("Não é possível trocar a mesma aula.");

        var aulas = await _context.Aulas
            .Where(a => (a.IdAula == idAulaA || a.IdAula == idAulaB) && a.DataDesativacao == null)
            .ToListAsync();

        if (aulas.Count != 2)
            throw new InvalidOperationException("Aulas inválidas para troca.");

        // Checar aulas marcadas futuras
        bool temMarcadasFuturas = await _context.AulasMarcadas.AnyAsync(am =>
            (am.IdAula == idAulaA || am.IdAula == idAulaB) &&
            am.DataAula > DateTime.UtcNow.Date &&
            am.DataDesativacao == null);

        if (temMarcadasFuturas)
            throw new InvalidOperationException("Não é possível trocar aulas com marcações futuras.");

        // Se não for forceSwap, impedir troca
        if (!forceSwap)
            throw new InvalidOperationException("Swap não autorizado sem forceSwap.");

        var aulaA = aulas.First(a => a.IdAula == idAulaA);
        var aulaB = aulas.First(a => a.IdAula == idAulaB);

        // Troca
        var diaTmp = aulaA.DiaSemana;
        var inicioTmp = aulaA.HoraInicio;
        var fimTmp = aulaA.HoraFim;

        aulaA.DiaSemana = aulaB.DiaSemana;
        aulaA.HoraInicio = aulaB.HoraInicio;
        aulaA.HoraFim = aulaB.HoraFim;

        aulaB.DiaSemana = diaTmp;
        aulaB.HoraInicio = inicioTmp;
        aulaB.HoraFim = fimTmp;

        await _context.SaveChangesAsync();
    }

    // Atribuir ou alterar PT de uma aula
    public async Task<ClassDto> AssignPtAsync(int idAula, int idPt)
    {
        var aula = await GetClassByIdAsync(idAula)
            ?? throw new KeyNotFoundException("Aula não encontrada.");

        // Verifica se já está atribuído o mesmo PT
        if (aula.IdFuncionario == idPt)
            throw new InvalidOperationException("Este Personal Trainer já está atribuído a esta aula.");

        // Buscar o funcionário
        var pt = await _context.Funcionarios
            .FirstOrDefaultAsync(f => f.IdFuncionario == idPt);

        if (pt == null)
            throw new KeyNotFoundException("Personal Trainer não encontrado.");

        // Validar se é realmente um PT
        if (pt.Funcao != Funcao.PT) // ou Enum Funcao.PT
            throw new InvalidOperationException("O funcionário selecionado não é um Personal Trainer.");

        aula.IdFuncionario = idPt;
        await _context.SaveChangesAsync();

        return new ClassDto
        {
            IdFuncionario = aula.IdFuncionario,
            Nome = aula.Nome,
            DiaSemana = aula.DiaSemana,
            HoraInicio = aula.HoraInicio,
            HoraFim = aula.HoraFim,
            Capacidade = aula.Capacidade

        };

    }

    // Ativar/desativar aula
    public async Task ChangeActiveStateAsync(int idAula, bool ativo)
    {
        var aula = await GetClassByIdAsync(idAula, false)
            ?? throw new KeyNotFoundException("Aula não encontrada.");

        if ((aula.DataDesativacao == null) != ativo)
        {
            aula.DataDesativacao = ativo ? null : DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    //Listar aulas por estado (ativas/inativas)
    public async Task<List<ClassResponseDto>> ListByStateAsync(bool ativo)
    {
        var aulas = await _context.Aulas
            .AsNoTracking()
            .Where(a => (ativo && a.DataDesativacao == null) || (!ativo && a.DataDesativacao != null))
            .Include(a => a.Funcionario)
            .ThenInclude(f => f.User)
            .OrderBy(a => a.DiaSemana).ThenBy(a => a.HoraInicio)
            .ToListAsync();

        return aulas.Select(a => new ClassResponseDto
        {
            IdAula = a.IdAula,
            IdFuncionario = a.IdFuncionario,
            Nome = a.Nome,
            DiaSemana = a.DiaSemana,
            HoraInicio = a.HoraInicio,
            HoraFim = a.HoraFim,
            Capacidade = a.Capacidade,
            DataDesativacao = a.DataDesativacao,
            Funcionario = a.Funcionario != null ? new FuncionarioSimpleDto
            {
                IdUser = a.Funcionario.IdUser,
                IdFuncionario = a.Funcionario.IdFuncionario,
                Nome = a.Funcionario.Nome,
                Email = a.Funcionario.User?.Email ?? "",
                Telemovel = a.Funcionario.Telemovel,
                Funcao = a.Funcionario.Funcao
            } : null
        }).ToList();
    }

    // Listar aulas por dia da semana
    public async Task<List<ClassResponseDto>> ListByDayAsync(DiaSemana dia)
    {
        var aulas = await _context.Aulas
            .AsNoTracking()
            .Where(a => a.DiaSemana == dia && a.DataDesativacao == null)
            .Include(a => a.Funcionario)
            .ThenInclude(f => f.User)
            .OrderBy(a => a.HoraInicio)
            .ToListAsync();

        return aulas.Select(a => new ClassResponseDto
        {
            IdAula = a.IdAula,
            IdFuncionario = a.IdFuncionario,
            Nome = a.Nome,
            DiaSemana = a.DiaSemana,
            HoraInicio = a.HoraInicio,
            HoraFim = a.HoraFim,
            Capacidade = a.Capacidade,
            DataDesativacao = a.DataDesativacao,
            Funcionario = a.Funcionario != null ? new FuncionarioSimpleDto
            {
                IdUser = a.Funcionario.IdUser,
                IdFuncionario = a.Funcionario.IdFuncionario,
                Nome = a.Funcionario.Nome,
                Email = a.Funcionario.User?.Email ?? "",
                Telemovel = a.Funcionario.Telemovel,
                Funcao = a.Funcionario.Funcao
            } : null
        }).ToList();
    }

    // Listar aulas por PT
    public async Task<List<ClassResponseDto>> ListByPtAsync(int idFuncionario)
    {
        var aulas = await _context.Aulas
            .AsNoTracking()
            .Where(a => a.IdFuncionario == idFuncionario && a.DataDesativacao == null)
            .Include(a => a.Funcionario)
            .ThenInclude(f => f.User)
            .OrderBy(a => a.DiaSemana).ThenBy(a => a.HoraInicio)
            .ToListAsync();

        return aulas.Select(a => new ClassResponseDto
        {
            IdAula = a.IdAula,
            IdFuncionario = a.IdFuncionario,
            Nome = a.Nome,
            DiaSemana = a.DiaSemana,
            HoraInicio = a.HoraInicio,
            HoraFim = a.HoraFim,
            Capacidade = a.Capacidade,
            DataDesativacao = a.DataDesativacao,
            Funcionario = a.Funcionario != null ? new FuncionarioSimpleDto
            {
                IdUser = a.Funcionario.IdUser,
                IdFuncionario = a.Funcionario.IdFuncionario,
                Nome = a.Funcionario.Nome,
                Email = a.Funcionario.User?.Email ?? "",
                Telemovel = a.Funcionario.Telemovel,
                Funcao = a.Funcionario.Funcao
            } : null
        }).ToList();
    }
}
