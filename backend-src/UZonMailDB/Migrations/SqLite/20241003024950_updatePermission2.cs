using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMail.DB.Migrations.SqLite
{
    /// <inheritdoc />
    public partial class updatePermission2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Roles_UserRoles_UserRoleId",
                table: "Roles");

            migrationBuilder.RenameColumn(
                name: "UserRoleId",
                table: "Roles",
                newName: "RoleUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Roles_UserRoleId",
                table: "Roles",
                newName: "IX_Roles_RoleUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_UserRoles_RoleUserId",
                table: "Roles",
                column: "RoleUserId",
                principalTable: "UserRoles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Roles_UserRoles_RoleUserId",
                table: "Roles");

            migrationBuilder.RenameColumn(
                name: "RoleUserId",
                table: "Roles",
                newName: "UserRoleId");

            migrationBuilder.RenameIndex(
                name: "IX_Roles_RoleUserId",
                table: "Roles",
                newName: "IX_Roles_UserRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_UserRoles_UserRoleId",
                table: "Roles",
                column: "UserRoleId",
                principalTable: "UserRoles",
                principalColumn: "Id");
        }
    }
}
