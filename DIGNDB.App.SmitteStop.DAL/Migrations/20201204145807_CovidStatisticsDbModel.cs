using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace DIGNDB.App.SmitteStop.DAL.Migrations
{
    public partial class CovidStatisticsDbModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationStatistics",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PositiveResultsLast7Days = table.Column<int>(nullable: false),
                    TotalSmittestopDownloads = table.Column<int>(nullable: false),
                    PositiveTestsResultsTotal = table.Column<int>(nullable: false),
                    EntryDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationStatistics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SSIStatistics",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfirmedCasesToday = table.Column<int>(nullable: false),
                    ConfirmedCasesTotal = table.Column<int>(nullable: false),
                    DeathsToday = table.Column<int>(nullable: false),
                    DeathsTotal = table.Column<int>(nullable: false),
                    TestsConductedToday = table.Column<int>(nullable: false),
                    TestsConductedTotal = table.Column<int>(nullable: false),
                    PatientsAdmittedToday = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SSIStatistics", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationStatistics");

            migrationBuilder.DropTable(
                name: "SSIStatistics");
        }
    }
}
