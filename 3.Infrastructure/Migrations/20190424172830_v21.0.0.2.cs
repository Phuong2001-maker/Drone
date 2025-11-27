using Microsoft.EntityFrameworkCore.Migrations;

namespace PT.Infrastructure.Migrations
{
    public partial class v21002 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Banner2",
                table: "Category",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IsHome",
                table: "Category",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Banner2",
                table: "Category");

            migrationBuilder.DropColumn(
                name: "IsHome",
                table: "Category");
        }
    }
}
