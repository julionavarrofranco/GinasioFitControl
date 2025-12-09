using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoFinal.Migrations
{
    /// <inheritdoc />
    public partial class EstruturaAtualizada_Users_Membros_Funcionarios_Subcricoes_PlanosTreino : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_Email",
                table: "users");

            migrationBuilder.DropColumn(
                name: "DataDesativacao",
                table: "membros");

            migrationBuilder.DropColumn(
                name: "DataDesativacao",
                table: "funcionarios");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataDesativacao",
                table: "users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "subscricoes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Nome",
                table: "subscricoes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Observacoes",
                table: "planos_treino",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Nome",
                table: "planos_treino",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true,
                filter: "[Ativo] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_subscricoes_Nome",
                table: "subscricoes",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_planos_treino_Nome",
                table: "planos_treino",
                column: "Nome",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_Email",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_subscricoes_Nome",
                table: "subscricoes");

            migrationBuilder.DropIndex(
                name: "IX_planos_treino_Nome",
                table: "planos_treino");

            migrationBuilder.DropColumn(
                name: "DataDesativacao",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Nome",
                table: "subscricoes");

            migrationBuilder.DropColumn(
                name: "Nome",
                table: "planos_treino");

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "subscricoes",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Observacoes",
                table: "planos_treino",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataDesativacao",
                table: "membros",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataDesativacao",
                table: "funcionarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);
        }
    }
}
