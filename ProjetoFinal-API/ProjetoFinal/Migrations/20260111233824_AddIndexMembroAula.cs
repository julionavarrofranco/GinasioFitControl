using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoFinal.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexMembroAula : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_membros_aulas_IdMembro_IdAulaMarcada",
                table: "membros_aulas",
                columns: new[] { "IdMembro", "IdAulaMarcada" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_membros_aulas_IdMembro_IdAulaMarcada",
                table: "membros_aulas");
        }
    }
}
