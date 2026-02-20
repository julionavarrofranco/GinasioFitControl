using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoFinal.Migrations
{
    /// <inheritdoc />
    public partial class Updatetableplanos_treino : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_planos_treino_membros_MembroIdMembro",
                table: "planos_treino");

            migrationBuilder.DropIndex(
                name: "IX_planos_treino_MembroIdMembro",
                table: "planos_treino");

            migrationBuilder.DropColumn(
                name: "MembroIdMembro",
                table: "planos_treino");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MembroIdMembro",
                table: "planos_treino",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_planos_treino_MembroIdMembro",
                table: "planos_treino",
                column: "MembroIdMembro");

            migrationBuilder.AddForeignKey(
                name: "FK_planos_treino_membros_MembroIdMembro",
                table: "planos_treino",
                column: "MembroIdMembro",
                principalTable: "membros",
                principalColumn: "id_membro");
        }
    }
}
