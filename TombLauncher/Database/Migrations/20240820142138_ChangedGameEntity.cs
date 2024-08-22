#nullable disable

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TombLauncher.Database.Migrations
{
    /// <inheritdoc />
    public partial class ChangedGameEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimePlayed",
                table: "Games");

            migrationBuilder.AddColumn<string>(
                name: "ExecutablePath",
                table: "Games",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstallDirectory",
                table: "Games",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExecutablePath",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "InstallDirectory",
                table: "Games");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TimePlayed",
                table: "Games",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}
