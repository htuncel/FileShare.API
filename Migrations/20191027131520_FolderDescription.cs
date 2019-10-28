using Microsoft.EntityFrameworkCore.Migrations;

namespace FileShare.API.Migrations
{
    public partial class FolderDescription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FolderDescription",
                table: "Folders",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FolderDescription",
                table: "Folders");
        }
    }
}
