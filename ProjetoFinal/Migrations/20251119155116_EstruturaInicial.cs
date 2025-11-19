using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoFinal.Migrations
{
    /// <inheritdoc />
    public partial class EstruturaInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "exercicios",
                columns: table => new
                {
                    id_exercicio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GrupoMuscular = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    FotoUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercicios", x => x.id_exercicio);
                });

            migrationBuilder.CreateTable(
                name: "subscricoes",
                columns: table => new
                {
                    id_subscricao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Preco = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscricoes", x => x.id_subscricao);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id_user = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PrimeiraVez = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id_user);
                });

            migrationBuilder.CreateTable(
                name: "funcionarios",
                columns: table => new
                {
                    id_funcionario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUser = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telemovel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Funcao = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DataDesativacao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_funcionarios", x => x.id_funcionario);
                    table.ForeignKey(
                        name: "FK_funcionarios_users_IdUser",
                        column: x => x.IdUser,
                        principalTable: "users",
                        principalColumn: "id_user",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id_refresh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUser = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SubstituidoPor = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Validade = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Cancelado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.id_refresh);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_IdUser",
                        column: x => x.IdUser,
                        principalTable: "users",
                        principalColumn: "id_user",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "aulas",
                columns: table => new
                {
                    IdAula = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdFuncionario = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DiaSemana = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraFim = table.Column<TimeSpan>(type: "time", nullable: false),
                    Capacidade = table.Column<int>(type: "int", nullable: false),
                    DataDesativacao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aulas", x => x.IdAula);
                    table.ForeignKey(
                        name: "FK_aulas_funcionarios_IdFuncionario",
                        column: x => x.IdFuncionario,
                        principalTable: "funcionarios",
                        principalColumn: "id_funcionario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "avaliacoes_fisicas",
                columns: table => new
                {
                    IdAvaliacao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdMembro = table.Column<int>(type: "int", nullable: false),
                    IdFuncionario = table.Column<int>(type: "int", nullable: false),
                    DataAvaliacao = table.Column<DateTime>(type: "date", nullable: false),
                    Peso = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Altura = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    Imc = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    MassaMuscular = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    MassaGorda = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Observacoes = table.Column<string>(type: "text", nullable: false),
                    DataDesativacao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_avaliacoes_fisicas", x => x.IdAvaliacao);
                    table.ForeignKey(
                        name: "FK_avaliacoes_fisicas_funcionarios_IdFuncionario",
                        column: x => x.IdFuncionario,
                        principalTable: "funcionarios",
                        principalColumn: "id_funcionario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "membros",
                columns: table => new
                {
                    id_membro = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUser = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telemovel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DataNascimento = table.Column<DateTime>(type: "date", nullable: false),
                    DataRegisto = table.Column<DateTime>(type: "date", nullable: false),
                    IdSubscricao = table.Column<int>(type: "int", nullable: false),
                    IdPlanoTreino = table.Column<int>(type: "int", nullable: true),
                    DataDesativacao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_membros", x => x.id_membro);
                    table.ForeignKey(
                        name: "FK_membros_subscricoes_IdSubscricao",
                        column: x => x.IdSubscricao,
                        principalTable: "subscricoes",
                        principalColumn: "id_subscricao",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_membros_users_IdUser",
                        column: x => x.IdUser,
                        principalTable: "users",
                        principalColumn: "id_user",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "membros_aulas",
                columns: table => new
                {
                    IdMembro = table.Column<int>(type: "int", nullable: false),
                    IdAula = table.Column<int>(type: "int", nullable: false),
                    DataReserva = table.Column<DateTime>(type: "date", nullable: false),
                    Presenca = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_membros_aulas", x => new { x.IdMembro, x.IdAula });
                    table.ForeignKey(
                        name: "FK_membros_aulas_aulas_IdAula",
                        column: x => x.IdAula,
                        principalTable: "aulas",
                        principalColumn: "IdAula",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_membros_aulas_membros_IdMembro",
                        column: x => x.IdMembro,
                        principalTable: "membros",
                        principalColumn: "id_membro",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pagamentos",
                columns: table => new
                {
                    IdPagamento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdMembro = table.Column<int>(type: "int", nullable: false),
                    IdSubscricao = table.Column<int>(type: "int", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "date", nullable: false),
                    ValorPago = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MetodoPagamento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EstadoPagamento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MesReferente = table.Column<DateTime>(type: "date", nullable: false),
                    DataRegisto = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataDesativacao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pagamentos", x => x.IdPagamento);
                    table.ForeignKey(
                        name: "FK_pagamentos_membros_IdMembro",
                        column: x => x.IdMembro,
                        principalTable: "membros",
                        principalColumn: "id_membro",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pagamentos_subscricoes_IdSubscricao",
                        column: x => x.IdSubscricao,
                        principalTable: "subscricoes",
                        principalColumn: "id_subscricao",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "planos_treino",
                columns: table => new
                {
                    id_plano = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdMembro = table.Column<int>(type: "int", nullable: true),
                    IdFuncionario = table.Column<int>(type: "int", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "date", nullable: false),
                    Observacoes = table.Column<string>(type: "text", nullable: false),
                    DataDesativacao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planos_treino", x => x.id_plano);
                    table.ForeignKey(
                        name: "FK_planos_treino_funcionarios_IdFuncionario",
                        column: x => x.IdFuncionario,
                        principalTable: "funcionarios",
                        principalColumn: "id_funcionario",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_planos_treino_membros_IdMembro",
                        column: x => x.IdMembro,
                        principalTable: "membros",
                        principalColumn: "id_membro",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "planos_exercicios",
                columns: table => new
                {
                    IdPlano = table.Column<int>(type: "int", nullable: false),
                    IdExercicio = table.Column<int>(type: "int", nullable: false),
                    Series = table.Column<int>(type: "int", nullable: false),
                    Repeticoes = table.Column<int>(type: "int", nullable: false),
                    Carga = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    PlanoTreinoIdPlano = table.Column<int>(type: "int", nullable: false),
                    ExercicioIdExercicio = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planos_exercicios", x => new { x.IdPlano, x.IdExercicio });
                    table.ForeignKey(
                        name: "FK_planos_exercicios_exercicios_ExercicioIdExercicio",
                        column: x => x.ExercicioIdExercicio,
                        principalTable: "exercicios",
                        principalColumn: "id_exercicio",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_planos_exercicios_planos_treino_PlanoTreinoIdPlano",
                        column: x => x.PlanoTreinoIdPlano,
                        principalTable: "planos_treino",
                        principalColumn: "id_plano",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_aulas_IdFuncionario",
                table: "aulas",
                column: "IdFuncionario");

            migrationBuilder.CreateIndex(
                name: "IX_avaliacoes_fisicas_IdFuncionario",
                table: "avaliacoes_fisicas",
                column: "IdFuncionario");

            migrationBuilder.CreateIndex(
                name: "IX_avaliacoes_fisicas_IdMembro",
                table: "avaliacoes_fisicas",
                column: "IdMembro");

            migrationBuilder.CreateIndex(
                name: "IX_funcionarios_IdUser",
                table: "funcionarios",
                column: "IdUser",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_membros_IdPlanoTreino",
                table: "membros",
                column: "IdPlanoTreino");

            migrationBuilder.CreateIndex(
                name: "IX_membros_IdSubscricao",
                table: "membros",
                column: "IdSubscricao");

            migrationBuilder.CreateIndex(
                name: "IX_membros_IdUser",
                table: "membros",
                column: "IdUser",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_membros_aulas_IdAula",
                table: "membros_aulas",
                column: "IdAula");

            migrationBuilder.CreateIndex(
                name: "IX_pagamentos_IdMembro",
                table: "pagamentos",
                column: "IdMembro");

            migrationBuilder.CreateIndex(
                name: "IX_pagamentos_IdSubscricao",
                table: "pagamentos",
                column: "IdSubscricao");

            migrationBuilder.CreateIndex(
                name: "IX_planos_exercicios_ExercicioIdExercicio",
                table: "planos_exercicios",
                column: "ExercicioIdExercicio");

            migrationBuilder.CreateIndex(
                name: "IX_planos_exercicios_PlanoTreinoIdPlano",
                table: "planos_exercicios",
                column: "PlanoTreinoIdPlano");

            migrationBuilder.CreateIndex(
                name: "IX_planos_treino_IdFuncionario",
                table: "planos_treino",
                column: "IdFuncionario");

            migrationBuilder.CreateIndex(
                name: "IX_planos_treino_IdMembro",
                table: "planos_treino",
                column: "IdMembro");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_IdUser_Token",
                table: "refresh_tokens",
                columns: new[] { "IdUser", "Token" });

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_avaliacoes_fisicas_membros_IdMembro",
                table: "avaliacoes_fisicas",
                column: "IdMembro",
                principalTable: "membros",
                principalColumn: "id_membro",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_membros_planos_treino_IdPlanoTreino",
                table: "membros",
                column: "IdPlanoTreino",
                principalTable: "planos_treino",
                principalColumn: "id_plano",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_planos_treino_funcionarios_IdFuncionario",
                table: "planos_treino");

            migrationBuilder.DropForeignKey(
                name: "FK_planos_treino_membros_IdMembro",
                table: "planos_treino");

            migrationBuilder.DropTable(
                name: "avaliacoes_fisicas");

            migrationBuilder.DropTable(
                name: "membros_aulas");

            migrationBuilder.DropTable(
                name: "pagamentos");

            migrationBuilder.DropTable(
                name: "planos_exercicios");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "aulas");

            migrationBuilder.DropTable(
                name: "exercicios");

            migrationBuilder.DropTable(
                name: "funcionarios");

            migrationBuilder.DropTable(
                name: "membros");

            migrationBuilder.DropTable(
                name: "planos_treino");

            migrationBuilder.DropTable(
                name: "subscricoes");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
