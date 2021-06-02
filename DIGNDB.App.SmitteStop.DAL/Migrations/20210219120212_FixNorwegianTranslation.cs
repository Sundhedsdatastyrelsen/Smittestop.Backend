using Microsoft.EntityFrameworkCore.Migrations;

namespace DIGNDB.App.SmitteStop.DAL.Migrations
{
    public partial class FixNorwegianTranslation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Translation",
                keyColumn: "Id",
                keyValue: 55L,
                column: "EntityId",
                value: 29L);

            migrationBuilder.UpdateData(
                table: "Translation",
                keyColumn: "Id",
                keyValue: 56L,
                columns: new[] { "EntityId", "LanguageCountryId" },
                values: new object[] { 29L, 28L });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Translation",
                keyColumn: "Id",
                keyValue: 55L,
                column: "EntityId",
                value: 28L);

            migrationBuilder.UpdateData(
                table: "Translation",
                keyColumn: "Id",
                keyValue: 56L,
                columns: new[] { "EntityId", "LanguageCountryId" },
                values: new object[] { 28L, 7L });
        }
    }
}
