using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Campus_Virtul_GRLL.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EliminadoPor",
                table: "Usuario",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EsEliminado",
                table: "Usuario",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEliminacion",
                table: "Usuario",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_EliminadoPor",
                table: "Usuario",
                column: "EliminadoPor");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuario_Usuario_EliminadoPor",
                table: "Usuario",
                column: "EliminadoPor",
                principalTable: "Usuario",
                principalColumn: "IdUsuario",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuario_Usuario_EliminadoPor",
                table: "Usuario");

            migrationBuilder.DropIndex(
                name: "IX_Usuario_EliminadoPor",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "EliminadoPor",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "EsEliminado",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "FechaEliminacion",
                table: "Usuario");
        }
    }
}
