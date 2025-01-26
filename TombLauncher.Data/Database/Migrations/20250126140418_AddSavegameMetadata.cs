using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TombLauncher.Data.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSavegameMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SavegameMetadata",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileBackupId = table.Column<int>(type: "INTEGER", nullable: false),
                    SlotNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    SaveNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    LevelName = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavegameMetadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavegameMetadata_FileBackups_FileBackupId",
                        column: x => x.FileBackupId,
                        principalTable: "FileBackups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SavegameMetadata_FileBackupId",
                table: "SavegameMetadata",
                column: "FileBackupId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SavegameMetadata");
        }
    }
}
