using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMail.DB.Migrations.Mysql
{
    /// <inheritdoc />
    public partial class removePro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrawlerTaskInfos");

            migrationBuilder.DropTable(
                name: "CrawlerTaskResults");

            migrationBuilder.DropTable(
                name: "DepartmentEmailTemplate");

            migrationBuilder.DropTable(
                name: "EmailTemplateSendingGroup");

            migrationBuilder.DropTable(
                name: "EmailTemplateUser");

            migrationBuilder.DropTable(
                name: "EmailVisitHistories");

            migrationBuilder.DropTable(
                name: "FileUsageSendingGroup");

            migrationBuilder.DropTable(
                name: "FileUsageSendingItem");

            migrationBuilder.DropTable(
                name: "IPInfos");

            migrationBuilder.DropTable(
                name: "OutboxSendingGroup");

            migrationBuilder.DropTable(
                name: "TikTokAuthorDiversifications");

            migrationBuilder.DropTable(
                name: "TikTokDevices");

            migrationBuilder.DropTable(
                name: "UnsubscribeButtons");

            migrationBuilder.DropTable(
                name: "UnsubscribeEmails");

            migrationBuilder.DropTable(
                name: "UnsubscribePages");

            migrationBuilder.DropTable(
                name: "UnsubscribeSettings");

            migrationBuilder.DropTable(
                name: "TiktokAuthors");

            migrationBuilder.DropTable(
                name: "EmailAnchors");

            migrationBuilder.AddColumn<long>(
                name: "EmailTemplateId",
                table: "Users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SendingGroupId",
                table: "Outboxes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SendingGroupId",
                table: "FileUsages",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SendingItemId",
                table: "FileUsages",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SendingGroupId",
                table: "EmailTemplates",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "EmailTemplateId",
                table: "Departments",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmailTemplateId",
                table: "Users",
                column: "EmailTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Outboxes_SendingGroupId",
                table: "Outboxes",
                column: "SendingGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_FileUsages_SendingGroupId",
                table: "FileUsages",
                column: "SendingGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_FileUsages_SendingItemId",
                table: "FileUsages",
                column: "SendingItemId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_SendingGroupId",
                table: "EmailTemplates",
                column: "SendingGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_EmailTemplateId",
                table: "Departments",
                column: "EmailTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_EmailTemplates_EmailTemplateId",
                table: "Departments",
                column: "EmailTemplateId",
                principalTable: "EmailTemplates",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailTemplates_SendingGroups_SendingGroupId",
                table: "EmailTemplates",
                column: "SendingGroupId",
                principalTable: "SendingGroups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FileUsages_SendingGroups_SendingGroupId",
                table: "FileUsages",
                column: "SendingGroupId",
                principalTable: "SendingGroups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FileUsages_SendingItems_SendingItemId",
                table: "FileUsages",
                column: "SendingItemId",
                principalTable: "SendingItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Outboxes_SendingGroups_SendingGroupId",
                table: "Outboxes",
                column: "SendingGroupId",
                principalTable: "SendingGroups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_EmailTemplates_EmailTemplateId",
                table: "Users",
                column: "EmailTemplateId",
                principalTable: "EmailTemplates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_EmailTemplates_EmailTemplateId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailTemplates_SendingGroups_SendingGroupId",
                table: "EmailTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_FileUsages_SendingGroups_SendingGroupId",
                table: "FileUsages");

            migrationBuilder.DropForeignKey(
                name: "FK_FileUsages_SendingItems_SendingItemId",
                table: "FileUsages");

            migrationBuilder.DropForeignKey(
                name: "FK_Outboxes_SendingGroups_SendingGroupId",
                table: "Outboxes");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_EmailTemplates_EmailTemplateId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_EmailTemplateId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Outboxes_SendingGroupId",
                table: "Outboxes");

            migrationBuilder.DropIndex(
                name: "IX_FileUsages_SendingGroupId",
                table: "FileUsages");

            migrationBuilder.DropIndex(
                name: "IX_FileUsages_SendingItemId",
                table: "FileUsages");

            migrationBuilder.DropIndex(
                name: "IX_EmailTemplates_SendingGroupId",
                table: "EmailTemplates");

            migrationBuilder.DropIndex(
                name: "IX_Departments_EmailTemplateId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "EmailTemplateId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SendingGroupId",
                table: "Outboxes");

            migrationBuilder.DropColumn(
                name: "SendingGroupId",
                table: "FileUsages");

            migrationBuilder.DropColumn(
                name: "SendingItemId",
                table: "FileUsages");

            migrationBuilder.DropColumn(
                name: "SendingGroupId",
                table: "EmailTemplates");

            migrationBuilder.DropColumn(
                name: "EmailTemplateId",
                table: "Departments");

            migrationBuilder.CreateTable(
                name: "CrawlerTaskInfos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Count = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Deadline = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsHidden = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _id = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OutboxGroupId = table.Column<long>(type: "bigint", nullable: false),
                    ProxyId = table.Column<long>(type: "bigint", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TikTokDeviceId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrawlerTaskInfos", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DepartmentEmailTemplate",
                columns: table => new
                {
                    EmailTemplateId = table.Column<long>(type: "bigint", nullable: false),
                    ShareToOrganizationsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentEmailTemplate", x => new { x.EmailTemplateId, x.ShareToOrganizationsId });
                    table.ForeignKey(
                        name: "FK_DepartmentEmailTemplate_Departments_ShareToOrganizationsId",
                        column: x => x.ShareToOrganizationsId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepartmentEmailTemplate_EmailTemplates_EmailTemplateId",
                        column: x => x.EmailTemplateId,
                        principalTable: "EmailTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmailAnchors",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreateDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FirstVisitDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    InboxEmails = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsHidden = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LastVisitDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    _id = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OutboxEmail = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SendingGroupId = table.Column<long>(type: "bigint", nullable: false),
                    SendingItemId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    VisitedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailAnchors", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmailTemplateSendingGroup",
                columns: table => new
                {
                    SendingGroupId = table.Column<long>(type: "bigint", nullable: false),
                    TemplatesId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplateSendingGroup", x => new { x.SendingGroupId, x.TemplatesId });
                    table.ForeignKey(
                        name: "FK_EmailTemplateSendingGroup_EmailTemplates_TemplatesId",
                        column: x => x.TemplatesId,
                        principalTable: "EmailTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmailTemplateSendingGroup_SendingGroups_SendingGroupId",
                        column: x => x.SendingGroupId,
                        principalTable: "SendingGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmailTemplateUser",
                columns: table => new
                {
                    EmailTemplateId = table.Column<long>(type: "bigint", nullable: false),
                    ShareToUsersId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplateUser", x => new { x.EmailTemplateId, x.ShareToUsersId });
                    table.ForeignKey(
                        name: "FK_EmailTemplateUser_EmailTemplates_EmailTemplateId",
                        column: x => x.EmailTemplateId,
                        principalTable: "EmailTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmailTemplateUser_Users_ShareToUsersId",
                        column: x => x.ShareToUsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FileUsageSendingGroup",
                columns: table => new
                {
                    AttachmentsId = table.Column<long>(type: "bigint", nullable: false),
                    SendingGroupId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileUsageSendingGroup", x => new { x.AttachmentsId, x.SendingGroupId });
                    table.ForeignKey(
                        name: "FK_FileUsageSendingGroup_FileUsages_AttachmentsId",
                        column: x => x.AttachmentsId,
                        principalTable: "FileUsages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FileUsageSendingGroup_SendingGroups_SendingGroupId",
                        column: x => x.SendingGroupId,
                        principalTable: "SendingGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FileUsageSendingItem",
                columns: table => new
                {
                    AttachmentsId = table.Column<long>(type: "bigint", nullable: false),
                    SendingItemId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileUsageSendingItem", x => new { x.AttachmentsId, x.SendingItemId });
                    table.ForeignKey(
                        name: "FK_FileUsageSendingItem_FileUsages_AttachmentsId",
                        column: x => x.AttachmentsId,
                        principalTable: "FileUsages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FileUsageSendingItem_SendingItems_SendingItemId",
                        column: x => x.SendingItemId,
                        principalTable: "SendingItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "IPInfos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    City = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Country = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreateDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    District = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IP = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ISP = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsHidden = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Latitude = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Longitude = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _id = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PostalCode = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Region = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TimeZone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UsageType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IPInfos", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OutboxSendingGroup",
                columns: table => new
                {
                    OutboxesId = table.Column<long>(type: "bigint", nullable: false),
                    SendingGroupId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxSendingGroup", x => new { x.OutboxesId, x.SendingGroupId });
                    table.ForeignKey(
                        name: "FK_OutboxSendingGroup_Outboxes_OutboxesId",
                        column: x => x.OutboxesId,
                        principalTable: "Outboxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OutboxSendingGroup_SendingGroups_SendingGroupId",
                        column: x => x.SendingGroupId,
                        principalTable: "SendingGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TikTokAuthorDiversifications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreateDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DiversificationId = table.Column<long>(type: "bigint", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsHidden = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    _id = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TikTokAuthorId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TikTokAuthorDiversifications", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TiktokAuthors",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AvatarLarger = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AvatarMedium = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AvatarThumb = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CommentSetting = table.Column<int>(type: "int", nullable: false),
                    CrawledCount = table.Column<long>(type: "bigint", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DiggCount = table.Column<long>(type: "bigint", nullable: false),
                    DownloadSetting = table.Column<int>(type: "int", nullable: false),
                    DueSetting = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FollowingAuthorId = table.Column<long>(type: "bigint", nullable: false),
                    FollwerCount = table.Column<long>(type: "bigint", nullable: false),
                    FollwingCount = table.Column<long>(type: "bigint", nullable: false),
                    FreindCount = table.Column<long>(type: "bigint", nullable: false),
                    Ftc = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Heart = table.Column<long>(type: "bigint", nullable: false),
                    HeartCount = table.Column<long>(type: "bigint", nullable: false),
                    Instagram = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsAdVirtual = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsEmbedBanned = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsHidden = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsParsed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Nickname = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _id = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OpenFavorite = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    OrganizationId = table.Column<long>(type: "bigint", nullable: false),
                    Phone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PrivateAccount = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Relation = table.Column<int>(type: "int", nullable: false),
                    SecUid = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Secret = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Signature = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StitchSetting = table.Column<int>(type: "int", nullable: false),
                    Telegram = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TtSeller = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UniqueId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Verified = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    VideoCount = table.Column<long>(type: "bigint", nullable: false),
                    WhatsApp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Youtube = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiktokAuthors", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TikTokDevices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreateDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeviceId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsHidden = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsShared = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _id = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OdinId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrganizationId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TikTokDevices", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UnsubscribeButtons",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ButtonHtml = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreateDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsHidden = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _id = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrganizationId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnsubscribeButtons", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UnsubscribeEmails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreateDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Email = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Host = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsHidden = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    _id = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrganizationId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnsubscribeEmails", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UnsubscribePages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreateDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    HtmlContent = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsHidden = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Language = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _id = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrganizationId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnsubscribePages", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UnsubscribeSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreateDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Enable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ExternalUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsHidden = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    _id = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrganizationId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    UnsubscribeButtonId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnsubscribeSettings", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmailVisitHistories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreateDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EmailAnchorId = table.Column<long>(type: "bigint", nullable: true),
                    IP = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsHidden = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    _id = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailVisitHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailVisitHistories_EmailAnchors_EmailAnchorId",
                        column: x => x.EmailAnchorId,
                        principalTable: "EmailAnchors",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CrawlerTaskResults",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TikTokAuthorId = table.Column<long>(type: "bigint", nullable: false),
                    CrawlerTaskInfoId = table.Column<long>(type: "bigint", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ExistExtraInfo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsAttachingInbox = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsHidden = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    _id = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrawlerTaskResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrawlerTaskResults_TiktokAuthors_TikTokAuthorId",
                        column: x => x.TikTokAuthorId,
                        principalTable: "TiktokAuthors",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CrawlerTaskResults_TikTokAuthorId",
                table: "CrawlerTaskResults",
                column: "TikTokAuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentEmailTemplate_ShareToOrganizationsId",
                table: "DepartmentEmailTemplate",
                column: "ShareToOrganizationsId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplateSendingGroup_TemplatesId",
                table: "EmailTemplateSendingGroup",
                column: "TemplatesId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplateUser_ShareToUsersId",
                table: "EmailTemplateUser",
                column: "ShareToUsersId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailVisitHistories_EmailAnchorId",
                table: "EmailVisitHistories",
                column: "EmailAnchorId");

            migrationBuilder.CreateIndex(
                name: "IX_FileUsageSendingGroup_SendingGroupId",
                table: "FileUsageSendingGroup",
                column: "SendingGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_FileUsageSendingItem_SendingItemId",
                table: "FileUsageSendingItem",
                column: "SendingItemId");

            migrationBuilder.CreateIndex(
                name: "IX_IPInfos_IP",
                table: "IPInfos",
                column: "IP");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxSendingGroup_SendingGroupId",
                table: "OutboxSendingGroup",
                column: "SendingGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_UnsubscribeEmails_OrganizationId_Email",
                table: "UnsubscribeEmails",
                columns: new[] { "OrganizationId", "Email" });
        }
    }
}
