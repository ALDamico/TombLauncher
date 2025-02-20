using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TombLauncher.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFieldsFromGame : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExecutablePath",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "UniversalLauncherPath",
                table: "Games");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExecutablePath",
                table: "Games",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UniversalLauncherPath",
                table: "Games",
                type: "TEXT",
                nullable: true);
        }
    }
}
