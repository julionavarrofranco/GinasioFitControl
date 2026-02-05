using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoFinal.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTabelaAulasMarcadas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_membros_aulas_aulas_IdAula",
                table: "membros_aulas");

            migrationBuilder.RenameColumn(
                name: "IdAula",
                table: "membros_aulas",
                newName: "IdAulaMarcada");

            migrationBuilder.RenameIndex(
                name: "IX_membros_aulas_IdAula",
                table: "membros_aulas",
                newName: "IX_membros_aulas_IdAulaMarcada");

            migrationBuilder.AddColumn<int>(
                name: "AulaIdAula",
                table: "membros_aulas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "aulas_marcadas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdAula = table.Column<int>(type: "int", nullable: false),
                    DataAula = table.Column<DateTime>(type: "date", nullable: false),
                    DataDesativacao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aulas_marcadas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_aulas_marcadas_aulas_IdAula",
                        column: x => x.IdAula,
                        principalTable: "aulas",
                        principalColumn: "IdAula",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_membros_aulas_AulaIdAula",
                table: "membros_aulas",
                column: "AulaIdAula");

            migrationBuilder.CreateIndex(
                name: "IX_aulas_marcadas_IdAula",
                table: "aulas_marcadas",
                column: "IdAula");

            migrationBuilder.AddForeignKey(
                name: "FK_membros_aulas_aulas_AulaIdAula",
                table: "membros_aulas",
                column: "AulaIdAula",
                principalTable: "aulas",
                principalColumn: "IdAula");

            migrationBuilder.AddForeignKey(
                name: "FK_membros_aulas_aulas_marcadas_IdAulaMarcada",
                table: "membros_aulas",
                column: "IdAulaMarcada",
                principalTable: "aulas_marcadas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_membros_aulas_aulas_AulaIdAula",
                table: "membros_aulas");

            migrationBuilder.DropForeignKey(
                name: "FK_membros_aulas_aulas_marcadas_IdAulaMarcada",
                table: "membros_aulas");

            migrationBuilder.DropTable(
                name: "aulas_marcadas");

            migrationBuilder.DropIndex(
                name: "IX_membros_aulas_AulaIdAula",
                table: "membros_aulas");

            migrationBuilder.DropColumn(
                name: "AulaIdAula",
                table: "membros_aulas");

            migrationBuilder.RenameColumn(
                name: "IdAulaMarcada",
                table: "membros_aulas",
                newName: "IdAula");

            migrationBuilder.RenameIndex(
                name: "IX_membros_aulas_IdAulaMarcada",
                table: "membros_aulas",
                newName: "IX_membros_aulas_IdAula");

            migrationBuilder.AddForeignKey(
                name: "FK_membros_aulas_aulas_IdAula",
                table: "membros_aulas",
                column: "IdAula",
                principalTable: "aulas",
                principalColumn: "IdAula",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
