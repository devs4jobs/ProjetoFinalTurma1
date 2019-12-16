using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Model.Migrations
{
    public partial class Anexo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Anexos",
                table: "Anexos");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Anexos");

            migrationBuilder.DropColumn(
                name: "Extensão",
                table: "Anexos");

            migrationBuilder.AlterColumn<string>(
                name: "NomeArquivo",
                table: "Anexos",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Anexos",
                table: "Anexos",
                column: "NomeArquivo");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Anexos",
                table: "Anexos");

            migrationBuilder.AlterColumn<string>(
                name: "NomeArquivo",
                table: "Anexos",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Anexos",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Extensão",
                table: "Anexos",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Anexos",
                table: "Anexos",
                column: "Id");
        }
    }
}
