using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraData.Migrations
{
    /// <inheritdoc />
    public partial class RemovendoUsuarioTipo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuario_Tipo",
                schema: "Imobiliaria",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "UsuarioTipo",
                schema: "Imobiliaria");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_usuario_tipo_id",
                schema: "Imobiliaria",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "usuario_tipo_id",
                schema: "Imobiliaria",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "usuario_tipo_id",
                schema: "Imobiliaria",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "UsuarioTipo",
                schema: "Imobiliaria",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    data_cadastro = table.Column<DateTime>(type: "datetime2", nullable: true),
                    datacriacao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Tipo = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_UsuarioTipo", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_usuario_tipo_id",
                schema: "Imobiliaria",
                table: "AspNetUsers",
                column: "usuario_tipo_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuario_Tipo",
                schema: "Imobiliaria",
                table: "AspNetUsers",
                column: "usuario_tipo_id",
                principalSchema: "Imobiliaria",
                principalTable: "UsuarioTipo",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
