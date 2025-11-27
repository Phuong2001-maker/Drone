using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PT.Infrastructure.Migrations
{
    public partial class v103 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContentPageReference",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ContentPageId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Href = table.Column<string>(nullable: true),
                    Target = table.Column<string>(nullable: true),
                    Rel = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentPageReference", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentPageReference_ContentPageId",
                table: "ContentPageReference",
                column: "ContentPageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentPageReference");
        }
    }
}
