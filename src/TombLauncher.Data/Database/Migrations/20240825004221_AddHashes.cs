#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace TombLauncher.Data.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddHashes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlaySession_Games_GameId",
                table: "PlaySession");

            migrationBuilder.AlterColumn<int>(
                name: "GameId",
                table: "PlaySession",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Guid",
                table: "Games",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "GameHashes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameId = table.Column<int>(type: "INTEGER", nullable: false),
                    Md5Hash = table.Column<string>(type: "TEXT", nullable: true),
                    FileName = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameHashes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameHashes_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameHashes_GameId",
                table: "GameHashes",
                column: "GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlaySession_Games_GameId",
                table: "PlaySession",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlaySession_Games_GameId",
                table: "PlaySession");

            migrationBuilder.DropTable(
                name: "GameHashes");

            migrationBuilder.DropColumn(
                name: "Guid",
                table: "Games");

            migrationBuilder.AlterColumn<int>(
                name: "GameId",
                table: "PlaySession",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_PlaySession_Games_GameId",
                table: "PlaySession",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id");
        }
    }
}
