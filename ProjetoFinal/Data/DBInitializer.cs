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

        // Aplica migrations (UMA vez)
        await context.Database.MigrateAsync();

        // =============================
        // LIMPEZA DE DADOS (EF SAFE)
        // =============================

        context.MembrosAvaliacoes.RemoveRange(context.MembrosAvaliacoes);
        context.AvaliacoesFisicas.RemoveRange(context.AvaliacoesFisicas);
        context.MembrosAulas.RemoveRange(context.MembrosAulas);
        context.AulasMarcadas.RemoveRange(context.AulasMarcadas);
        context.Aulas.RemoveRange(context.Aulas);
        context.PlanosExercicios.RemoveRange(context.PlanosExercicios);
        context.Planos.RemoveRange(context.Planos);
        context.Exercicios.RemoveRange(context.Exercicios);
        context.Pagamentos.RemoveRange(context.Pagamentos);
        context.Membros.RemoveRange(context.Membros);
        context.Funcionarios.RemoveRange(context.Funcionarios);
        context.RefreshTokens.RemoveRange(context.RefreshTokens);
        context.Users.RemoveRange(context.Users);
        context.Subscricoes.RemoveRange(context.Subscricoes);

        await context.SaveChangesAsync();

        // =============================
        // SEED DE DADOS
        // =============================

        var hasher = new PasswordHasher<User>();

        // Subscrições
        var subscricoes = new[]
        {
            new Subscricao { Nome = "Mensal", Tipo = TipoSubscricao.Mensal, Preco = 29.99m },
            new Subscricao { Nome = "Trimestral", Tipo = TipoSubscricao.Trimestral, Preco = 79.99m },
            new Subscricao { Nome = "Anual", Tipo = TipoSubscricao.Anual, Preco = 199.99m }
        };

        context.Subscricoes.AddRange(subscricoes);
        await context.SaveChangesAsync();

        // Users
        var users = new List<User>
        {
            new() { Email = "admin@fit.local", Tipo = Tipo.Funcionario, Ativo = true },
            new() { Email = "rececao@fit.local", Tipo = Tipo.Funcionario, Ativo = true },
            new() { Email = "pt@fit.local", Tipo = Tipo.Funcionario, Ativo = true },
            new() { Email = "m1@fit.local", Tipo = Tipo.Membro, Ativo = true },
            new() { Email = "m2@fit.local", Tipo = Tipo.Membro, Ativo = true }
        };

        foreach (var user in users)
            user.PasswordHash = hasher.HashPassword(user, "Teste@123!");

        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        // Funcionários
        var admin = users.First(u => u.Email.Contains("admin"));
        var rececao = users.First(u => u.Email.Contains("rececao"));
        var ptUser = users.First(u => u.Email == "pt@fit.local");

        var funcionarios = new[]
{
            new Funcionario
            {
                IdUser = admin.IdUser,
                Nome = "Admin",
                Funcao = Funcao.Admin,
                Telemovel = "910000001"
            },
            new Funcionario
            {
                IdUser = rececao.IdUser,
                Nome = "Receção",
                Funcao = Funcao.Rececao,
                Telemovel = "910000002"
            },
            new Funcionario
            {
                IdUser = ptUser.IdUser,
                Nome = "PT",
                Funcao = Funcao.PT,
                Telemovel = "910000003"
            }
        };


        context.Funcionarios.AddRange(funcionarios);
        await context.SaveChangesAsync();

        // Membros
        var sub = subscricoes.First();

        var membros = new[]
        {
            new Membro { IdUser = users[3].IdUser, Nome = "Membro 1",  Telemovel = "920000001", IdSubscricao = sub.IdSubscricao, DataRegisto = DateTime.UtcNow },
            new Membro { IdUser = users[4].IdUser, Nome = "Membro 2",  Telemovel = "920000001", IdSubscricao = sub.IdSubscricao, DataRegisto = DateTime.UtcNow }
        };

        context.Membros.AddRange(membros);
        await context.SaveChangesAsync();

        // Exercícios
        var exercicios = new[]
        {
            new Exercicio { Nome = "Supino Reto", GrupoMuscular = GrupoMuscular.Peito, Descricao = "exercicio para peito", FotoUrl = "" },
            new Exercicio { Nome = "Agachamento", GrupoMuscular = GrupoMuscular.Pernas, Descricao = "exercicio para pernas", FotoUrl = "" }
        };

        context.Exercicios.AddRange(exercicios);
        await context.SaveChangesAsync();

        // Plano de treino
        var ptFuncionario = funcionarios.First(f => f.Funcao == Funcao.PT);

        var plano = new PlanoTreino
        {
            Nome = "Plano Inicial",
            IdFuncionario = ptFuncionario.IdFuncionario,
            DataCriacao = DateTime.UtcNow
        };

        context.Planos.Add(plano);
        await context.SaveChangesAsync();

        context.PlanosExercicios.AddRange(
            new PlanoExercicio { IdPlano = plano.IdPlano, IdExercicio = exercicios[0].IdExercicio, Series = 3, Repeticoes = 10 },
            new PlanoExercicio { IdPlano = plano.IdPlano, IdExercicio = exercicios[1].IdExercicio, Series = 3, Repeticoes = 12 }
        );

        await context.SaveChangesAsync();
    }
}
