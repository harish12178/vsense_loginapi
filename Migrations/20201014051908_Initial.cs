using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace VSign.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Apps",
                columns: table => new
                {
                    AppID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppName = table.Column<string>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    ModifiedOn = table.Column<DateTime>(nullable: true),
                    ModifiedBy = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Apps", x => x.AppID);
                });

            migrationBuilder.CreateTable(
                name: "AppUsages",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<Guid>(nullable: false),
                    AppName = table.Column<string>(nullable: true),
                    UsageCount = table.Column<int>(nullable: false),
                    LastUsedOn = table.Column<DateTime>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    ModifiedOn = table.Column<DateTime>(nullable: true),
                    ModifiedBy = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsages", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Secret = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    ApplicationType = table.Column<int>(nullable: false),
                    Active = table.Column<bool>(nullable: false),
                    RefreshTokenLifeTime = table.Column<int>(nullable: false),
                    AllowedOrigin = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleAppMaps",
                columns: table => new
                {
                    RoleID = table.Column<Guid>(nullable: false),
                    AppID = table.Column<int>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    ModifiedOn = table.Column<DateTime>(nullable: true),
                    ModifiedBy = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleAppMaps", x => new { x.RoleID, x.AppID });
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleID = table.Column<Guid>(nullable: false),
                    RoleName = table.Column<string>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    ModifiedOn = table.Column<DateTime>(nullable: true),
                    ModifiedBy = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "SessionMasters",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectName = table.Column<string>(nullable: true),
                    SessionTimeOut = table.Column<int>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    ModifiedOn = table.Column<DateTime>(nullable: true),
                    ModifiedBy = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionMasters", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TokenHistories",
                columns: table => new
                {
                    TokenHistoryID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<Guid>(nullable: false),
                    UserName = table.Column<string>(nullable: true),
                    Token = table.Column<string>(nullable: true),
                    EmailAddress = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    ExpireOn = table.Column<DateTime>(nullable: false),
                    UsedOn = table.Column<DateTime>(nullable: true),
                    IsUsed = table.Column<bool>(nullable: false),
                    Comment = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenHistories", x => x.TokenHistoryID);
                });

            migrationBuilder.CreateTable(
                name: "UserLoginHistory",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<Guid>(nullable: false),
                    UserName = table.Column<string>(nullable: true),
                    LoginTime = table.Column<DateTime>(nullable: false),
                    LogoutTime = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLoginHistory", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "UserPreferences",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<Guid>(nullable: false),
                    NavbarPrimaryBackground = table.Column<string>(nullable: true),
                    NavbarSecondaryBackground = table.Column<string>(nullable: true),
                    ToolbarBackground = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    ModifiedOn = table.Column<DateTime>(nullable: false),
                    ModifiedBy = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "UserRoleMaps",
                columns: table => new
                {
                    UserID = table.Column<Guid>(nullable: false),
                    RoleID = table.Column<Guid>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    ModifiedOn = table.Column<DateTime>(nullable: true),
                    ModifiedBy = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoleMaps", x => new { x.UserID, x.RoleID });
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<Guid>(nullable: false),
                    UserName = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    Password = table.Column<string>(nullable: true),
                    ContactNumber = table.Column<string>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    ModifiedOn = table.Column<DateTime>(nullable: true),
                    ModifiedBy = table.Column<string>(nullable: true),
                    DisplayName = table.Column<string>(nullable: true),
                    TourStatus = table.Column<bool>(nullable: false),
                    Pass1 = table.Column<string>(nullable: true),
                    Pass2 = table.Column<string>(nullable: true),
                    Pass3 = table.Column<string>(nullable: true),
                    ExpiringOn = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                });

            migrationBuilder.InsertData(
                table: "Clients",
                columns: new[] { "Id", "Active", "AllowedOrigin", "ApplicationType", "Name", "RefreshTokenLifeTime", "Secret" },
                values: new object[] { "ngAuthApp", true, "*", 0, "AngularJS front-end Application", 7200, "5YV7M1r981yoGhELyB84aC+KiYksxZf1OY3++C1CtRM=" });

            migrationBuilder.InsertData(
                table: "Clients",
                columns: new[] { "Id", "Active", "AllowedOrigin", "ApplicationType", "Name", "RefreshTokenLifeTime", "Secret" },
                values: new object[] { "consoleApp", true, "*", 1, "Console Application", 14400, "lCXDroz4HhR1EIx8qaz3C13z/quTXBkQ3Q5hj7Qx3aA=" });

            migrationBuilder.CreateIndex(
                name: "IX_AppUsages_UserID_AppName",
                table: "AppUsages",
                columns: new[] { "UserID", "AppName" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Apps");

            migrationBuilder.DropTable(
                name: "AppUsages");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "RoleAppMaps");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "SessionMasters");

            migrationBuilder.DropTable(
                name: "TokenHistories");

            migrationBuilder.DropTable(
                name: "UserLoginHistory");

            migrationBuilder.DropTable(
                name: "UserPreferences");

            migrationBuilder.DropTable(
                name: "UserRoleMaps");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
