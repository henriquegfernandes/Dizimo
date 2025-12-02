using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dizimo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCamposDizimista : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataCadastro",
                table: "Dizimistas",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Endereco_Bairro",
                table: "Dizimistas",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Endereco_CEP",
                table: "Dizimistas",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Endereco_Cidade",
                table: "Dizimistas",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Endereco_Numero",
                table: "Dizimistas",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Endereco_Rua",
                table: "Dizimistas",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Endereco_UF",
                table: "Dizimistas",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Telefone",
                table: "Dizimistas",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Whatsapp",
                table: "Dizimistas",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataCadastro",
                table: "Dizimistas");

            migrationBuilder.DropColumn(
                name: "Endereco_Bairro",
                table: "Dizimistas");

            migrationBuilder.DropColumn(
                name: "Endereco_CEP",
                table: "Dizimistas");

            migrationBuilder.DropColumn(
                name: "Endereco_Cidade",
                table: "Dizimistas");

            migrationBuilder.DropColumn(
                name: "Endereco_Numero",
                table: "Dizimistas");

            migrationBuilder.DropColumn(
                name: "Endereco_Rua",
                table: "Dizimistas");

            migrationBuilder.DropColumn(
                name: "Endereco_UF",
                table: "Dizimistas");

            migrationBuilder.DropColumn(
                name: "Telefone",
                table: "Dizimistas");

            migrationBuilder.DropColumn(
                name: "Whatsapp",
                table: "Dizimistas");
        }
    }
}
