using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Model.Migrations
{
    public partial class AnexoComposto : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Anexos_Respostas_RespostaId",
                table: "Anexos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Anexos",
                table: "Anexos");

            migrationBuilder.DropIndex(
                name: "IX_Anexos_RespostaId",
                table: "Anexos");

            migrationBuilder.AlterColumn<Guid>(
                name: "RespostaId",
                table: "Anexos",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Anexos_NomeArquivo",
                table: "Anexos",
                column: "NomeArquivo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Anexos",
                table: "Anexos",
                columns: new[] { "NomeArquivo", "RespostaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Anexos_RespostaId",
                table: "Anexos",
                column: "RespostaId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Anexos_Respostas_RespostaId",
                table: "Anexos",
                column: "RespostaId",
                principalTable: "Respostas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Anexos_Respostas_RespostaId",
                table: "Anexos");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Anexos_NomeArquivo",
                table: "Anexos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Anexos",
                table: "Anexos");

            migrationBuilder.DropIndex(
                name: "IX_Anexos_RespostaId",
                table: "Anexos");

            migrationBuilder.AlterColumn<Guid>(
                name: "RespostaId",
                table: "Anexos",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Anexos",
                table: "Anexos",
                column: "NomeArquivo");

            migrationBuilder.CreateIndex(
                name: "IX_Anexos_RespostaId",
                table: "Anexos",
                column: "RespostaId",
                unique: true,
                filter: "[RespostaId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Anexos_Respostas_RespostaId",
                table: "Anexos",
                column: "RespostaId",
                principalTable: "Respostas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
