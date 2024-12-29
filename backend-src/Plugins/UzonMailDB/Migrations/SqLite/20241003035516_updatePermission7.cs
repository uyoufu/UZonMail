using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMail.DB.Migrations.SqLite
{
    /// <inheritdoc />
    public partial class updatePermission7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRole_Roles_RoleId1",
                table: "UserRole");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRole_Roles_RolesId",
                table: "UserRole");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRole_Users_UserId1",
                table: "UserRole");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRole_Users_UsersId",
                table: "UserRole");

            migrationBuilder.DropIndex(
                name: "IX_UserRole_RoleId1",
                table: "UserRole");

            migrationBuilder.DropIndex(
                name: "IX_UserRole_RolesId",
                table: "UserRole");

            migrationBuilder.DropIndex(
                name: "IX_UserRole_UserId1",
                table: "UserRole");

            migrationBuilder.DropIndex(
                name: "IX_UserRole_UsersId",
                table: "UserRole");

            migrationBuilder.DropColumn(
                name: "RoleId1",
                table: "UserRole");

            migrationBuilder.DropColumn(
                name: "RolesId",
                table: "UserRole");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "UserRole");

            migrationBuilder.DropColumn(
                name: "UsersId",
                table: "UserRole");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "RoleId1",
                table: "UserRole",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "RolesId",
                table: "UserRole",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UserId1",
                table: "UserRole",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UsersId",
                table: "UserRole",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_RoleId1",
                table: "UserRole",
                column: "RoleId1");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_RolesId",
                table: "UserRole",
                column: "RolesId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_UserId1",
                table: "UserRole",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_UsersId",
                table: "UserRole",
                column: "UsersId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRole_Roles_RoleId1",
                table: "UserRole",
                column: "RoleId1",
                principalTable: "Roles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRole_Roles_RolesId",
                table: "UserRole",
                column: "RolesId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRole_Users_UserId1",
                table: "UserRole",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRole_Users_UsersId",
                table: "UserRole",
                column: "UsersId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
