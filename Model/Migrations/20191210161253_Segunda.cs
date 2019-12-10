using Microsoft.EntityFrameworkCore.Migrations;

namespace Model.Migrations
{
    public partial class Segunda : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "VisualizarTicket",
                table: "Tickets",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "VisualizarMensagem",
                table: "Respostas",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VisualizarTicket",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "VisualizarMensagem",
                table: "Respostas");
        }
    }
}
