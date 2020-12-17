using Microsoft.EntityFrameworkCore.Migrations;

namespace Bibs_Infrastructure.Migrations
{
    public partial class _11thVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Limit",
                table: "Servers");

            migrationBuilder.AddColumn<bool>(
                name: "HasLimit",
                table: "Servers",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasLimit",
                table: "Servers");

            migrationBuilder.AddColumn<bool>(
                name: "Limit",
                table: "Servers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
