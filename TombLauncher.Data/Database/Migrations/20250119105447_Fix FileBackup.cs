using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TombLauncher.Data.Database.Migrations
{
    /// <inheritdoc />
    public partial class FixFileBackup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileBackups_Games_GameId",
                table: "FileBackups");

            migrationBuilder.DropIndex(
                name: "IX_FileBackups_GameId",
                table: "FileBackups");

            migrationBuilder.AlterColumn<int>(
                name: "GameId",
                table: "FileBackups",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "GameId",
                table: "FileBackups",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.CreateIndex(
                name: "IX_FileBackups_GameId",
                table: "FileBackups",
                column: "GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_FileBackups_Games_GameId",
                table: "FileBackups",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id");
        }
    }
}
