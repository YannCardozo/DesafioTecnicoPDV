using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraData.Migrations
{
    /// <inheritdoc />
    public partial class InclusaoImagemImovelBase64 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagemBase64",
                schema: "Imobiliaria",
                table: "Imovel",
                type: "varchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagemBase64",
                schema: "Imobiliaria",
                table: "Imovel");
        }
    }
}
