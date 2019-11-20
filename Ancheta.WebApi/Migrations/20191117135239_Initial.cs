using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ancheta.WebApi.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Polls",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Question = table.Column<string>(maxLength: 400, nullable: true),
                    IsPublic = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Duration = table.Column<TimeSpan>(nullable: false),
                    SecretCodeHash = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Polls", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Answers",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Content = table.Column<string>(maxLength: 1000, nullable: true),
                    OwnerPollId = table.Column<Guid>(nullable: true),
                    Votes = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Answers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Answers_Polls_OwnerPollId",
                        column: x => x.OwnerPollId,
                        principalTable: "Polls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Answers_OwnerPollId",
                table: "Answers",
                column: "OwnerPollId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Answers");

            migrationBuilder.DropTable(
                name: "Polls");
        }
    }
}
