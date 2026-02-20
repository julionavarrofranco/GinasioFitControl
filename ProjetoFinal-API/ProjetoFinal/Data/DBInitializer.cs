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

        // subscrições
        var subscricoesSeed = new[]
        {
            new Subscricao
            {
                Nome = "Subscrição Mensal",
                Tipo = TipoSubscricao.Mensal,
                Preco = 25.99m,
                Descricao = "Subscrição básica mensal",
                Ativo = true
            },
            new Subscricao
            {
                Nome = "Subscrição Trimestral",
                Tipo = TipoSubscricao.Trimestral,
                Preco = 79.99m,
                Descricao = "Subscrição básica trimestral",
                Ativo = true
            },
            new Subscricao
            {
                Nome = "Subscrição Anual",
                Tipo = TipoSubscricao.Anual,
                Preco = 249.99m,
                Descricao = "Subscrição básica anual",
                Ativo = true
            },
            new Subscricao
            {
                Nome = "Subscrição 3 por 2",
                Tipo = TipoSubscricao.Trimestral,
                Preco = 54.99m,
                Descricao = "Promoção 3 meses pelo preço de 2. Apenas aplicado ao 1º pagamento trimestral",
                Ativo = false
            }
        };

        foreach (var s in subscricoesSeed)
            if (!await context.Subscricoes.AnyAsync(x => x.Nome == s.Nome))
                context.Subscricoes.Add(s);

        await context.SaveChangesAsync();
        var subscricaoMensal = await context.Subscricoes.FirstAsync(s => s.Tipo == TipoSubscricao.Mensal && s.Ativo);

        // users
        var usersSeed = new[]
        {
            ("admin@fit.local", Tipo.Funcionario),
            ("rececao@fit.local", Tipo.Funcionario),
            ("pt1@fit.local", Tipo.Funcionario),
            ("pt2@fit.local", Tipo.Funcionario),
            ("m1@fit.local", Tipo.Membro),
            ("m2@fit.local", Tipo.Membro),
            ("m3@fit.local", Tipo.Membro),
            ("m4@fit.local", Tipo.Membro)
        };

        foreach (var (email, tipo) in usersSeed)
        {
            if (await context.Users.AnyAsync(u => u.Email == email)) continue;

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

        // funcionários
        var funcionariosSeed = new[]
        {
            ("admin@fit.local", "Carlos Gouveia", Funcao.Admin, "+351901200400"),
            ("rececao@fit.local", "Mamadu Baldé", Funcao.Rececao, "+351963107354"),
            ("pt1@fit.local", "João Andrade", Funcao.PT, "+351926147020"),
            ("pt2@fit.local", "Maria Antonieta", Funcao.PT, "+351900123456")
        };

        foreach (var (email, nome, funcao, telemovel) in funcionariosSeed)
        {
            var user = await context.Users.FirstAsync(u => u.Email == email);
            if (user.Tipo != Tipo.Funcionario) continue;
            if (await context.Funcionarios.AnyAsync(f => f.IdUser == user.IdUser)) continue;

            context.Funcionarios.Add(new Funcionario
            {
                IdUser = user.IdUser,
                Nome = nome,
                Funcao = funcao,
                Telemovel = telemovel ?? "910000000"
            });
        }

        await context.SaveChangesAsync();
        var ptPrincipal = await context.Funcionarios.Where(f => f.Funcao == Funcao.PT).OrderBy(f => f.IdFuncionario).FirstAsync();

        // membros
        var membrosSeed = new[]
        {
            ("m1@fit.local", "Carlos Silva", "+351920000001", new DateTime(1995, 5, 10)),
            ("m2@fit.local", "Ana Costa", "+351920000002", new DateTime(1998, 8, 20)),
            ("m3@fit.local", "David Lopes", "+351920000003", new DateTime(1990, 3, 15)),
            ("m4@fit.local", "Mohammed Salem", "+351920000004", new DateTime(2000, 11, 2))
        };

        foreach (var (email, nome, telemovel, dataNasc) in membrosSeed)
        {
            var user = await context.Users.FirstAsync(u => u.Email == email);
            if (user.Tipo != Tipo.Membro) continue;
            if (await context.Membros.AnyAsync(m => m.IdUser == user.IdUser)) continue;

            context.Membros.Add(new Membro
            {
                IdUser = user.IdUser,
                Nome = nome,
                Telemovel = telemovel,
                DataNascimento = dataNasc,
                IdSubscricao = subscricaoMensal.IdSubscricao,
                DataRegisto = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
        var membros = await context.Membros.OrderBy(m => m.IdMembro).ToListAsync();

        // exercícios
        if (!await context.Exercicios.AnyAsync())
        {
            context.Exercicios.AddRange(
                new Exercicio
                {
                    Nome = "Supino Reto",
                    GrupoMuscular = GrupoMuscular.Peito,
                    Descricao = "Exercício composto para desenvolvimento do peitoral maior.",
                    FotoUrl = "https://i.imgur.com/5LXHpqT.jpeg",
                    Ativo = true
                },
                new Exercicio
                {
                    Nome = "Supino Inclinado",
                    GrupoMuscular = GrupoMuscular.Peito,
                    Descricao = "Foco na parte superior do peitoral.",
                    FotoUrl = "https://i.imgur.com/WH15pZ8.jpeg",
                    Ativo = true
                },
                new Exercicio
                {
                    Nome = "Agachamento",
                    GrupoMuscular = GrupoMuscular.Pernas,
                    Descricao = "Exercício base para desenvolvimento dos membros inferiores.",
                    FotoUrl = "https://i.imgur.com/e71xRWj.jpeg",
                    Ativo = true
                },
                new Exercicio
                {
                    Nome = "Leg Press",
                    GrupoMuscular = GrupoMuscular.Pernas,
                    Descricao = "Exercício guiado para fortalecimento de quadríceps e glúteos.",
                    FotoUrl = "https://i.imgur.com/qBw50ub.jpeg",
                    Ativo = true
                },
                new Exercicio
                {
                    Nome = "Remada com Barra",
                    GrupoMuscular = GrupoMuscular.Costas,
                    Descricao = "Fortalecimento da musculatura dorsal e lombar.",
                    FotoUrl = "https://i.imgur.com/a4qUo8P.jpeg",
                    Ativo = true
                },
                new Exercicio
                {
                    Nome = "Puxada na Polia",
                    GrupoMuscular = GrupoMuscular.Costas,
                    Descricao = "Trabalho de dorsais em movimento vertical.",
                    FotoUrl = "https://i.imgur.com/CcHR4Lt.jpeg",
                    Ativo = true
                },
                new Exercicio
                {
                    Nome = "Desenvolvimento de Ombros",
                    GrupoMuscular = GrupoMuscular.Ombros,
                    Descricao = "Exercício para fortalecimento dos deltoides.",
                    FotoUrl = "/images/exercicios/desenvolvimento_ombros.jpg",
                    Ativo = true
                },
                new Exercicio
                {
                    Nome = "Elevação Lateral",
                    GrupoMuscular = GrupoMuscular.Ombros,
                    Descricao = "Isolamento da porção lateral dos ombros.",
                    FotoUrl = "/images/exercicios/elevacao_lateral.jpg",
                    Ativo = true
                },
                new Exercicio
                {
                    Nome = "Bíceps Curl",
                    GrupoMuscular = GrupoMuscular.Bracos,
                    Descricao = "Isolamento do bíceps braquial.",
                    FotoUrl = "https://i.imgur.com/3MKEBrH.jpeg",
                    Ativo = false
                },
                new Exercicio
                {
                    Nome = "Tríceps Testa",
                    GrupoMuscular = GrupoMuscular.Bracos,
                    Descricao = "Exercício de isolamento para tríceps.",
                    FotoUrl = "/images/exercicios/triceps_testa.jpg",
                    Ativo = true
                },
                new Exercicio
                {
                    Nome = "Prancha Abdominal",
                    GrupoMuscular = GrupoMuscular.Abdominais,
                    Descricao = "Exercício isométrico para fortalecimento do core.",
                    FotoUrl = "/images/exercicios/prancha.jpg",
                    Ativo = true
                },
                new Exercicio
                {
                    Nome = "Burpees",
                    GrupoMuscular = GrupoMuscular.CorpoInteiro,
                    Descricao = "Exercício funcional que trabalha o corpo inteiro.",
                    FotoUrl = "/images/exercicios/burpees.jpg",
                    Ativo = true
                }
            );

            await context.SaveChangesAsync();
        }

        var exercicios = await context.Exercicios.Where(e => e.Ativo).OrderBy(e => e.IdExercicio).ToListAsync();

        // planos de treino + exercícios associados
        if (!await context.Planos.AnyAsync())
        {
            var plano1 = new PlanoTreino
            {
                Nome = "Plano Hipertrofia",
                IdFuncionario = ptPrincipal.IdFuncionario,
                DataCriacao = DateTime.UtcNow.AddMonths(-1)
            };
            var plano2 = new PlanoTreino
            {
                Nome = "Plano Perda de Gordura",
                IdFuncionario = ptPrincipal.IdFuncionario,
                DataCriacao = DateTime.UtcNow.AddDays(-20)
            };
            var plano3 = new PlanoTreino
            {
                Nome = "Plano Iniciante Full Body",
                IdFuncionario = ptPrincipal.IdFuncionario,
                DataCriacao = DateTime.UtcNow.AddDays(-10)
            };

            context.Planos.AddRange(plano1, plano2, plano3);
            await context.SaveChangesAsync();

            context.PlanosExercicios.AddRange(
                new PlanoExercicio
                {
                    IdPlano = plano1.IdPlano,
                    IdExercicio = exercicios[0].IdExercicio,
                    Series = 4,
                    Repeticoes = 10,
                    Carga = 60,
                    Ordem = 1
                },
                new PlanoExercicio
                {
                    IdPlano = plano1.IdPlano,
                    IdExercicio = exercicios[1].IdExercicio,
                    Series = 4,
                    Repeticoes = 12,
                    Carga = 40,
                    Ordem = 2
                },
                new PlanoExercicio
                {
                    IdPlano = plano2.IdPlano,
                    IdExercicio = exercicios[2].IdExercicio,
                    Series = 3,
                    Repeticoes = 15,
                    Carga = 50,
                    Ordem = 1
                },
                new PlanoExercicio
                {
                    IdPlano = plano2.IdPlano,
                    IdExercicio = exercicios[3].IdExercicio,
                    Series = 3,
                    Repeticoes = 20,
                    Carga = 60,
                    Ordem = 2
                },
                new PlanoExercicio
                {
                    IdPlano = plano3.IdPlano,
                    IdExercicio = exercicios[4].IdExercicio,
                    Series = 3,
                    Repeticoes = 12,
                    Carga = 40,
                    Ordem = 1
                },
                new PlanoExercicio
                {
                    IdPlano = plano3.IdPlano,
                    IdExercicio = exercicios[5].IdExercicio,
                    Series = 3,
                    Repeticoes = 12,
                    Carga = 40,
                    Ordem = 2
                }
            );

            await context.SaveChangesAsync();

            // Atribuir planos apenas aos 2 primeiros membros
            membros[0].IdPlanoTreino = plano1.IdPlano;
            membros[1].IdPlanoTreino = plano2.IdPlano;

            await context.SaveChangesAsync();
        }

        // pagamentos
        foreach (var membro in membros.Take(2))
        {
            if (!await context.Pagamentos.AnyAsync(p => p.IdMembro == membro.IdMembro))
            {
                context.Pagamentos.Add(new Pagamento
                {
                    IdMembro = membro.IdMembro,
                    IdSubscricao = membro.IdSubscricao,
                    DataPagamento = DateTime.UtcNow.AddDays(-30),
                    ValorPago = 25.99m,
                    MetodoPagamento = MetodoPagamento.Cartao,
                    EstadoPagamento = EstadoPagamento.Pago,
                    MesReferente = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1),
                    DataRegisto = DateTime.UtcNow
                });
            }
        }
        await context.SaveChangesAsync();

        // avaliações físicas
        if (!await context.AvaliacoesFisicas.AnyAsync())
        {
            var pt = await context.Funcionarios.FirstAsync(f => f.Funcao == Funcao.PT);

            // Membro 1
            context.AvaliacoesFisicas.AddRange(
                new AvaliacaoFisica
                {
                    IdMembro = membros[0].IdMembro,
                    IdFuncionario = pt.IdFuncionario,
                    DataAvaliacao = DateTime.UtcNow.AddMonths(-2),
                    Peso = 85,
                    Altura = 1.75m,
                    Imc = 27.75m,
                    MassaMuscular = 30,
                    MassaGorda = 20,
                    Observacoes = "Avaliação inicial"
                },
                new AvaliacaoFisica
                {
                    IdMembro = membros[0].IdMembro,
                    IdFuncionario = pt.IdFuncionario,
                    DataAvaliacao = DateTime.UtcNow.AddDays(-10),
                    Peso = 82,
                    Altura = 1.75m,
                    Imc = 26.78m,
                    MassaMuscular = 24,
                    MassaGorda = 18,
                    Observacoes = "Evolução positiva"
                }
            );

            // Membro 2
            context.AvaliacoesFisicas.Add(
                new AvaliacaoFisica
                {
                    IdMembro = membros[1].IdMembro,
                    IdFuncionario = pt.IdFuncionario,
                    DataAvaliacao = DateTime.UtcNow.AddDays(-5),
                    Peso = 70,
                    Altura = 1.65m,
                    Imc = 25.71m,
                    MassaMuscular = 41,
                    MassaGorda = 22,
                    Observacoes = "Primeira avaliação"
                }
            );

            await context.SaveChangesAsync();
        }

        // aulasMarcadas + reservas de aulas
        if (!await context.Aulas.AnyAsync())
        {
            var pt = await context.Funcionarios.FirstAsync(f => f.Funcao == Funcao.PT);

            context.Aulas.AddRange(
                new Aula
                {
                    Nome = "HIIT",
                    DiaSemana = DiaSemana.Sexta,
                    HoraInicio = new TimeSpan(18, 0, 0),
                    HoraFim = new TimeSpan(19, 0, 0),
                    Capacidade = 15,
                    IdFuncionario = pt.IdFuncionario
                },
                new Aula
                {
                    Nome = "Alongamentos",
                    DiaSemana = DiaSemana.Sabado,
                    HoraInicio = new TimeSpan(10, 0, 0),
                    HoraFim = new TimeSpan(11, 0, 0),
                    Capacidade = 12,
                    IdFuncionario = pt.IdFuncionario
                },
                new Aula
                {
                    Nome = "Funcional Outdoor",
                    DiaSemana = DiaSemana.Domingo,
                    HoraInicio = new TimeSpan(9, 30, 0),
                    HoraFim = new TimeSpan(10, 30, 0),
                    Capacidade = 20,
                    IdFuncionario = pt.IdFuncionario
                }
            );

            await context.SaveChangesAsync();
        }

        if (!await context.AulasMarcadas.AnyAsync(a => a.DataAula > DateTime.UtcNow))
        {
            var aulas = await context.Aulas.ToListAsync();
            foreach (var aula in aulas)
            {
                context.AulasMarcadas.Add(new AulaMarcada
                {
                    IdAula = aula.IdAula,
                    DataAula = DateTime.UtcNow.Date.AddDays(14),
                    Sala = 1
                });
            }
            await context.SaveChangesAsync();
        }

        if (!await context.MembrosAulas.AnyAsync())
        {
            var membro = membros[0];
            var aulaMarcada = await context.AulasMarcadas.FirstAsync();
            context.MembrosAulas.Add(new MembroAula
            {
                IdMembro = membro.IdMembro,
                IdAulaMarcada = aulaMarcada.Id,
                DataReserva = DateTime.UtcNow,
                Presenca = Presenca.Reservado
            });
            await context.SaveChangesAsync();
        }
    }
}