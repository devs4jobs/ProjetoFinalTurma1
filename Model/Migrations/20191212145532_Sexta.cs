using Microsoft.EntityFrameworkCore.Migrations;

namespace Model.Migrations
{
    public partial class Sexta : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Anexos_Respostas_RespostaId",
                table: "Anexos");

            migrationBuilder.DropIndex(
                name: "IX_Anexos_RespostaId",
                table: "Anexos");
        }
    }
}
