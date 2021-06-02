using Microsoft.EntityFrameworkCore.Migrations;

namespace DIGNDB.App.SmitteStop.DAL.Migrations
{
    public partial class Remove_Pattient_field : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PatientsAdmittedToday",
                table: "SSIStatistics");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PatientsAdmittedToday",
                table: "SSIStatistics",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
