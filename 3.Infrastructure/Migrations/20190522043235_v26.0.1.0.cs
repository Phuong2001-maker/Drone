using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PT.Infrastructure.Migrations
{
    public partial class v26010 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoleParents",
                schema: "adm",
                table: "User");

            migrationBuilder.DropColumn(
                name: "RoleSchool",
                schema: "adm",
                table: "User");

            migrationBuilder.DropColumn(
                name: "RoleTransportCompany",
                schema: "adm",
                table: "User");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationResetPassword",
                schema: "adm",
                table: "User",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationWrongPassword",
                schema: "adm",
                table: "User",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberWrongPasswords",
                schema: "adm",
                table: "User",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpirationResetPassword",
                schema: "adm",
                table: "User");

            migrationBuilder.DropColumn(
                name: "ExpirationWrongPassword",
                schema: "adm",
                table: "User");

            migrationBuilder.DropColumn(
                name: "NumberWrongPasswords",
                schema: "adm",
                table: "User");

            migrationBuilder.AddColumn<string>(
                name: "RoleParents",
                schema: "adm",
                table: "User",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoleSchool",
                schema: "adm",
                table: "User",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoleTransportCompany",
                schema: "adm",
                table: "User",
                nullable: true);
        }
    }
}
