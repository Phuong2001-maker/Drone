using Microsoft.EntityFrameworkCore.Migrations;

namespace PT.Infrastructure.Migrations
{
    public partial class v20230705_t2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TemplateType",
                table: "Category");

            migrationBuilder.AddColumn<int>(
                name: "TemplateType",
                table: "ContentPage",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TemplateType",
                table: "ContentPage");

            migrationBuilder.AddColumn<int>(
                name: "TemplateType",
                table: "Category",
                nullable: true);
        }
    }
}
