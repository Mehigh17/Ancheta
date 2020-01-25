using Microsoft.EntityFrameworkCore.Migrations;

namespace Ancheta.WebApi.Migrations
{
    public partial class AddFlagIndicatingMultipleVotesPerIp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowMultipleVotesPerIp",
                table: "Polls",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowMultipleVotesPerIp",
                table: "Polls");
        }
    }
}
