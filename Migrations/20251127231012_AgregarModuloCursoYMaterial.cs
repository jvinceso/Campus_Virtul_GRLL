using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Campus_Virtul_GRLL.Migrations
{
    /// <inheritdoc />
    public partial class AgregarModuloCursoYMaterial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModuloCurso",
                columns: table => new
                {
                    IdModuloCurso = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCurso = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EsEliminado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuloCurso", x => x.IdModuloCurso);
                    table.ForeignKey(
                        name: "FK_ModuloCurso_Curso_IdCurso",
                        column: x => x.IdCurso,
                        principalTable: "Curso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Material",
                columns: table => new
                {
                    IdMaterial = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCurso = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Extension = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: true),
                    UrlExterna = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IdModuloCurso = table.Column<int>(type: "int", nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    EsVisible = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubidoPorId = table.Column<int>(type: "int", nullable: false),
                    NumeroDescargas = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    FechaUltimaDescarga = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EsEliminado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Material", x => x.IdMaterial);
                    table.ForeignKey(
                        name: "FK_Material_Curso_IdCurso",
                        column: x => x.IdCurso,
                        principalTable: "Curso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Material_ModuloCurso_IdModuloCurso",
                        column: x => x.IdModuloCurso,
                        principalTable: "ModuloCurso",
                        principalColumn: "IdModuloCurso",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Material_Usuario_EliminadoPorId",
                        column: x => x.EliminadoPorId,
                        principalTable: "Usuario",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Material_Usuario_SubidoPorId",
                        column: x => x.SubidoPorId,
                        principalTable: "Usuario",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Material_EliminadoPorId",
                table: "Material",
                column: "EliminadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Material_IdCurso",
                table: "Material",
                column: "IdCurso");

            migrationBuilder.CreateIndex(
                name: "IX_Material_IdModuloCurso",
                table: "Material",
                column: "IdModuloCurso");

            migrationBuilder.CreateIndex(
                name: "IX_Material_SubidoPorId",
                table: "Material",
                column: "SubidoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuloCurso_IdCurso",
                table: "ModuloCurso",
                column: "IdCurso");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Material");

            migrationBuilder.DropTable(
                name: "ModuloCurso");
        }
    }
}
