using Microsoft.EntityFrameworkCore.Migrations;

namespace DIGNDB.App.SmitteStop.DAL.Migrations
{
    public partial class ChangeVisitedCountriesEnabledValuesInCountrySeeder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 2L,
                column: "VisitedCountriesEnabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 3L,
                column: "VisitedCountriesEnabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 4L,
                column: "VisitedCountriesEnabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 5L,
                column: "VisitedCountriesEnabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 9L,
                column: "VisitedCountriesEnabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 10L,
                column: "VisitedCountriesEnabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 12L,
                column: "VisitedCountriesEnabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 13L,
                column: "VisitedCountriesEnabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 17L,
                column: "VisitedCountriesEnabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 18L,
                column: "VisitedCountriesEnabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 19L,
                column: "VisitedCountriesEnabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 22L,
                column: "VisitedCountriesEnabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 23L,
                column: "VisitedCountriesEnabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 24L,
                column: "VisitedCountriesEnabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 25L,
                column: "VisitedCountriesEnabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 27L,
                column: "VisitedCountriesEnabled",
                value: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 2L,
                column: "VisitedCountriesEnabled",
                value: true);

            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 3L,
                column: "VisitedCountriesEnabled",
                value: true);

            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 4L,
                column: "VisitedCountriesEnabled",
                value: true);

            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 5L,
                column: "VisitedCountriesEnabled",
                value: true);

            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 9L,
                column: "VisitedCountriesEnabled",
                value: true);

            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 10L,
                column: "VisitedCountriesEnabled",
                value: true);

            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 12L,
                column: "VisitedCountriesEnabled",
                value: true);

            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 13L,
                column: "VisitedCountriesEnabled",
                value: true);

            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 17L,
                column: "VisitedCountriesEnabled",
                value: true);

            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 18L,
                column: "VisitedCountriesEnabled",
                value: true);

            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 19L,
                column: "VisitedCountriesEnabled",
                value: true);

            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 22L,
                column: "VisitedCountriesEnabled",
                value: true);

            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 23L,
                column: "VisitedCountriesEnabled",
                value: true);

            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 24L,
                column: "VisitedCountriesEnabled",
                value: true);

            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 25L,
                column: "VisitedCountriesEnabled",
                value: true);

            migrationBuilder.UpdateData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 27L,
                column: "VisitedCountriesEnabled",
                value: true);
        }
    }
}
