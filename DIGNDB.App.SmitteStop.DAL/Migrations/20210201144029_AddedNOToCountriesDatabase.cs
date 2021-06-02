using Microsoft.EntityFrameworkCore.Migrations;

namespace DIGNDB.App.SmitteStop.DAL.Migrations
{
    public partial class AddedNOToCountriesDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Country",
                columns: new[] { "Id", "Code", "PullingFromGatewayEnabled", "VisitedCountriesEnabled" },
                values: new object[] { 29L, "NO", true, false });

            migrationBuilder.InsertData(
                table: "Translation",
                columns: new[] { "Id", "EntityId", "EntityName", "EntityPropertyName", "LanguageCountryId", "Value" },
                values: new object[] { 55L, 28L, "Country", "NameInDanish", 7L, "Norge" });

            migrationBuilder.InsertData(
                table: "Translation",
                columns: new[] { "Id", "EntityId", "EntityName", "EntityPropertyName", "LanguageCountryId", "Value" },
                values: new object[] { 56L, 28L, "Country", "NameInEnglish", 7L, "Norway" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 29L);

            migrationBuilder.DeleteData(
                table: "Translation",
                keyColumn: "Id",
                keyValue: 55L);

            migrationBuilder.DeleteData(
                table: "Translation",
                keyColumn: "Id",
                keyValue: 56L);
        }
    }
}
