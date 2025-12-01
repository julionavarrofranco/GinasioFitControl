using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Data;
using ProjetoFinal.Models;

namespace ProjetoFinal
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var provider = scope.ServiceProvider;
            var context = provider.GetRequiredService<GinasioDbContext>();

            // Opcional: garante que as migrations foram aplicadas
            await context.Database.MigrateAsync();

            // Guard: não seedar se já existir qualquer user (ajusta conforme desejado)
            if (await context.Users.AnyAsync())
                return;

            // Usar transaction para atomicidade
            await using var tx = await context.Database.BeginTransactionAsync();
            try
            {
                var hasher = new PasswordHasher<User>();

                // 1) Subscrições de exemplo (se a tabela estiver vazia)
                if (!await context.Subscricoes.AnyAsync())
                {
                    var subs = new List<Subscricao>
                    {
                        new Subscricao { Tipo = TipoSubscricao.Mensal, Preco = 29.99m, Descricao = "Mensal standard" },
                        new Subscricao { Tipo = TipoSubscricao.Trimestral, Preco = 79.99m, Descricao = "3 meses" },
                        new Subscricao { Tipo = TipoSubscricao.Anual, Preco = 199.99m, Descricao = "12 meses" }
                    };
                    context.Subscricoes.AddRange(subs);
                    await context.SaveChangesAsync();
                }

                // 2) Criar USERS (funcionários e membros)
                // Senhas em texto para testes — ajusta para ambiente real
                var users = new List<(User user, string plainPassword, Action createLinkedEntity)>
                {
                    // Admin funcionário
                    ( new User {
                        Email = "admin@fit.local",
                        Tipo = Tipo.Funcionario,
                        PrimeiraVez = false,
                        Ativo = true
                      },
                      "Admin@123!",
                      () => {
                        // placeholder - cria depois após SaveChanges (ver abaixo)
                      }
                    ),

                    // Receção
                    ( new User {
                        Email = "rececao@fit.local",
                        Tipo = Tipo.Funcionario,
                        PrimeiraVez = false,
                        Ativo = true
                      },
                      "Recepcao@123!",
                      () => {}
                    ),

                    // PT
                    ( new User {
                        Email = "pt@fit.local",
                        Tipo = Tipo.Funcionario,
                        PrimeiraVez = false,
                        Ativo = true
                      },
                      "PT@123!",
                      () => {}
                    ),

                    // Membros de exemplo
                    ( new User {
                        Email = "m1@fit.local",
                        Tipo = Tipo.Membro,
                        PrimeiraVez = false,
                        Ativo = true
                      },
                      "Membro1@123!",
                      () => {}
                    ),

                    ( new User {
                        Email = "m2@fit.local",
                        Tipo = Tipo.Membro,
                        PrimeiraVez = false,
                        Ativo = true
                      },
                      "Membro2@123!",
                      () => {}
                    )
                };

                // Adiciona users e gera password hashes
                foreach (var (user, plainPassword, _) in users)
                {
                    user.PasswordHash = hasher.HashPassword(user, plainPassword);
                    context.Users.Add(user);
                }

                await context.SaveChangesAsync(); // para obter IdUser

                // 3) Criar Funcionarios e Membros com base nos Users criados
                // Obter users recém-criados do BD para ter IdUser
                var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@fit.local");
                var rececaoUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "rececao@fit.local");
                var ptUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "pt@fit.local");

                var membro1User = await context.Users.FirstOrDefaultAsync(u => u.Email == "m1@fit.local");
                var membro2User = await context.Users.FirstOrDefaultAsync(u => u.Email == "m2@fit.local");

                // Criar Funcionários
                if (adminUser != null)
                {
                    context.Funcionarios.Add(new Funcionario
                    {
                        IdUser = adminUser.IdUser,
                        Nome = "Admin Teste",
                        Telemovel = "911000000",
                        Funcao = Funcao.Admin
                    });
                }

                if (rececaoUser != null)
                {
                    context.Funcionarios.Add(new Funcionario
                    {
                        IdUser = rececaoUser.IdUser,
                        Nome = "Rececao Teste",
                        Telemovel = "912000000",
                        Funcao = Funcao.Rececao
                    });
                }

                if (ptUser != null)
                {
                    context.Funcionarios.Add(new Funcionario
                    {
                        IdUser = ptUser.IdUser,
                        Nome = "PT Teste",
                        Telemovel = "913000000",
                        Funcao = Funcao.PT
                    });
                }

                // Criar Membros
                // Usa a primeira subscrição existente (ajusta se quiseres mapear especificamente)
                var primeiraSub = await context.Subscricoes.FirstOrDefaultAsync();
                if (primeiraSub == null)
                    throw new InvalidOperationException("Subscrição de teste não encontrada.");

                if (membro1User != null)
                {
                    context.Membros.Add(new Membro
                    {
                        IdUser = membro1User.IdUser,
                        Nome = "Membro Teste 1",
                        Telemovel = "914000000",
                        DataNascimento = new DateTime(1990, 1, 1),
                        DataRegisto = DateTime.UtcNow.Date,
                        IdSubscricao = primeiraSub.IdSubscricao
                    });
                }

                if (membro2User != null)
                {
                    context.Membros.Add(new Membro
                    {
                        IdUser = membro2User.IdUser,
                        Nome = "Membro Teste 2",
                        Telemovel = "915000000",
                        DataNascimento = new DateTime(1992, 5, 15),
                        DataRegisto = DateTime.UtcNow.Date,
                        IdSubscricao = primeiraSub.IdSubscricao
                    });
                }

                await context.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
    }
}
