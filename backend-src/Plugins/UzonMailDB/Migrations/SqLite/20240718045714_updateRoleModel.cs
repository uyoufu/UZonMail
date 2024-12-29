using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMailService.Migrations.SqLite
{
    /// <inheritdoc />
    public partial class updateRoleModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PermissionCodes_Roles_RoleId",
                table: "PermissionCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "RolePermissionCodes");

            migrationBuilder.DropIndex(
                name: "IX_Users_RoleId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_PermissionCodes_RoleId",
                table: "PermissionCodes");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "PermissionCodes");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Roles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Roles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PermissionCodeRole",
                columns: table => new
                {
                    PermissionCodesId = table.Column<long>(type: "INTEGER", nullable: false),
                    RolesId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionCodeRole", x => new { x.PermissionCodesId, x.RolesId });
                    table.ForeignKey(
                        name: "FK_PermissionCodeRole_PermissionCodes_PermissionCodesId",
                        column: x => x.PermissionCodesId,
                        principalTable: "PermissionCodes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PermissionCodeRole_Roles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PermissionCodes_Code",
                table: "PermissionCodes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PermissionCodeRole_RolesId",
                table: "PermissionCodeRole",
                column: "RolesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PermissionCodeRole");

            migrationBuilder.DropIndex(
                name: "IX_PermissionCodes_Code",
                table: "PermissionCodes");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Roles");

            migrationBuilder.AddColumn<long>(
                name: "RoleId",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RoleId",
                table: "PermissionCodes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RolePermissionCodes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    PermissionCodeId = table.Column<long>(type: "INTEGER", nullable: false),
                    RoleId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissionCodes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionCodes_RoleId",
                table: "PermissionCodes",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_PermissionCodes_Roles_RoleId",
                table: "PermissionCodes",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id");
        }
    }
}
