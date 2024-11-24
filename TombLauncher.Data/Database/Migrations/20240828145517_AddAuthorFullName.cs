#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace TombLauncher.Data.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthorFullName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorFullName",
                table: "Games",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorFullName",
                table: "Games");
        }
    }
}
