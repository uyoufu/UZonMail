using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMail.DB.Migrations.SqLite
{
    /// <inheritdoc />
    public partial class addEnableEmailTracker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnableEmailTracker",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableEmailTracker",
                table: "SendingItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FirstDate",
                table: "FileReaders",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastDate",
                table: "FileReaders",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "EmailAnchors",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SendingGroupId = table.Column<long>(type: "INTEGER", nullable: false),
                    SendingItemId = table.Column<long>(type: "INTEGER", nullable: false),
                    OutboxEmail = table.Column<string>(type: "TEXT", nullable: false),
                    InboxEmail = table.Column<string>(type: "TEXT", nullable: false),
                    VisitedCount = table.Column<int>(type: "INTEGER", nullable: false),
                    FirstVisitDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastVisitDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailAnchors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IPInfos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IP = table.Column<string>(type: "TEXT", nullable: false),
                    Country = table.Column<string>(type: "TEXT", nullable: true),
                    Region = table.Column<string>(type: "TEXT", nullable: true),
                    City = table.Column<string>(type: "TEXT", nullable: true),
                    District = table.Column<string>(type: "TEXT", nullable: true),
                    ISP = table.Column<string>(type: "TEXT", nullable: true),
                    PostalCode = table.Column<string>(type: "TEXT", nullable: true),
                    Latitude = table.Column<string>(type: "TEXT", nullable: true),
                    Longitude = table.Column<string>(type: "TEXT", nullable: true),
                    TimeZone = table.Column<string>(type: "TEXT", nullable: true),
                    UsageType = table.Column<string>(type: "TEXT", nullable: true),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IPInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailVisitHistories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IP = table.Column<string>(type: "TEXT", nullable: false),
                    EmailAnchorId = table.Column<long>(type: "INTEGER", nullable: true),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailVisitHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailVisitHistories_EmailAnchors_EmailAnchorId",
                        column: x => x.EmailAnchorId,
                        principalTable: "EmailAnchors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailVisitHistories_EmailAnchorId",
                table: "EmailVisitHistories",
                column: "EmailAnchorId");

            migrationBuilder.CreateIndex(
                name: "IX_IPInfos_IP",
                table: "IPInfos",
                column: "IP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailVisitHistories");

            migrationBuilder.DropTable(
                name: "IPInfos");

            migrationBuilder.DropTable(
                name: "EmailAnchors");

            migrationBuilder.DropColumn(
                name: "EnableEmailTracker",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "EnableEmailTracker",
                table: "SendingItems");

            migrationBuilder.DropColumn(
                name: "FirstDate",
                table: "FileReaders");

            migrationBuilder.DropColumn(
                name: "LastDate",
                table: "FileReaders");
        }
    }
}
