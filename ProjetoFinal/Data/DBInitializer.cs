using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Data;
using ProjetoFinal.Models;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GinasioDbContext>();
        var hasher = new PasswordHasher<User>();

        await context.Database.MigrateAsync();

        // =============================
        // SUBSCRIÇÕES
        // =============================
        var subscricoesSeed = new[]
        {
            new Subscricao { Nome = "Mensal", Tipo = TipoSubscricao.Mensal, Preco = 30, Ativo = true },
            new Subscricao { Nome = "Trimestral", Tipo = TipoSubscricao.Trimestral, Preco = 80, Ativo = true },
            new Subscricao { Nome = "Anual", Tipo = TipoSubscricao.Anual, Preco = 200, Ativo = true },
            new Subscricao { Nome = "Promo Antiga", Tipo = TipoSubscricao.Mensal, Preco = 25, Ativo = false }
        };

        foreach (var s in subscricoesSeed)
        {
            if (!await context.Subscricoes.AnyAsync(x => x.Nome == s.Nome))
                context.Subscricoes.Add(s);
        }

        await context.SaveChangesAsync();

        var subscricaoMensal = await context.Subscricoes
            .FirstAsync(s => s.Tipo == TipoSubscricao.Mensal && s.Ativo);

        // =============================
        // USERS
        // =============================
        var usersSeed = new[]
        {
            ("admin@fit.local", Tipo.Funcionario),
            ("rececao@fit.local", Tipo.Funcionario),
            ("pt@fit.local", Tipo.Funcionario),
            ("pt2@fit.local", Tipo.Funcionario),

            ("m1@fit.local", Tipo.Membro),
            ("m2@fit.local", Tipo.Membro),
            ("m3@fit.local", Tipo.Membro),
            ("m4@fit.local", Tipo.Membro)
        };

        foreach (var (email, tipo) in usersSeed)
        {
            if (await context.Users.AnyAsync(u => u.Email == email))
                continue;

            var user = new User
            {
                Email = email,
                Tipo = tipo,
                Ativo = true,
                PrimeiraVez = true
            };

            user.PasswordHash = hasher.HashPassword(user, "Teste@123!");
            context.Users.Add(user);
        }

        await context.SaveChangesAsync();

        // =============================
        // FUNCIONÁRIOS (CRÍTICO)
        // =============================
        var funcionariosSeed = new[]
        {
            ("admin@fit.local", "Admin", Funcao.Admin, "910000001"),
            ("rececao@fit.local", "Receção", Funcao.Rececao, null),
            ("pt@fit.local", "PT João", Funcao.PT, null),
            ("pt2@fit.local", "PT Maria", Funcao.PT, null)
        };

        foreach (var (email, nome, funcao, telemovel) in funcionariosSeed)
        {
            var user = await context.Users.FirstAsync(u => u.Email == email);

            if (user.Tipo != Tipo.Funcionario)
                continue;

            if (await context.Funcionarios.AnyAsync(f => f.IdUser == user.IdUser))
                continue;

            context.Funcionarios.Add(new Funcionario
            {
                IdUser = user.IdUser,
                Nome = nome,
                Funcao = funcao,
                Telemovel = telemovel ?? "910000000"
            });
        }

        await context.SaveChangesAsync();

        var ptPrincipal = await context.Funcionarios
            .Where(f => f.Funcao == Funcao.PT)
            .OrderBy(f => f.IdFuncionario)
            .FirstAsync();

        // =============================
        // MEMBROS
        // =============================
        var membrosSeed = new[]
        {
            ("m1@fit.local", "Carlos Silva", "920000001"),
            ("m2@fit.local", "Ana Costa", "920000002"),
            ("m3@fit.local", "Membro Inativo", "920000003"),
            ("m4@fit.local", "Novo Membro", "920000004")
        };

        foreach (var (email, nome, telemovel) in membrosSeed)
        {
            var user = await context.Users.FirstAsync(u => u.Email == email);

            if (user.Tipo != Tipo.Membro)
                continue;

            if (await context.Membros.AnyAsync(m => m.IdUser == user.IdUser))
                continue;

            context.Membros.Add(new Membro
            {
                IdUser = user.IdUser,
                Nome = nome,
                Telemovel = telemovel,
                IdSubscricao = subscricaoMensal.IdSubscricao,
                DataRegisto = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();

        var membroAtivo = await context.Membros.FirstAsync();

        // =============================
        // EXERCÍCIOS
        // =============================
        if (!context.Exercicios.Any())
        {
            context.Exercicios.AddRange(
                new Exercicio { Nome = "Supino Reto", GrupoMuscular = GrupoMuscular.Peito, Ativo = true },
                new Exercicio { Nome = "Agachamento", GrupoMuscular = GrupoMuscular.Pernas, Ativo = true },
                new Exercicio { Nome = "Remada", GrupoMuscular = GrupoMuscular.Costas, Ativo = true },
                new Exercicio { Nome = "Bíceps Curl", GrupoMuscular = GrupoMuscular.Bracos, Ativo = false }
            );
            await context.SaveChangesAsync();
        }

        // =============================
        // PLANO TREINO + EXERCÍCIOS
        // =============================
        if (!context.Planos.Any())
        {
            var plano = new PlanoTreino
            {
                Nome = "Plano Hipertrofia",
                IdFuncionario = ptPrincipal.IdFuncionario,
                DataCriacao = DateTime.UtcNow.AddMonths(-1)
            };

            context.Planos.Add(plano);
            await context.SaveChangesAsync();

            var exerciciosAtivos = context.Exercicios
                .Where(e => e.Ativo)
                .OrderBy(e => e.IdExercicio)
                .ToList();

            context.PlanosExercicios.AddRange(
                new PlanoExercicio { IdPlano = plano.IdPlano, IdExercicio = exerciciosAtivos[0].IdExercicio, Series = 4, Repeticoes = 10, Carga = 60 },
                new PlanoExercicio { IdPlano = plano.IdPlano, IdExercicio = exerciciosAtivos[1].IdExercicio, Series = 4, Repeticoes = 12 }
            );

            await context.SaveChangesAsync();
        }

        // =============================
        // AULAS + AULAS MARCADAS
        // =============================
        if (!context.Aulas.Any())
        {
            var aula = new Aula
            {
                Nome = "Yoga",
                DiaSemana = DiaSemana.Segunda,
                HoraInicio = new TimeSpan(18, 0, 0),
                HoraFim = new TimeSpan(19, 0, 0),
                Capacidade = 15,
                IdFuncionario = ptPrincipal.IdFuncionario
            };

            context.Aulas.Add(aula);
            await context.SaveChangesAsync();

            context.AulasMarcadas.AddRange(
                new AulaMarcada { IdAula = aula.IdAula, DataAula = DateTime.UtcNow.AddDays(-7) },
                new AulaMarcada { IdAula = aula.IdAula, DataAula = DateTime.UtcNow },
                new AulaMarcada { IdAula = aula.IdAula, DataAula = DateTime.UtcNow.AddDays(7) }
            );

            await context.SaveChangesAsync();
        }

        // =============================
        // RESERVA AULA
        // =============================
        if (!context.MembrosAulas.Any())
        {
            var aulaMarcada = context.AulasMarcadas
                .OrderBy(a => a.DataAula)
                .First();

            context.MembrosAulas.Add(
                new MembroAula
                {
                    IdMembro = membroAtivo.IdMembro,
                    IdAulaMarcada = aulaMarcada.Id,
                    DataReserva = DateTime.UtcNow.AddDays(-8),
                    Presenca = Presenca.Presente
                }
            );

            await context.SaveChangesAsync();
        }

        // =============================
        // AVALIAÇÃO FÍSICA
        // =============================
        if (!context.AvaliacoesFisicas.Any())
        {
            context.AvaliacoesFisicas.Add(
                new AvaliacaoFisica
                {
                    IdMembro = membroAtivo.IdMembro,
                    IdFuncionario = ptPrincipal.IdFuncionario,
                    DataAvaliacao = DateTime.UtcNow.AddMonths(-1),
                    Peso = 82,
                    Altura = 1.75m,
                    Imc = 26.78m,
                    MassaGorda = 18,
                    Observacoes = "Avaliação inicial"
                }
            );

            await context.SaveChangesAsync();
        }

        // =============================
        // PAGAMENTOS
        // =============================
        if (!context.Pagamentos.Any())
        {
            context.Pagamentos.AddRange(
                new Pagamento
                {
                    IdMembro = membroAtivo.IdMembro,
                    IdSubscricao = subscricaoMensal.IdSubscricao,
                    ValorPago = 30,
                    MetodoPagamento = MetodoPagamento.Cartao,
                    EstadoPagamento = EstadoPagamento.Pago,
                    MesReferente = DateTime.UtcNow.AddMonths(-1),
                    DataPagamento = DateTime.UtcNow.AddMonths(-1)
                },
                new Pagamento
                {
                    IdMembro = membroAtivo.IdMembro,
                    IdSubscricao = subscricaoMensal.IdSubscricao,
                    ValorPago = 30,
                    MetodoPagamento = MetodoPagamento.MBWay,
                    EstadoPagamento = EstadoPagamento.Pendente,
                    MesReferente = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();
        }

        // =============================
        // RESERVAS AVALIAÇÃO FÍSICA
        // =============================
        if (!context.MembrosAvaliacoes.Any())
        {
            var avaliacao = context.AvaliacoesFisicas
                .OrderBy(a => a.IdAvaliacao)
                .First();

            context.MembrosAvaliacoes.AddRange(
                new MembroAvaliacao
                {
                    IdMembro = membroAtivo.IdMembro,
                    DataReserva = DateTime.UtcNow.AddDays(7),
                    Estado = EstadoAvaliacao.Reservado
                },
                new MembroAvaliacao
                {
                    IdMembro = membroAtivo.IdMembro,
                    DataReserva = avaliacao.DataAvaliacao,
                    Estado = EstadoAvaliacao.Presente,
                    IdAvaliacaoFisica = avaliacao.IdAvaliacao
                },
                new MembroAvaliacao
                {
                    IdMembro = membroAtivo.IdMembro,
                    DataReserva = DateTime.UtcNow.AddDays(-3),
                    Estado = EstadoAvaliacao.Cancelado,
                    DataCancelamento = DateTime.UtcNow.AddDays(-3)
                }
            );

            await context.SaveChangesAsync();
        }
    }
}
