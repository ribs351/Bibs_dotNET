using Microsoft.EntityFrameworkCore.Migrations;

namespace Bibs_Infrastructure.Migrations
{
    public partial class SeventhVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Mutes",
                table: "Mutes");

            migrationBuilder.DropColumn(
                name: "IsMuted",
                table: "Mutes");

            migrationBuilder.RenameTable(
                name: "Mutes",
                newName: "Muteds");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Muteds",
                table: "Muteds",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Muteds",
                table: "Muteds");

            migrationBuilder.RenameTable(
                name: "Muteds",
                newName: "Mutes");

            migrationBuilder.AddColumn<bool>(
                name: "IsMuted",
                table: "Mutes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Mutes",
                table: "Mutes",
                column: "Id");
        }
    }
}
