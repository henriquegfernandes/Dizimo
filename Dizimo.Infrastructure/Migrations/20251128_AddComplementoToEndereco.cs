using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dizimo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddComplementoToEndereco : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Endereco_Complemento",
                table: "Dizimistas",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Endereco_Complemento",
                table: "Dizimistas");
        }
    }
}
