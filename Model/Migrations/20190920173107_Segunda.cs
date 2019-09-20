using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Model.Migrations
{
    public partial class Segunda : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Respostas_Tickets_TicketId",
                table: "Respostas");

            migrationBuilder.DropForeignKey(
                name: "FK_Respostas_Usuarios_UsuarioId",
                table: "Respostas");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Tickets");

            migrationBuilder.AlterColumn<Guid>(
                name: "UsuarioId",
                table: "Respostas",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AlterColumn<Guid>(
                name: "TicketId",
                table: "Respostas",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddForeignKey(
                name: "FK_Respostas_Tickets_TicketId",
                table: "Respostas",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Respostas_Usuarios_UsuarioId",
                table: "Respostas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Respostas_Tickets_TicketId",
                table: "Respostas");

            migrationBuilder.DropForeignKey(
                name: "FK_Respostas_Usuarios_UsuarioId",
                table: "Respostas");

            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "Tickets",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "UsuarioId",
                table: "Respostas",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TicketId",
                table: "Respostas",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Respostas_Tickets_TicketId",
                table: "Respostas",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Respostas_Usuarios_UsuarioId",
                table: "Respostas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
