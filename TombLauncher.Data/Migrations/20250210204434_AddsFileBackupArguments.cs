using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TombLauncher.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddsFileBackupArguments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Arguments",
                table: "FileBackups",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Arguments",
                table: "FileBackups");
        }
    }
}
