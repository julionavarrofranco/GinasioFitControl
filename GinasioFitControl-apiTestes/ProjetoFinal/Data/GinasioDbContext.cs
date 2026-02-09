using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Models;

namespace ProjetoFinal.Data
{
    public class GinasioDbContext : DbContext
    {
        public GinasioDbContext(DbContextOptions<GinasioDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Membro> Membros { get; set; }
        public DbSet<Funcionario> Funcionarios { get; set; }
        public DbSet<Subscricao> Subscricoes { get; set; }
        public DbSet<Exercicio> Exercicios { get; set; }
        public DbSet<PlanoTreino> Planos { get; set; }
        public DbSet<PlanoExercicio> PlanosExercicios { get; set; }
        public DbSet<Aula> Aulas { get; set; }
        public DbSet<AulaMarcada> AulasMarcadas { get; set; }
        public DbSet<MembroAula> MembrosAulas { get; set; }
        public DbSet<AvaliacaoFisica> AvaliacoesFisicas { get; set; }
        public DbSet<MembroAvaliacao> MembrosAvaliacoes { get; set; }
        public DbSet<Pagamento> Pagamentos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Users
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(u => u.IdUser);
                entity.Property(u => u.IdUser).HasColumnName("id_user");
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(u => u.Email).IsUnique()
                                             .HasFilter("[Ativo] = 1");
                entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(f => f.Tipo)
                     .HasConversion<string>()
                     .HasMaxLength(20)
                     .IsRequired();
                entity.Property(u => u.PrimeiraVez).HasDefaultValue(true);
                entity.Property(u => u.Ativo).HasDefaultValue(true);
                entity.Property(u => u.DataDesativacao);
            });

            //RefreshTokens
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("refresh_tokens");
                entity.HasKey(rt => rt.IdRefresh);
                entity.Property(rt => rt.IdRefresh).HasColumnName("id_refresh");
                entity.Property(rt => rt.IdUser).IsRequired();
                entity.Property(rt => rt.Token).IsRequired().HasMaxLength(255);
                entity.Property(rt => rt.SubstituidoPor).HasMaxLength(255);
                entity.Property(rt => rt.Validade).IsRequired();
                entity.Property(rt => rt.Cancelado).HasDefaultValue(false);

                entity.HasOne(rt => rt.User)
                      .WithMany(u => u.RefreshTokens)
                      .HasForeignKey(rt => rt.IdUser)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(rt => new { rt.IdUser, rt.Token });
            });

            //Membros
            modelBuilder.Entity<Membro>(entity =>
            {
                entity.ToTable("membros");
                entity.HasKey(m => m.IdMembro);
                entity.Property(m => m.IdMembro).HasColumnName("id_membro");
                entity.Property(m => m.IdUser).IsRequired();
                entity.Property(m => m.Nome).IsRequired().HasMaxLength(100);
                entity.Property(m => m.Telemovel).HasMaxLength(20);
                entity.Property(m => m.DataNascimento).HasColumnType("date");
                entity.Property(m => m.DataRegisto).HasColumnType("date");
                entity.Property(m => m.IdSubscricao).IsRequired();
                entity.Property(m => m.IdPlanoTreino);

                entity.HasOne(m => m.User)
                      .WithOne(u => u.Membro)
                      .HasForeignKey<Membro>(m => m.IdUser)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Subscricao)
                      .WithMany(s => s.Membros)
                      .HasForeignKey(m => m.IdSubscricao)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.PlanoTreino)
                      .WithMany()
                      .HasForeignKey(m => m.IdPlanoTreino)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            //Funcionarios
            modelBuilder.Entity<Funcionario>(entity =>
            {
                entity.ToTable("funcionarios");
                entity.HasKey(f => f.IdFuncionario);
                entity.Property(f => f.IdFuncionario).HasColumnName("id_funcionario");
                entity.Property(f => f.IdUser).IsRequired();
                entity.Property(f => f.Nome).IsRequired().HasMaxLength(100);
                entity.Property(f => f.Telemovel).HasMaxLength(20);
                entity.Property(f => f.Funcao)
                      .HasConversion<string>()
                      .HasMaxLength(10)
                      .IsRequired();

                entity.HasOne(f => f.User)
                      .WithOne(u => u.Funcionario)
                      .HasForeignKey<Funcionario>(f => f.IdUser)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            //Subscricoes
            modelBuilder.Entity<Subscricao>(entity =>
            {
                entity.ToTable("subscricoes");
                entity.HasKey(s => s.IdSubscricao);
                entity.Property(s => s.IdSubscricao).HasColumnName("id_subscricao");
                entity.Property(s => s.Nome).IsRequired().HasMaxLength(100);
                entity.HasIndex(s => s.Nome).IsUnique();

                entity.Property(s => s.Tipo)
                      .HasConversion<string>()
                      .HasMaxLength(10)
                      .IsRequired();

                entity.Property(s => s.Preco).HasColumnType("decimal(10,2)").IsRequired();
                entity.Property(s => s.Descricao).HasColumnType("text");
                entity.Property(s => s.Ativo).HasDefaultValue(true);
            });

            //Exercicios
            modelBuilder.Entity<Exercicio>(entity =>
            {
                entity.ToTable("exercicios");
                entity.HasKey(e => e.IdExercicio);
                entity.Property(e => e.IdExercicio).HasColumnName("id_exercicio");
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
                entity.Property(e => e.GrupoMuscular)
                     .HasConversion<string>()
                     .HasMaxLength(100)
                     .IsRequired();
                entity.Property(e => e.Descricao).HasColumnType("text").HasDefaultValue("");
                entity.Property(e => e.FotoUrl).HasMaxLength(255).HasDefaultValue("");
                entity.Property(e => e.Ativo).HasDefaultValue(true);
            });

            //PlanosTreino
            modelBuilder.Entity<PlanoTreino>(entity =>
            {
                entity.ToTable("planos_treino");
                entity.HasKey(p => p.IdPlano);
                entity.Property(p => p.IdPlano).HasColumnName("id_plano");
                entity.Property(p => p.IdFuncionario).IsRequired();
                entity.Property(p => p.Nome).IsRequired().HasMaxLength(100);
                entity.HasIndex(p => p.Nome).IsUnique();
                entity.Property(p => p.DataCriacao).HasColumnType("date");
                entity.Property(p => p.Observacoes).HasColumnType("text");
                entity.Property(p => p.DataDesativacao);

                entity.HasOne(p => p.Funcionario)
                      .WithMany(f => f.PlanosTreino)
                      .HasForeignKey(p => p.IdFuncionario)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            //PlanosExercicios
            modelBuilder.Entity<PlanoExercicio>(entity =>
            {
                entity.ToTable("planos_exercicios");
                entity.HasKey(pe => new { pe.IdPlano, pe.IdExercicio });

                entity.Property(pe => pe.Series);
                entity.Property(pe => pe.Repeticoes);
                entity.Property(pe => pe.Carga).HasColumnType("decimal(10,2)");
                entity.Property(pe => pe.Ordem);

                entity.HasOne(pe => pe.PlanoTreino)
                      .WithMany(p => p.PlanosExercicios)
                      .HasForeignKey(pe => pe.IdPlano)
                      .HasConstraintName("FK_PlanosExercicios_PlanoTreino");

                entity.HasOne(pe => pe.Exercicio)
                      .WithMany(e => e.PlanosExercicios)
                      .HasForeignKey(pe => pe.IdExercicio)
                      .HasConstraintName("FK_PlanosExercicios_Exercicio");
            });

            //Aulas
            modelBuilder.Entity<Aula>(entity =>
            {
                entity.ToTable("aulas");
                entity.HasKey(a => a.IdAula);

                // Permitir que IdFuncionario seja nulo
                entity.Property(a => a.IdFuncionario);

                entity.Property(a => a.Nome).IsRequired().HasMaxLength(255);

                entity.Property(a => a.DiaSemana)
                      .HasConversion<string>()
                      .HasMaxLength(10)
                      .IsRequired();

                entity.Property(a => a.HoraInicio).HasColumnType("time");
                entity.Property(a => a.HoraFim).HasColumnType("time");
                entity.Property(a => a.Capacidade);
                entity.Property(a => a.DataDesativacao);

                entity.HasOne(a => a.Funcionario)
                      .WithMany(f => f.Aulas)
                      .HasForeignKey(a => a.IdFuncionario)
                      .OnDelete(DeleteBehavior.Restrict);
            });


            // AulaMarcada
            modelBuilder.Entity<AulaMarcada>(entity =>
            {
                entity.ToTable("aulas_marcadas");
                entity.HasKey(am => am.Id);
                entity.Property(am => am.DataAula).HasColumnType("date").IsRequired();
                entity.Property(am => am.Sala).IsRequired();
                entity.Property(am => am.DataDesativacao);

                entity.HasOne(am => am.Aula)
                      .WithMany(a => a.AulasMarcadas)
                      .HasForeignKey(am => am.IdAula)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(am => am.MembrosAulas)
                      .WithOne(ma => ma.AulaMarcada)
                      .HasForeignKey(ma => ma.IdAulaMarcada)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            //MembrosAulas
            modelBuilder.Entity<MembroAula>(entity =>
            {
                entity.ToTable("membros_aulas");
                entity.HasKey(ma => new { ma.IdMembro, ma.IdAulaMarcada });
                entity.Property(ma => ma.DataReserva).HasColumnType("date");
                entity.Property(ma => ma.Presenca)
                      .HasConversion<string>()
                      .HasMaxLength(20)
                      .IsRequired();

                entity.HasOne(ma => ma.Membro)
                      .WithMany(m => m.MembroAulas)
                      .HasForeignKey(ma => ma.IdMembro)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ma => ma.AulaMarcada)
                      .WithMany(a => a.MembrosAulas)
                      .HasForeignKey(ma => ma.IdAulaMarcada)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(ma => new { ma.IdMembro, ma.IdAulaMarcada })
                      .IsUnique();
            });

            //AvaliacoesFisicas
            modelBuilder.Entity<AvaliacaoFisica>(entity =>
            {
                entity.ToTable("avaliacoes_fisicas");
                entity.HasKey(av => av.IdAvaliacao);
                entity.Property(av => av.IdMembro).IsRequired();
                entity.Property(av => av.IdFuncionario).IsRequired();
                entity.Property(av => av.DataAvaliacao).HasColumnType("date");
                entity.Property(av => av.Peso).HasColumnType("decimal(5,2)");
                entity.Property(av => av.Altura).HasColumnType("decimal(4,2)");
                entity.Property(av => av.Imc).HasColumnType("decimal(4,2)");
                entity.Property(av => av.MassaMuscular).HasColumnType("decimal(5,2)");
                entity.Property(av => av.MassaGorda).HasColumnType("decimal(5,2)");
                entity.Property(av => av.Observacoes).HasColumnType("text").HasDefaultValue("");
                entity.Property(av => av.DataDesativacao);

                entity.HasOne(av => av.Membro)
                      .WithMany(m => m.AvaliacoesFisicas)
                      .HasForeignKey(av => av.IdMembro)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(av => av.Funcionario)
                      .WithMany(f => f.AvaliacoesFisicas)
                      .HasForeignKey(av => av.IdFuncionario)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<MembroAvaliacao>(entity =>
            {
                entity.ToTable("membros_avaliacoes");

                entity.HasKey(ma => ma.IdMembroAvaliacao);

                entity.Property(ma => ma.DataReserva)
                      .HasColumnType("datetime")
                      .IsRequired();

                entity.Property(ma => ma.Estado)
                      .HasConversion<string>()
                      .HasMaxLength(20)
                      .IsRequired();

                entity.Property(ma => ma.DataCancelamento)
                      .HasColumnType("datetime");

                entity.Property(ma => ma.DataDesativacao)
                      .HasColumnType("datetime");

                entity.HasOne(ma => ma.Membro)
                      .WithMany(m => m.MembroAvaliacoes)
                      .HasForeignKey(ma => ma.IdMembro)
                      .OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<MembroAvaliacao>()
                      .HasOne(ma => ma.AvaliacaoFisica)
                      .WithOne()
                      .HasForeignKey<MembroAvaliacao>(ma => ma.IdAvaliacaoFisica)
                      .OnDelete(DeleteBehavior.Restrict);

            });


            //Pagamentos
            modelBuilder.Entity<Pagamento>(entity =>
            {
                entity.ToTable("pagamentos");
                entity.HasKey(p => p.IdPagamento);
                entity.Property(p => p.IdMembro).IsRequired();
                entity.Property(p => p.IdSubscricao).IsRequired();
                entity.Property(p => p.DataPagamento).HasColumnType("date");
                entity.Property(p => p.ValorPago).HasColumnType("decimal(10,2)");
                entity.Property(p => p.MetodoPagamento)
                      .HasConversion<string>()
                      .HasMaxLength(20)
                      .IsRequired();

                entity.Property(p => p.EstadoPagamento)
                      .HasConversion<string>()
                      .HasMaxLength(20)
                      .IsRequired();

                entity.Property(p => p.MesReferente).HasColumnType("date");
                entity.Property(p => p.DataRegisto).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(p => p.DataDesativacao);

                entity.HasOne(p => p.Membro)
                      .WithMany(m => m.Pagamentos)
                      .HasForeignKey(p => p.IdMembro)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Subscricao)
                      .WithMany(s => s.Pagamentos)
                      .HasForeignKey(p => p.IdSubscricao)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}


