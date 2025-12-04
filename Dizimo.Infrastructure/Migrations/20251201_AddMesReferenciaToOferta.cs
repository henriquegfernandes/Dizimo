using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dizimo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMesReferenciaToOferta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MesReferencia",
                table: "Ofertas",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MesReferencia",
                table: "Ofertas");
        }
    }
}
