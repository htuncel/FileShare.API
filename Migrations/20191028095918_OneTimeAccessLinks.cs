using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FileShare.API.Migrations
{
    public partial class OneTimeAccessLinks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OneTimeAccessLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsUsed = table.Column<bool>(nullable: false),
                    UsedAt = table.Column<DateTime>(nullable: false),
                    FileId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OneTimeAccessLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OneTimeAccessLinks_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OneTimeAccessLinks_FileId",
                table: "OneTimeAccessLinks",
                column: "FileId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OneTimeAccessLinks");
        }
    }
}
