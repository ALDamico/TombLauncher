using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TombLauncher.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInstalledFromGameLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InstalledFromLinkId",
                table: "Games",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Games_InstalledFromLinkId",
                table: "Games",
                column: "InstalledFromLinkId");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_GameLink_InstalledFromLinkId",
                table: "Games",
                column: "InstalledFromLinkId",
                principalTable: "GameLink",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_GameLink_InstalledFromLinkId",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_Games_InstalledFromLinkId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "InstalledFromLinkId",
                table: "Games");
        }
    }
}
