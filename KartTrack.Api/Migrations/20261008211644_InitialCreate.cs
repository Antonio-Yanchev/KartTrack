using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KartTrack.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tracks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Location = table.Column<string>(type: "TEXT", nullable: false),
                    IsIndoor = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tracks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TrackId = table.Column<int>(type: "INTEGER", nullable: false),
                    DriverName = table.Column<string>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FastestLap = table.Column<double>(type: "REAL", nullable: false),
                    AverageLap = table.Column<double>(type: "REAL", nullable: false),
                    TotalLaps = table.Column<int>(type: "INTEGER", nullable: false),
                    KartNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Tracks",
                columns: new[] { "Id", "IsIndoor", "Location", "Name" },
                values: new object[,]
                {
                    { 1, true, "London", "TeamSport Edmonton" },
                    { 2, false, "Milton Keynes", "Daytona Milton Keynes" }
                });

            migrationBuilder.InsertData(
                table: "Sessions",
                columns: new[] { "Id", "AverageLap", "Date", "DriverName", "FastestLap", "KartNumber", "Notes", "Position", "TotalLaps", "TrackId" },
                values: new object[,]
                {
                    { 1, 61.200000000000003, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Antonio", 58.450000000000003, 7, "First test session at TeamSport Edmonton", 3, 12, 1 },
                    { 2, 65.400000000000006, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Adam", 62.100000000000001, 14, "Outdoor session at Daytona Milton Keynes", 6, 10, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_TrackId",
                table: "Sessions",
                column: "TrackId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "Tracks");
        }
    }
}
