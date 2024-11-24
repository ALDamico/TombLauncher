#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace TombLauncher.Data.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddTitlePic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "TitlePic",
                table: "Games",
                type: "BLOB",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TitlePic",
                table: "Games");
        }
    }
}
