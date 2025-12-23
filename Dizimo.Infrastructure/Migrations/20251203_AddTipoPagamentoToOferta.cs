using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dizimo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTipoPagamentoToOferta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TipoPagamento",
                table: "Ofertas",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoPagamento",
                table: "Ofertas");
        }
    }
}
