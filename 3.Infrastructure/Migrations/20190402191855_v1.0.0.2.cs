using Microsoft.EntityFrameworkCore.Migrations;

namespace PT.Infrastructure.Migrations
{
    public partial class v1002 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContentPage_Type_Delete_Status_Language",
                table: "ContentPage");

            migrationBuilder.CreateIndex(
                name: "IX_ContentPage_Type_Delete_Status_Language_DatePosted",
                table: "ContentPage",
                columns: new[] { "Type", "Delete", "Status", "Language", "DatePosted" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContentPage_Type_Delete_Status_Language_DatePosted",
                table: "ContentPage");

            migrationBuilder.CreateIndex(
                name: "IX_ContentPage_Type_Delete_Status_Language",
                table: "ContentPage",
                columns: new[] { "Type", "Delete", "Status", "Language" });
        }
    }
}
