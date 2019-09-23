using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Model.Migrations
{
    public partial class Segunda : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Usuarios_AtendentId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Usuarios_ClientId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_AtendentId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_ClientId",
                table: "Tickets");

            migrationBuilder.AddColumn<Guid>(
                name: "AtendenteId",
                table: "Tickets",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClienteId",
                table: "Tickets",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AtendenteId",
                table: "Tickets",
                column: "AtendenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ClienteId",
                table: "Tickets",
                column: "ClienteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Usuarios_AtendenteId",
                table: "Tickets",
                column: "AtendenteId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Usuarios_ClienteId",
                table: "Tickets",
                column: "ClienteId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Usuarios_AtendenteId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Usuarios_ClienteId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_AtendenteId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_ClienteId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "AtendenteId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ClienteId",
                table: "Tickets");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AtendentId",
                table: "Tickets",
                column: "AtendentId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ClientId",
                table: "Tickets",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Usuarios_AtendentId",
                table: "Tickets",
                column: "AtendentId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Usuarios_ClientId",
                table: "Tickets",
                column: "ClientId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
