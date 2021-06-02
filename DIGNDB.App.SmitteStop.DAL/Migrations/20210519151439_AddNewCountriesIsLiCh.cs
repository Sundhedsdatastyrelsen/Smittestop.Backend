using Microsoft.EntityFrameworkCore.Migrations;

namespace DIGNDB.App.SmitteStop.DAL.Migrations
{
    public partial class AddNewCountriesIsLiCh : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Country",
                columns: new[] { "Id", "Code", "PullingFromGatewayEnabled", "VisitedCountriesEnabled" },
                values: new object[,]
                {
                    { 30L, "IS", true, true },
                    { 31L, "LI", true, true },
                    { 32L, "CH", true, true }
                });

            migrationBuilder.InsertData(
                table: "Translation",
                columns: new[] { "Id", "EntityId", "EntityName", "EntityPropertyName", "LanguageCountryId", "Value" },
                values: new object[,]
                {
                    { 57L, 30L, "Country", "NameInDanish", 7L, "Island" },
                    { 58L, 31L, "Country", "NameInDanish", 7L, "Liechtenstein" },
                    { 59L, 32L, "Country", "NameInDanish", 7L, "Schweiz" },
                    { 60L, 30L, "Country", "NameInEnglish", 28L, "Iceland" },
                    { 61L, 31L, "Country", "NameInEnglish", 28L, "Liechtenstein" },
                    { 62L, 32L, "Country", "NameInEnglish", 28L, "Switzerland" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 30L);

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 31L);

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 32L);

            migrationBuilder.DeleteData(
                table: "Translation",
                keyColumn: "Id",
                keyValue: 57L);

            migrationBuilder.DeleteData(
                table: "Translation",
                keyColumn: "Id",
                keyValue: 58L);

            migrationBuilder.DeleteData(
                table: "Translation",
                keyColumn: "Id",
                keyValue: 59L);

            migrationBuilder.DeleteData(
                table: "Translation",
                keyColumn: "Id",
                keyValue: 60L);

            migrationBuilder.DeleteData(
                table: "Translation",
                keyColumn: "Id",
                keyValue: 61L);

            migrationBuilder.DeleteData(
                table: "Translation",
                keyColumn: "Id",
                keyValue: 62L);
        }
    }
}
