using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TombLauncher.Data.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddUniversalLauncherPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UniversalLauncherPath",
                table: "Games",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UniversalLauncherPath",
                table: "Games");
        }
    }
}
