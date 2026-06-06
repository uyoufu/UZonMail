using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMailService.Migrations.SqLite
{
    /// <inheritdoc />
    public partial class initSqLite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    ParentId = table.Column<long>(type: "INTEGER", nullable: true),
                    FullPath = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    IsSystem = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailTemplates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    Thumbnail = table.Column<string>(type: "TEXT", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FileBuckets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BucketName = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    RootDir = table.Column<string>(type: "TEXT", nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileBuckets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Outboxes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SmtpHost = table.Column<string>(type: "TEXT", nullable: false),
                    SmtpPort = table.Column<int>(type: "INTEGER", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", nullable: true),
                    Password = table.Column<string>(type: "TEXT", nullable: false),
                    EnableSSL = table.Column<bool>(type: "INTEGER", nullable: false),
                    ProxyId = table.Column<long>(type: "INTEGER", nullable: false),
                    MaxSendCountPerDay = table.Column<int>(type: "INTEGER", nullable: false),
                    SentTotalToday = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    EmailGroupId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Domain = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Remark = table.Column<string>(type: "TEXT", nullable: true),
                    LinkCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Outboxes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissionCodes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<long>(type: "INTEGER", nullable: false),
                    PermissionCodeId = table.Column<long>(type: "INTEGER", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissionCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SendingGroups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    Subjects = table.Column<string>(type: "TEXT", nullable: false),
                    Body = table.Column<string>(type: "TEXT", nullable: true),
                    Inboxes = table.Column<string>(type: "TEXT", nullable: false),
                    CcBoxes = table.Column<string>(type: "TEXT", nullable: true),
                    BccBoxes = table.Column<string>(type: "TEXT", nullable: true),
                    Data = table.Column<string>(type: "TEXT", nullable: true),
                    IsDistributed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TotalCount = table.Column<int>(type: "INTEGER", nullable: false),
                    SuccessCount = table.Column<int>(type: "INTEGER", nullable: false),
                    SentCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    SendStartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SendEndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastMessage = table.Column<string>(type: "TEXT", nullable: true),
                    SendingType = table.Column<int>(type: "INTEGER", nullable: false),
                    ScheduleDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SendingGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    StringValue = table.Column<string>(type: "TEXT", nullable: true),
                    BoolValue = table.Column<bool>(type: "INTEGER", nullable: false),
                    IntValue = table.Column<int>(type: "INTEGER", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserProxies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    MatchRegex = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Proxy = table.Column<string>(type: "TEXT", nullable: false),
                    IsShared = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProxies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    MaxSendCountPerEmailDay = table.Column<int>(type: "INTEGER", nullable: false),
                    MinOutboxCooldownSecond = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxOutboxCooldownSecond = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxSendingBatchSize = table.Column<int>(type: "INTEGER", nullable: false),
                    MinInboxCooldownHours = table.Column<int>(type: "INTEGER", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FileObjects",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileBucketId = table.Column<long>(type: "INTEGER", nullable: false),
                    LastModifyDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Sha256 = table.Column<string>(type: "TEXT", nullable: false),
                    Path = table.Column<string>(type: "TEXT", nullable: false),
                    LinkCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileObjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileObjects_FileBuckets_FileBucketId",
                        column: x => x.FileBucketId,
                        principalTable: "FileBuckets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PermissionCodes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    RoleId = table.Column<long>(type: "INTEGER", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PermissionCodes_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizationId = table.Column<long>(type: "INTEGER", nullable: false),
                    DepartmentId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", nullable: true),
                    Password = table.Column<string>(type: "TEXT", nullable: false),
                    Avatar = table.Column<string>(type: "TEXT", nullable: true),
                    ConnectionId = table.Column<string>(type: "TEXT", nullable: true),
                    ForbiddenToLogin = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSuperAdmin = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSystemUser = table.Column<bool>(type: "INTEGER", nullable: false),
                    RoleId = table.Column<long>(type: "INTEGER", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EmailTemplateSendingGroup",
                columns: table => new
                {
                    SendingGroupId = table.Column<long>(type: "INTEGER", nullable: false),
                    TemplatesId = table.Column<long>(type: "INTEGER", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "OutboxSendingGroup",
                columns: table => new
                {
                    OutboxesId = table.Column<long>(type: "INTEGER", nullable: false),
                    SendingGroupId = table.Column<long>(type: "INTEGER", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "SendingItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SendingGroupId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    OutBoxId = table.Column<long>(type: "INTEGER", nullable: false),
                    FromEmail = table.Column<string>(type: "TEXT", nullable: true),
                    Inboxes = table.Column<string>(type: "TEXT", nullable: false),
                    CC = table.Column<string>(type: "TEXT", nullable: true),
                    BCC = table.Column<string>(type: "TEXT", nullable: true),
                    ToEmails = table.Column<string>(type: "TEXT", nullable: true),
                    EmailTemplateId = table.Column<long>(type: "INTEGER", nullable: false),
                    Subject = table.Column<string>(type: "TEXT", nullable: true),
                    Content = table.Column<string>(type: "TEXT", nullable: true),
                    IsSendingBatch = table.Column<bool>(type: "INTEGER", nullable: false),
                    ProxyId = table.Column<long>(type: "INTEGER", nullable: false),
                    Data = table.Column<string>(type: "TEXT", nullable: true),
                    SendDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    SendResult = table.Column<string>(type: "TEXT", nullable: true),
                    TriedCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ReceiptId = table.Column<string>(type: "TEXT", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    OrganizationId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SendingItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SendingItems_SendingGroups_SendingGroupId",
                        column: x => x.SendingGroupId,
                        principalTable: "SendingGroups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FileReaders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    FileObjectId = table.Column<long>(type: "INTEGER", nullable: false),
                    ExpireDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileReaders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileReaders_FileObjects_FileObjectId",
                        column: x => x.FileObjectId,
                        principalTable: "FileObjects",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EmailGroups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Icon = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    ParentId = table.Column<int>(type: "INTEGER", nullable: false),
                    Order = table.Column<long>(type: "INTEGER", nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailGroups_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FileUsages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OwnerUserId = table.Column<long>(type: "INTEGER", nullable: false),
                    UniqueName = table.Column<string>(type: "TEXT", nullable: true),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    FileObjectId = table.Column<long>(type: "INTEGER", nullable: false),
                    IsPublic = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileUsages_FileObjects_FileObjectId",
                        column: x => x.FileObjectId,
                        principalTable: "FileObjects",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FileUsages_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    RoleId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId1 = table.Column<long>(type: "INTEGER", nullable: false),
                    RoleId1 = table.Column<long>(type: "INTEGER", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId1",
                        column: x => x.RoleId1,
                        principalTable: "Roles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Inboxes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizationId = table.Column<long>(type: "INTEGER", nullable: false),
                    LastSuccessDeliveryDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastBeDeliveredDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MinInboxCooldownHours = table.Column<long>(type: "INTEGER", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    EmailGroupId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Domain = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Remark = table.Column<string>(type: "TEXT", nullable: true),
                    LinkCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inboxes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inboxes_EmailGroups_EmailGroupId",
                        column: x => x.EmailGroupId,
                        principalTable: "EmailGroups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FileUsageSendingGroup",
                columns: table => new
                {
                    AttachmentsId = table.Column<long>(type: "INTEGER", nullable: false),
                    SendingGroupId = table.Column<long>(type: "INTEGER", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "FileUsageSendingItem",
                columns: table => new
                {
                    AttachmentsId = table.Column<long>(type: "INTEGER", nullable: false),
                    SendingItemId = table.Column<long>(type: "INTEGER", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "SendingItemInboxes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SendingItemId = table.Column<long>(type: "INTEGER", nullable: false),
                    InboxId = table.Column<long>(type: "INTEGER", nullable: false),
                    ToEmail = table.Column<string>(type: "TEXT", nullable: true),
                    FromEmail = table.Column<string>(type: "TEXT", nullable: true),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    SendDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    OrganizationId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SendingItemInboxes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SendingItemInboxes_Inboxes_InboxId",
                        column: x => x.InboxId,
                        principalTable: "Inboxes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SendingItemInboxes_SendingItems_SendingItemId",
                        column: x => x.SendingItemId,
                        principalTable: "SendingItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailGroups_UserId",
                table: "EmailGroups",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplateSendingGroup_TemplatesId",
                table: "EmailTemplateSendingGroup",
                column: "TemplatesId");

            migrationBuilder.CreateIndex(
                name: "IX_FileObjects_FileBucketId",
                table: "FileObjects",
                column: "FileBucketId");

            migrationBuilder.CreateIndex(
                name: "IX_FileReaders_FileObjectId",
                table: "FileReaders",
                column: "FileObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_FileUsages_FileObjectId",
                table: "FileUsages",
                column: "FileObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_FileUsages_OwnerUserId",
                table: "FileUsages",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FileUsageSendingGroup_SendingGroupId",
                table: "FileUsageSendingGroup",
                column: "SendingGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_FileUsageSendingItem_SendingItemId",
                table: "FileUsageSendingItem",
                column: "SendingItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Inboxes_Email",
                table: "Inboxes",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inboxes_EmailGroupId",
                table: "Inboxes",
                column: "EmailGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Outboxes_Email",
                table: "Outboxes",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxSendingGroup_SendingGroupId",
                table: "OutboxSendingGroup",
                column: "SendingGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionCodes_RoleId",
                table: "PermissionCodes",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_SendingItemInboxes_InboxId",
                table: "SendingItemInboxes",
                column: "InboxId");

            migrationBuilder.CreateIndex(
                name: "IX_SendingItemInboxes_SendingItemId",
                table: "SendingItemInboxes",
                column: "SendingItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SendingItems_SendingGroupId",
                table: "SendingItems",
                column: "SendingGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId1",
                table: "UserRoles",
                column: "RoleId1");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId1",
                table: "UserRoles",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "EmailTemplateSendingGroup");

            migrationBuilder.DropTable(
                name: "FileReaders");

            migrationBuilder.DropTable(
                name: "FileUsageSendingGroup");

            migrationBuilder.DropTable(
                name: "FileUsageSendingItem");

            migrationBuilder.DropTable(
                name: "OutboxSendingGroup");

            migrationBuilder.DropTable(
                name: "PermissionCodes");

            migrationBuilder.DropTable(
                name: "RolePermissionCodes");

            migrationBuilder.DropTable(
                name: "SendingItemInboxes");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropTable(
                name: "UserProxies");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "UserSettings");

            migrationBuilder.DropTable(
                name: "EmailTemplates");

            migrationBuilder.DropTable(
                name: "FileUsages");

            migrationBuilder.DropTable(
                name: "Outboxes");

            migrationBuilder.DropTable(
                name: "Inboxes");

            migrationBuilder.DropTable(
                name: "SendingItems");

            migrationBuilder.DropTable(
                name: "FileObjects");

            migrationBuilder.DropTable(
                name: "EmailGroups");

            migrationBuilder.DropTable(
                name: "SendingGroups");

            migrationBuilder.DropTable(
                name: "FileBuckets");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
