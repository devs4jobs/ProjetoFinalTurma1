using Microsoft.EntityFrameworkCore.Migrations;

namespace Model.Migrations
{
    public partial class Quarta : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Tickets",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Avaliacao",
                table: "Tickets",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Respostas_UsuarioId",
                table: "Respostas",
                column: "UsuarioId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Respostas_Usuarios_UsuarioId",
                table: "Respostas");

            migrationBuilder.DropIndex(
                name: "IX_Respostas_UsuarioId",
                table: "Respostas");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Tickets",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "Avaliacao",
                table: "Tickets",
                nullable: true,
                oldClrType: typeof(int));
        }
    }
}
