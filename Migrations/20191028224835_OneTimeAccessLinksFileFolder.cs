using Microsoft.EntityFrameworkCore.Migrations;

namespace FileShare.API.Migrations
{
    public partial class OneTimeAccessLinksFileFolder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "OneTimeAccessLinks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FolderName",
                table: "OneTimeAccessLinks",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "OneTimeAccessLinks");

            migrationBuilder.DropColumn(
                name: "FolderName",
                table: "OneTimeAccessLinks");
        }
    }
}
