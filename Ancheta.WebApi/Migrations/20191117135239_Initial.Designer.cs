﻿// <auto-generated />
using System;
using Ancheta.WebApi.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Ancheta.WebApi.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20191117135239_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Ancheta.Model.Data.Answer", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Content")
                        .HasColumnType("character varying(1000)")
                        .HasMaxLength(1000);

                    b.Property<Guid?>("OwnerPollId")
                        .HasColumnType("uuid");

                    b.Property<int>("Votes")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("OwnerPollId");

                    b.ToTable("Answers");
                });

            modelBuilder.Entity("Ancheta.Model.Data.Poll", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("timestamp without time zone");

                    b.Property<TimeSpan>("Duration")
                        .HasColumnType("interval");

                    b.Property<bool>("IsPublic")
                        .HasColumnType("boolean");

                    b.Property<string>("Question")
                        .HasColumnType("character varying(400)")
                        .HasMaxLength(400);

                    b.Property<string>("SecretCodeHash")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Polls");
                });

            modelBuilder.Entity("Ancheta.Model.Data.Answer", b =>
                {
                    b.HasOne("Ancheta.Model.Data.Poll", "OwnerPoll")
                        .WithMany("Answers")
                        .HasForeignKey("OwnerPollId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}