using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TombLauncher.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPerGameCompatibilitySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WinePrefix",
                table: "Games",
                newName: "CompatibilityPrefixPath");

            migrationBuilder.AddColumn<string>(
                name: "CompatibilityToolPath",
                table: "Games",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompatibilityTool",
                table: "Games",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "GameEnvironmentVariables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameId = table.Column<int>(type: "INTEGER", nullable: false),
                    VariableName = table.Column<string>(type: "TEXT", nullable: false),
                    VariableValue = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameEnvironmentVariables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameEnvironmentVariables_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameEnvironmentVariables_GameId",
                table: "GameEnvironmentVariables",
                column: "GameId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameEnvironmentVariables");

            migrationBuilder.DropColumn(
                name: "CompatibilityToolPath",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "CompatibilityTool",
                table: "Games");

            migrationBuilder.RenameColumn(
                name: "CompatibilityPrefixPath",
                table: "Games",
                newName: "WinePrefix");
        }
    }
}
