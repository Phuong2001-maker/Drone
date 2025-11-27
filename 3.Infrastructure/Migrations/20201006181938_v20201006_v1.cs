using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PT.Infrastructure.Migrations
{
    public partial class v20201006_v1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContactLog",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IP = table.Column<string>(maxLength: 20, nullable: true),
                    Count = table.Column<int>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    LastConnection = table.Column<DateTime>(nullable: false),
                    IsBanlist = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactLog", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactLog_Type_LastConnection_Count",
                table: "ContactLog",
                columns: new[] { "Type", "LastConnection", "Count" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactLog");
        }
    }
}
