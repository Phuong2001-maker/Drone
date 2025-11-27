using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PT.Infrastructure.Migrations
{
    public partial class v26008 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BannerUrl",
                table: "Banner",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisplayTime",
                table: "Banner",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Banner",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Banner",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimeOut",
                table: "Banner",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BannerUrl",
                table: "Banner");

            migrationBuilder.DropColumn(
                name: "DisplayTime",
                table: "Banner");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Banner");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Banner");

            migrationBuilder.DropColumn(
                name: "TimeOut",
                table: "Banner");
        }
    }
}
