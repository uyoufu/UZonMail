using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMail.DB.Migrations.SqLite
{
    /// <inheritdoc />
    public partial class updateAuthorInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TikTokAuthorDiversifications_TiktokAuthors_TikTokAuthorId",
                table: "TikTokAuthorDiversifications");

            migrationBuilder.DropTable(
                name: "TikTokAuthorExtras");

            migrationBuilder.DropTable(
                name: "TikTokAuthStats");

            migrationBuilder.DropIndex(
                name: "IX_TikTokAuthorDiversifications_TikTokAuthorId",
                table: "TikTokAuthorDiversifications");

            migrationBuilder.AddColumn<long>(
                name: "CrawledCount",
                table: "TiktokAuthors",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "DiggCount",
                table: "TiktokAuthors",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "TiktokAuthors",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FollowingAuthorId",
                table: "TiktokAuthors",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "FollwerCount",
                table: "TiktokAuthors",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FollwingCount",
                table: "TiktokAuthors",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FreindCount",
                table: "TiktokAuthors",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Heart",
                table: "TiktokAuthors",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HeartCount",
                table: "TiktokAuthors",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Instagram",
                table: "TiktokAuthors",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsParsed",
                table: "TiktokAuthors",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "TiktokAuthors",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telegram",
                table: "TiktokAuthors",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VideoCount",
                table: "TiktokAuthors",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "WhatsApp",
                table: "TiktokAuthors",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Youtube",
                table: "TiktokAuthors",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DiversificationId",
                table: "TikTokAuthorDiversifications",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "TikTokDeviceId",
                table: "CrawlerTaskInfos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "CrawlerTaskResults",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CrawlerTaskInfoId = table.Column<long>(type: "INTEGER", nullable: false),
                    TikTokAuthorId = table.Column<long>(type: "INTEGER", nullable: false),
                    ExistExtraInfo = table.Column<bool>(type: "INTEGER", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrawlerTaskResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TikTokDevices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    DeviceId = table.Column<long>(type: "INTEGER", nullable: false),
                    OdinId = table.Column<long>(type: "INTEGER", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    OrganizationId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TikTokDevices", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrawlerTaskResults");

            migrationBuilder.DropTable(
                name: "TikTokDevices");

            migrationBuilder.DropColumn(
                name: "CrawledCount",
                table: "TiktokAuthors");

            migrationBuilder.DropColumn(
                name: "DiggCount",
                table: "TiktokAuthors");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "TiktokAuthors");

            migrationBuilder.DropColumn(
                name: "FollowingAuthorId",
                table: "TiktokAuthors");

            migrationBuilder.DropColumn(
                name: "FollwerCount",
                table: "TiktokAuthors");

            migrationBuilder.DropColumn(
                name: "FollwingCount",
                table: "TiktokAuthors");

            migrationBuilder.DropColumn(
                name: "FreindCount",
                table: "TiktokAuthors");

            migrationBuilder.DropColumn(
                name: "Heart",
                table: "TiktokAuthors");

            migrationBuilder.DropColumn(
                name: "HeartCount",
                table: "TiktokAuthors");

            migrationBuilder.DropColumn(
                name: "Instagram",
                table: "TiktokAuthors");

            migrationBuilder.DropColumn(
                name: "IsParsed",
                table: "TiktokAuthors");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "TiktokAuthors");

            migrationBuilder.DropColumn(
                name: "Telegram",
                table: "TiktokAuthors");

            migrationBuilder.DropColumn(
                name: "VideoCount",
                table: "TiktokAuthors");

            migrationBuilder.DropColumn(
                name: "WhatsApp",
                table: "TiktokAuthors");

            migrationBuilder.DropColumn(
                name: "Youtube",
                table: "TiktokAuthors");

            migrationBuilder.DropColumn(
                name: "DiversificationId",
                table: "TikTokAuthorDiversifications");

            migrationBuilder.DropColumn(
                name: "TikTokDeviceId",
                table: "CrawlerTaskInfos");

            migrationBuilder.CreateTable(
                name: "TikTokAuthorExtras",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CrawlerTaskId = table.Column<long>(type: "INTEGER", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Instagram = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    Nickname = table.Column<string>(type: "TEXT", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: true),
                    Telegram = table.Column<string>(type: "TEXT", nullable: true),
                    TikTokAuthorId = table.Column<long>(type: "INTEGER", nullable: false),
                    WhatsApp = table.Column<string>(type: "TEXT", nullable: true),
                    Youtube = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TikTokAuthorExtras", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TikTokAuthStats",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TikTokAuthorId = table.Column<long>(type: "INTEGER", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DiggCount = table.Column<int>(type: "INTEGER", nullable: false),
                    FollwerCount = table.Column<int>(type: "INTEGER", nullable: false),
                    FollwingCount = table.Column<int>(type: "INTEGER", nullable: false),
                    FreindCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Heart = table.Column<int>(type: "INTEGER", nullable: false),
                    HeartCount = table.Column<int>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    VideoCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TikTokAuthStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TikTokAuthStats_TiktokAuthors_TikTokAuthorId",
                        column: x => x.TikTokAuthorId,
                        principalTable: "TiktokAuthors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TikTokAuthorDiversifications_TikTokAuthorId",
                table: "TikTokAuthorDiversifications",
                column: "TikTokAuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_TikTokAuthStats_TikTokAuthorId",
                table: "TikTokAuthStats",
                column: "TikTokAuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_TikTokAuthorDiversifications_TiktokAuthors_TikTokAuthorId",
                table: "TikTokAuthorDiversifications",
                column: "TikTokAuthorId",
                principalTable: "TiktokAuthors",
                principalColumn: "Id");
        }
    }
}
