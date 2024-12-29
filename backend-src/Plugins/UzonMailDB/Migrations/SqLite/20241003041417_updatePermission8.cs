using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMail.DB.Migrations.SqLite
{
    /// <inheritdoc />
    public partial class updatePermission8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRole_Roles_RoleId",
                table: "UserRole");

            migrationBuilder.DropIndex(
                name: "IX_UserRole_RoleId",
                table: "UserRole");

            migrationBuilder.DropIndex(
                name: "IX_UserRole_UserId",
                table: "UserRole");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "UserRole",
                newName: "OrganizationId");

            migrationBuilder.AddColumn<long>(
                name: "OrganizationId",
                table: "Roles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "RoleUserRoles",
                columns: table => new
                {
                    RolesId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserRolesId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleUserRoles", x => new { x.RolesId, x.UserRolesId });
                    table.ForeignKey(
                        name: "FK_RoleUserRoles_Roles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RoleUserRoles_UserRole_UserRolesId",
                        column: x => x.UserRolesId,
                        principalTable: "UserRole",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_UserId",
                table: "UserRole",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleUserRoles_UserRolesId",
                table: "RoleUserRoles",
                column: "UserRolesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleUserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRole_UserId",
                table: "UserRole");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Roles");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "UserRole",
                newName: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_RoleId",
                table: "UserRole",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_UserId",
                table: "UserRole",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRole_Roles_RoleId",
                table: "UserRole",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id");
        }
    }
}
