using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMail.DB.Migrations.SqLite
{
    /// <inheritdoc />
    public partial class updateFileObject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "UserSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "UserRoles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "UserProxies",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "SystemSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "SendingItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "SendingItemInboxes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "SendingGroups",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "Roles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "PermissionCodes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "Outboxes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "LicenseInfos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "Inboxes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "IdAndName",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "FileUsages",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "FileReaders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowAnonymousReading",
                table: "FileObjects",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "FileObjects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "FileBuckets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "EmailTemplates",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "EmailGroups",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "EmailAddress",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "DepartmentSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "Departments",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "_id",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "_id",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "_id",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "_id",
                table: "UserProxies");

            migrationBuilder.DropColumn(
                name: "_id",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "_id",
                table: "SendingItems");

            migrationBuilder.DropColumn(
                name: "_id",
                table: "SendingItemInboxes");

            migrationBuilder.DropColumn(
                name: "_id",
                table: "SendingGroups");

            migrationBuilder.DropColumn(
                name: "_id",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "_id",
                table: "PermissionCodes");

            migrationBuilder.DropColumn(
                name: "_id",
                table: "Outboxes");

            migrationBuilder.DropColumn(
                name: "_id",
                table: "LicenseInfos");

            migrationBuilder.DropColumn(
                name: "_id",
                table: "Inboxes");

            migrationBuilder.DropColumn(
                name: "_id",
                table: "IdAndName");

            migrationBuilder.DropColumn(
                name: "_id",
                table: "FileUsages");

            migrationBuilder.DropColumn(
                name: "_id",
                table: "FileReaders");

            migrationBuilder.DropColumn(
                name: "AllowAnonymousReading",
                table: "FileObjects");

            migrationBuilder.DropColumn(
                name: "_id",
                table: "FileObjects");

            migrationBuilder.DropColumn(
                name: "_id",
                table: "FileBuckets");

            migrationBuilder.DropColumn(
                name: "_id",
                table: "EmailTemplates");

            migrationBuilder.DropColumn(
                name: "_id",
                table: "EmailGroups");

            migrationBuilder.DropColumn(
                name: "_id",
                table: "EmailAddress");

            migrationBuilder.DropColumn(
                name: "_id",
                table: "DepartmentSettings");

            migrationBuilder.DropColumn(
                name: "_id",
                table: "Departments");
        }
    }
}
