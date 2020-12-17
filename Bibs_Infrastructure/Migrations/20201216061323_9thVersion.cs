using Microsoft.EntityFrameworkCore.Migrations;

namespace Bibs_Infrastructure.Migrations
{
    public partial class _9thVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NoWeeb",
                table: "Servers",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoWeeb",
                table: "Servers");
        }
    }
}
