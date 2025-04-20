using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class init_app_role : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppRoles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    JsonRequest = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    JsonRequestStatus = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppRolePermissions",
                columns: table => new
                {
                    AppRoleId = table.Column<long>(type: "bigint", nullable: false),
                    PermissionId = table.Column<string>(type: "nvarchar(37)", maxLength: 37, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRolePermissions", x => new { x.AppRoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_AppRolePermissions_AppRoles_AppRoleId",
                        column: x => x.AppRoleId,
                        principalTable: "AppRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppRolePermissions");

            migrationBuilder.DropTable(
                name: "AppRoles");
        }
    }
}
