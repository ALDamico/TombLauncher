using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TombLauncher.Data.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddFileBackupList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_FileBackups_GameId",
                table: "FileBackups",
                column: "GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_FileBackups_Games_GameId",
                table: "FileBackups",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileBackups_Games_GameId",
                table: "FileBackups");

            migrationBuilder.DropIndex(
                name: "IX_FileBackups_GameId",
                table: "FileBackups");
        }
    }
}
