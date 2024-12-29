using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMail.DB.Migrations.Mysql
{
    /// <inheritdoc />
    public partial class updteFileObject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "UserSettings",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "Users",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "UserRoles",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "UserProxies",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "SystemSettings",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "SendingItems",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "SendingItemInboxes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "SendingGroups",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "Roles",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "PermissionCodes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "Outboxes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "LicenseInfos",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "Inboxes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "IdAndName",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "FileUsages",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "MaxVisitCount",
                table: "FileReaders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VisitedCount",
                table: "FileReaders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "FileReaders",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "FileObjects",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "FileBuckets",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "EmailTemplates",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "EmailGroups",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "EmailAddress",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "DepartmentSettings",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "_id",
                table: "Departments",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
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
                name: "MaxVisitCount",
                table: "FileReaders");

            migrationBuilder.DropColumn(
                name: "VisitedCount",
                table: "FileReaders");

            migrationBuilder.DropColumn(
                name: "_id",
                table: "FileReaders");

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
