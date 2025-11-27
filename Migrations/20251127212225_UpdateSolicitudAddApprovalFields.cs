using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Campus_Virtul_GRLL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSolicitudAddApprovalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRespuesta",
                table: "Solicitud",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoRechazo",
                table: "Solicitud",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RespuestaDe",
                table: "Solicitud",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioCreado",
                table: "Solicitud",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Solicitud_RespuestaDe",
                table: "Solicitud",
                column: "RespuestaDe");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitud_UsuarioCreado",
                table: "Solicitud",
                column: "UsuarioCreado");

            migrationBuilder.AddForeignKey(
                name: "FK_Solicitud_Usuario_RespuestaDe",
                table: "Solicitud",
                column: "RespuestaDe",
                principalTable: "Usuario",
                principalColumn: "IdUsuario",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Solicitud_Usuario_UsuarioCreado",
                table: "Solicitud",
                column: "UsuarioCreado",
                principalTable: "Usuario",
                principalColumn: "IdUsuario",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Solicitud_Usuario_RespuestaDe",
                table: "Solicitud");

            migrationBuilder.DropForeignKey(
                name: "FK_Solicitud_Usuario_UsuarioCreado",
                table: "Solicitud");

            migrationBuilder.DropIndex(
                name: "IX_Solicitud_RespuestaDe",
                table: "Solicitud");

            migrationBuilder.DropIndex(
                name: "IX_Solicitud_UsuarioCreado",
                table: "Solicitud");

            migrationBuilder.DropColumn(
                name: "FechaRespuesta",
                table: "Solicitud");

            migrationBuilder.DropColumn(
                name: "MotivoRechazo",
                table: "Solicitud");

            migrationBuilder.DropColumn(
                name: "RespuestaDe",
                table: "Solicitud");

            migrationBuilder.DropColumn(
                name: "UsuarioCreado",
                table: "Solicitud");
        }
    }
}
