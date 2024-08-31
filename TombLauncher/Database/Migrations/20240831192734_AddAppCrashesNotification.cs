using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TombLauncher.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddAppCrashesNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "WasNotified",
                table: "AppCrashes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WasNotified",
                table: "AppCrashes");
        }
    }
}
