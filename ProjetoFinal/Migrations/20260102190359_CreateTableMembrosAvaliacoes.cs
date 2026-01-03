using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoFinal.Migrations
{
    /// <inheritdoc />
    public partial class CreateTableMembrosAvaliacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "membros_avaliacoes",
                columns: table => new
                {
                    IdMembroAvaliacao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdMembro = table.Column<int>(type: "int", nullable: false),
                    DataAvaliacao = table.Column<DateTime>(type: "datetime", nullable: false),
                    DataReserva = table.Column<DateTime>(type: "datetime", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DataCancelamento = table.Column<DateTime>(type: "datetime", nullable: true),
                    DataDesativacao = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_membros_avaliacoes", x => x.IdMembroAvaliacao);
                    table.ForeignKey(
                        name: "FK_membros_avaliacoes_membros_IdMembro",
                        column: x => x.IdMembro,
                        principalTable: "membros",
                        principalColumn: "id_membro",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_membros_avaliacoes_IdMembro",
                table: "membros_avaliacoes",
                column: "IdMembro");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "membros_avaliacoes");
        }
    }
}
