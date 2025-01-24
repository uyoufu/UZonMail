using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMail.DB.Migrations.SqLite
{
    /// <inheritdoc />
    public partial class addCrawlerModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserProxies");

            migrationBuilder.CreateTable(
                name: "CrawlerEmailResults",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CrawlerTaskId = table.Column<long>(type: "INTEGER", nullable: false),
                    Nickname = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    TikTokAuthorId = table.Column<int>(type: "INTEGER", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrawlerEmailResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CrawlerTaskInfos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ProxyId = table.Column<long>(type: "INTEGER", nullable: false),
                    Deadline = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrawlerTaskInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proxies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    MatchRegex = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsShared = table.Column<bool>(type: "INTEGER", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    OrganizationId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proxies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiktokAuthors",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AvatarLarger = table.Column<string>(type: "TEXT", nullable: false),
                    AvatarMedium = table.Column<string>(type: "TEXT", nullable: false),
                    AvatarThumb = table.Column<string>(type: "TEXT", nullable: false),
                    CommentSetting = table.Column<int>(type: "INTEGER", nullable: false),
                    DownloadSetting = table.Column<int>(type: "INTEGER", nullable: false),
                    DueSetting = table.Column<int>(type: "INTEGER", nullable: false),
                    Ftc = table.Column<bool>(type: "INTEGER", nullable: false),
                    AuthorId = table.Column<string>(type: "TEXT", nullable: false),
                    IsAdVirtual = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsEmbedBanned = table.Column<bool>(type: "INTEGER", nullable: false),
                    Nickname = table.Column<string>(type: "TEXT", nullable: false),
                    OpenFavorite = table.Column<bool>(type: "INTEGER", nullable: false),
                    PrivateAccount = table.Column<bool>(type: "INTEGER", nullable: false),
                    Relation = table.Column<int>(type: "INTEGER", nullable: false),
                    SecUid = table.Column<string>(type: "TEXT", nullable: false),
                    Secret = table.Column<bool>(type: "INTEGER", nullable: false),
                    Signature = table.Column<string>(type: "TEXT", nullable: true),
                    StitchSetting = table.Column<int>(type: "INTEGER", nullable: false),
                    TtSeller = table.Column<bool>(type: "INTEGER", nullable: false),
                    UniqueId = table.Column<string>(type: "TEXT", nullable: false),
                    Verified = table.Column<bool>(type: "INTEGER", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    OrganizationId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiktokAuthors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TikTokAuthorDiversifications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TikTokAuthorId = table.Column<long>(type: "INTEGER", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TikTokAuthorDiversifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TikTokAuthorDiversifications_TiktokAuthors_TikTokAuthorId",
                        column: x => x.TikTokAuthorId,
                        principalTable: "TiktokAuthors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TikTokAuthStats",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TikTokAuthorId = table.Column<long>(type: "INTEGER", nullable: false),
                    DiggCount = table.Column<int>(type: "INTEGER", nullable: false),
                    FollwerCount = table.Column<int>(type: "INTEGER", nullable: false),
                    FollwingCount = table.Column<int>(type: "INTEGER", nullable: false),
                    FreindCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Heart = table.Column<int>(type: "INTEGER", nullable: false),
                    HeartCount = table.Column<int>(type: "INTEGER", nullable: false),
                    VideoCount = table.Column<int>(type: "INTEGER", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrawlerEmailResults");

            migrationBuilder.DropTable(
                name: "CrawlerTaskInfos");

            migrationBuilder.DropTable(
                name: "Proxies");

            migrationBuilder.DropTable(
                name: "TikTokAuthorDiversifications");

            migrationBuilder.DropTable(
                name: "TikTokAuthStats");

            migrationBuilder.DropTable(
                name: "TiktokAuthors");

            migrationBuilder.CreateTable(
                name: "UserProxies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsShared = table.Column<bool>(type: "INTEGER", nullable: false),
                    MatchRegex = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    OrganizationId = table.Column<long>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProxies", x => x.Id);
                });
        }
    }
}
