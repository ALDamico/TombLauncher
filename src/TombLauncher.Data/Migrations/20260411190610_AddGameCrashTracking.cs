using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TombLauncher.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGameCrashTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CrashFileContent",
                table: "PlaySession",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExitCode",
                table: "PlaySession",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StdErr",
                table: "PlaySession",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StdOut",
                table: "PlaySession",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CrashFileContent",
                table: "PlaySession");

            migrationBuilder.DropColumn(
                name: "ExitCode",
                table: "PlaySession");

            migrationBuilder.DropColumn(
                name: "StdErr",
                table: "PlaySession");

            migrationBuilder.DropColumn(
                name: "StdOut",
                table: "PlaySession");
        }
    }
}
