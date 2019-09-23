﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Model;

namespace Model.Migrations
{
    [DbContext(typeof(ServiceContext))]
    [Migration("20190923135455_Segunda")]
    partial class Segunda
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Model.Resposta", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("DataCadastro");

                    b.Property<string>("Mensagem");

                    b.Property<Guid?>("TicketId");

                    b.Property<Guid?>("UsuarioId");

                    b.HasKey("Id");

                    b.HasIndex("TicketId");

                    b.ToTable("Respostas");
                });

            modelBuilder.Entity("Model.Ticket", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid?>("AtendentId");

                    b.Property<Guid?>("AtendenteId");

                    b.Property<int?>("Avaliacao");

                    b.Property<Guid?>("ClientId");

                    b.Property<Guid?>("ClienteId");

                    b.Property<DateTime>("DataCadastro");

                    b.Property<string>("Mensagem");

                    b.Property<long>("NumeroTicket");

                    b.Property<int?>("Status");

                    b.Property<string>("Titulo");

                    b.HasKey("Id");

                    b.HasIndex("AtendenteId");

                    b.HasIndex("ClienteId");

                    b.ToTable("Tickets");
                });

            modelBuilder.Entity("Model.Usuario", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("DataCadastro");

                    b.Property<string>("Email");

                    b.Property<string>("Nome");

                    b.Property<string>("Senha");

                    b.Property<string>("Tipo");

                    b.HasKey("Id");

                    b.ToTable("Usuarios");
                });

            modelBuilder.Entity("Model.Resposta", b =>
                {
                    b.HasOne("Model.Ticket")
                        .WithMany("LstRespostas")
                        .HasForeignKey("TicketId");
                });

            modelBuilder.Entity("Model.Ticket", b =>
                {
                    b.HasOne("Model.Usuario", "Atendente")
                        .WithMany()
                        .HasForeignKey("AtendenteId");

                    b.HasOne("Model.Usuario", "Cliente")
                        .WithMany()
                        .HasForeignKey("ClienteId");
                });
#pragma warning restore 612, 618
        }
    }
}
