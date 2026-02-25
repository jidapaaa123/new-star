using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FinalWorkerCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    FinalMilitaryCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    FinalMinerals = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    FinalGas = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    DidExpand = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    UpgradesCompleted = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    Result = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "Ongoing")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Matches");
        }
    }
}
