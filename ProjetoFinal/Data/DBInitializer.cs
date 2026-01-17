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

        await context.Database.MigrateAsync();

        var hasher = new PasswordHasher<User>();

        // =============================
        // SUBSCRIÇÕES
        // =============================
        if (!context.Subscricoes.Any())
        {
            context.Subscricoes.AddRange(
                new Subscricao { Nome = "Mensal", Tipo = TipoSubscricao.Mensal, Preco = 30, Ativo = true },
                new Subscricao { Nome = "Trimestral", Tipo = TipoSubscricao.Trimestral, Preco = 80, Ativo = true },
                new Subscricao { Nome = "Anual", Tipo = TipoSubscricao.Anual, Preco = 200, Ativo = true },
                new Subscricao { Nome = "Promo Antiga", Tipo = TipoSubscricao.Mensal, Preco = 25, Ativo = false }
            );
            await context.SaveChangesAsync();
        }

        var subscricaoMensal = context.Subscricoes
            .Where(s => s.Tipo == TipoSubscricao.Mensal && s.Ativo)
            .OrderBy(s => s.IdSubscricao)
            .First();

        // =============================
        // USERS
        // =============================
        if (!context.Users.Any())
        {
            var users = new[]
            {
                new User { Email="admin@fit.local", Tipo=Tipo.Funcionario, Ativo=true },
                new User { Email="rececao@fit.local", Tipo=Tipo.Funcionario, Ativo=true },
                new User { Email="pt@fit.local", Tipo=Tipo.Funcionario, Ativo=true },
                new User { Email="pt2@fit.local", Tipo=Tipo.Funcionario, Ativo=true },

                new User { Email="m1@fit.local", Tipo=Tipo.Membro, Ativo=true },
                new User { Email="m2@fit.local", Tipo=Tipo.Membro, Ativo=true },
                new User { Email="m3@fit.local", Tipo=Tipo.Membro, Ativo=false },
                new User { Email="m4@fit.local", Tipo=Tipo.Membro, Ativo=true, PrimeiraVez=true }
            };

            foreach (var u in users)
                u.PasswordHash = hasher.HashPassword(u, "Teste@123!");

            context.Users.AddRange(users);
            await context.SaveChangesAsync();
        }

        // =============================
        // FUNCIONÁRIOS
        // =============================
        if (!context.Funcionarios.Any())
        {
            var admin = context.Users.Where(u => u.Email == "admin@fit.local").OrderBy(u => u.IdUser).First();
            var rececao = context.Users.Where(u => u.Email == "rececao@fit.local").OrderBy(u => u.IdUser).First();
            var pt1 = context.Users.Where(u => u.Email == "pt@fit.local").OrderBy(u => u.IdUser).First();
            var pt2 = context.Users.Where(u => u.Email == "pt2@fit.local").OrderBy(u => u.IdUser).First();

            context.Funcionarios.AddRange(
                new Funcionario { IdUser = admin.IdUser, Nome = "Admin", Funcao = Funcao.Admin, Telemovel = "910000001" },
                new Funcionario { IdUser = rececao.IdUser, Nome = "Receção", Funcao = Funcao.Rececao },
                new Funcionario { IdUser = pt1.IdUser, Nome = "PT João", Funcao = Funcao.PT },
                new Funcionario { IdUser = pt2.IdUser, Nome = "PT Maria", Funcao = Funcao.PT }
            );

            await context.SaveChangesAsync();
        }

        var ptPrincipal = context.Funcionarios
            .Where(f => f.Funcao == Funcao.PT)
            .OrderBy(f => f.IdFuncionario)
            .First();

        // =============================
        // MEMBROS
        // =============================
        if (!context.Membros.Any())
        {
            var membrosUsers = context.Users
                .Where(u => u.Tipo == Tipo.Membro)
                .OrderBy(u => u.IdUser)
                .ToList();

            context.Membros.AddRange(
                new Membro
                {
                    IdUser = membrosUsers[0].IdUser,
                    Nome = "Carlos Silva",
                    Telemovel = "920000001",
                    IdSubscricao = subscricaoMensal.IdSubscricao,
                    DataRegisto = DateTime.UtcNow.AddMonths(-6)
                },
                new Membro
                {
                    IdUser = membrosUsers[1].IdUser,
                    Nome = "Ana Costa",
                    Telemovel = "920000002",
                    IdSubscricao = subscricaoMensal.IdSubscricao,
                    DataRegisto = DateTime.UtcNow.AddMonths(-2)
                },
                new Membro
                {
                    IdUser = membrosUsers[2].IdUser,
                    Nome = "Membro Inativo",
                    Telemovel = "920000003",
                    IdSubscricao = subscricaoMensal.IdSubscricao,
                    DataRegisto = DateTime.UtcNow.AddYears(-1)
                }
            );

            await context.SaveChangesAsync();
        }

        var membroAtivo = context.Membros
            .OrderBy(m => m.IdMembro)
            .First();

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
