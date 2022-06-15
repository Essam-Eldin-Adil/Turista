using Microsoft.EntityFrameworkCore.Migrations;

namespace Turista.Migrations
{
    public partial class CompleteStep : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompleteStep",
                table: "Resorts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CompleteStep",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompleteStep",
                table: "Resorts");

            migrationBuilder.DropColumn(
                name: "CompleteStep",
                table: "Properties");
        }
    }
}
