using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoFinal.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTableMembrosAvaliacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdAvaliacaoFisica",
                table: "membros_avaliacoes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_membros_avaliacoes_IdAvaliacaoFisica",
                table: "membros_avaliacoes",
                column: "IdAvaliacaoFisica",
                unique: true,
                filter: "[IdAvaliacaoFisica] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_membros_avaliacoes_avaliacoes_fisicas_IdAvaliacaoFisica",
                table: "membros_avaliacoes",
                column: "IdAvaliacaoFisica",
                principalTable: "avaliacoes_fisicas",
                principalColumn: "IdAvaliacao",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_membros_avaliacoes_avaliacoes_fisicas_IdAvaliacaoFisica",
                table: "membros_avaliacoes");

            migrationBuilder.DropIndex(
                name: "IX_membros_avaliacoes_IdAvaliacaoFisica",
                table: "membros_avaliacoes");

            migrationBuilder.DropColumn(
                name: "IdAvaliacaoFisica",
                table: "membros_avaliacoes");
        }
    }
}
