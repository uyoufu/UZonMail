using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMail.DB.Migrations.SqLite
{
    /// <inheritdoc />
    public partial class updateLicense : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LicenseInfos");

            migrationBuilder.AlterColumn<bool>(
                name: "EnableEmailTracker",
                table: "UserSettings",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTime",
                table: "SystemSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Json",
                table: "SystemSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LongValue",
                table: "SystemSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadDate",
                table: "SendingItems",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateTime",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "Json",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "LongValue",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "ReadDate",
                table: "SendingItems");

            migrationBuilder.AlterColumn<bool>(
                name: "EnableEmailTracker",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "LicenseInfos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ActiveDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpireDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LicenseKey = table.Column<string>(type: "TEXT", nullable: false),
                    LicenseType = table.Column<int>(type: "INTEGER", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseInfos", x => x.Id);
                });
        }
    }
}
