using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace VSign.Migrations
{
    public partial class UserUpdated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiringOn",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Pass1",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Pass2",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Pass3",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TourStatus",
                table: "Users");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiringOn",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pass1",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pass2",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pass3",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TourStatus",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
