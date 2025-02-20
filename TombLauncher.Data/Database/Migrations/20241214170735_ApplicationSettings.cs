using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TombLauncher.Data.Database.Migrations
{
    /// <inheritdoc />
    public partial class ApplicationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SettingName = table.Column<string>(type: "TEXT", nullable: true),
                    StringValue = table.Column<string>(type: "TEXT", nullable: true),
                    IntValue = table.Column<int>(type: "INTEGER", nullable: true),
                    DoubleValue = table.Column<double>(type: "REAL", nullable: true),
                    BoolValue = table.Column<bool>(type: "INTEGER", nullable: true),
                    DateTimeValue = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Settings");
        }
    }
}
