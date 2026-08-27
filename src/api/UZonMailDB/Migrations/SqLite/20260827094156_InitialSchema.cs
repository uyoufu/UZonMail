using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UzonMail.DB.Migrations.SqLite
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    OrganizationId = table.Column<long>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    StringValue = table.Column<string>(type: "TEXT", nullable: true),
                    BoolValue = table.Column<bool>(type: "INTEGER", nullable: false),
                    IntValue = table.Column<int>(type: "INTEGER", nullable: false),
                    LongValue = table.Column<long>(type: "INTEGER", nullable: false),
                    DateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Json = table.Column<string>(type: "TEXT", nullable: true),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });

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
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailAddress",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailAddress", x => x.Id);
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
                    _id = table.Column<string>(type: "TEXT", nullable: false),
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
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileBuckets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IdAndName",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdAndName", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PermissionCodes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    IsNegative = table.Column<bool>(type: "INTEGER", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proxies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    MatchRegex = table.Column<string>(type: "TEXT", nullable: true),
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
                name: "RecipientSuppressions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    Reason = table.Column<int>(type: "INTEGER", nullable: false),
                    ReasonDetail = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: false),
                    SuppressedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReleasedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    ReleasedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReleaseReason = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    OrganizationId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipientSuppressions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Icon = table.Column<string>(type: "TEXT", nullable: true),
                    PermissionCodesCount = table.Column<int>(type: "INTEGER", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    OrganizationId = table.Column<long>(type: "INTEGER", nullable: false)
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
                    SourceType = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    Subjects = table.Column<string>(type: "TEXT", nullable: false),
                    Body = table.Column<string>(type: "TEXT", nullable: true),
                    SenderAccountGroups = table.Column<string>(type: "TEXT", nullable: true),
                    SenderAccountCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Recipients = table.Column<string>(type: "TEXT", nullable: false),
                    RecipientContactGroups = table.Column<string>(type: "TEXT", nullable: true),
                    RecipientCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CcBoxes = table.Column<string>(type: "TEXT", nullable: true),
                    BccBoxes = table.Column<string>(type: "TEXT", nullable: true),
                    Data = table.Column<string>(type: "TEXT", nullable: true),
                    IsDistributed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TotalCount = table.Column<int>(type: "INTEGER", nullable: false),
                    SuccessCount = table.Column<int>(type: "INTEGER", nullable: false),
                    SentCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    StatusReason = table.Column<string>(type: "TEXT", nullable: true),
                    ResumeAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SendStartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SendEndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastMessage = table.Column<string>(type: "TEXT", nullable: true),
                    SendingType = table.Column<int>(type: "INTEGER", nullable: false),
                    ScheduleDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProxyIds = table.Column<string>(type: "TEXT", nullable: true),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SendingGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SendingItemRecipients",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SendingItemId = table.Column<long>(type: "INTEGER", nullable: false),
                    RecipientContactId = table.Column<long>(type: "INTEGER", nullable: false),
                    RecipientEmail = table.Column<string>(type: "TEXT", nullable: true),
                    SenderEmail = table.Column<string>(type: "TEXT", nullable: true),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    SendDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    OrganizationId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SendingItemRecipients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SmtpInfos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Domain = table.Column<string>(type: "TEXT", nullable: false),
                    Host = table.Column<string>(type: "TEXT", nullable: false),
                    Port = table.Column<int>(type: "INTEGER", nullable: false),
                    ConnectionSecurity = table.Column<int>(type: "INTEGER", nullable: false),
                    EnableSSL = table.Column<bool>(type: "INTEGER", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmtpInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    OrganizationId = table.Column<long>(type: "INTEGER", nullable: false),
                    DepartmentId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", nullable: true),
                    Salt = table.Column<string>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: false),
                    Avatar = table.Column<string>(type: "TEXT", nullable: true),
                    ConnectionId = table.Column<string>(type: "TEXT", nullable: true),
                    IsSuperAdmin = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSystsemUser = table.Column<bool>(type: "INTEGER", nullable: false),
                    Weight = table.Column<int>(type: "INTEGER", nullable: false),
                    CreateBy = table.Column<long>(type: "INTEGER", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentEmailTemplate",
                columns: table => new
                {
                    EmailTemplateId = table.Column<long>(type: "INTEGER", nullable: false),
                    ShareToOrganizationsId = table.Column<long>(type: "INTEGER", nullable: false)
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
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    StorageState = table.Column<int>(type: "INTEGER", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
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
                name: "PermissionCodeRole",
                columns: table => new
                {
                    PermissionCodesId = table.Column<long>(type: "INTEGER", nullable: false),
                    RolesId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionCodeRole", x => new { x.PermissionCodesId, x.RolesId });
                    table.ForeignKey(
                        name: "FK_PermissionCodeRole_PermissionCodes_PermissionCodesId",
                        column: x => x.PermissionCodesId,
                        principalTable: "PermissionCodes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PermissionCodeRole_Roles_RolesId",
                        column: x => x.RolesId,
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
                name: "SendingItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SendingGroupId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    SenderAccountId = table.Column<long>(type: "INTEGER", nullable: false),
                    SenderEmail = table.Column<string>(type: "TEXT", nullable: true),
                    Recipients = table.Column<string>(type: "TEXT", nullable: false),
                    CC = table.Column<string>(type: "TEXT", nullable: true),
                    BCC = table.Column<string>(type: "TEXT", nullable: true),
                    RecipientEmails = table.Column<string>(type: "TEXT", nullable: true),
                    EmailTemplateId = table.Column<long>(type: "INTEGER", nullable: false),
                    Subject = table.Column<string>(type: "TEXT", nullable: true),
                    Content = table.Column<string>(type: "TEXT", nullable: true),
                    IsSendingBatch = table.Column<bool>(type: "INTEGER", nullable: false),
                    ProxyId = table.Column<long>(type: "INTEGER", nullable: false),
                    Data = table.Column<string>(type: "TEXT", nullable: true),
                    EnableEmailTracker = table.Column<bool>(type: "INTEGER", nullable: false),
                    SendDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    SendResult = table.Column<string>(type: "TEXT", nullable: true),
                    TriedCount = table.Column<int>(type: "INTEGER", nullable: false),
                    IsHardBounce = table.Column<bool>(type: "INTEGER", nullable: false),
                    InternetMessageId = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    InternetMessageIdKey = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ReceiptId = table.Column<string>(type: "TEXT", nullable: true),
                    ReadDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
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
                name: "EmailGroups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    Icon = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    ParentId = table.Column<int>(type: "INTEGER", nullable: false),
                    Order = table.Column<long>(type: "INTEGER", nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    Extra = table.Column<string>(type: "TEXT", nullable: true),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
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
                name: "EmailTemplateUser",
                columns: table => new
                {
                    EmailTemplateId = table.Column<long>(type: "INTEGER", nullable: false),
                    ShareToUsersId = table.Column<long>(type: "INTEGER", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "FileCategories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OwnerUserId = table.Column<long>(type: "INTEGER", nullable: false),
                    ParentId = table.Column<long>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Sort = table.Column<long>(type: "INTEGER", nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileCategories_FileCategories_ParentId",
                        column: x => x.ParentId,
                        principalTable: "FileCategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FileCategories_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    OrganizationId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRole_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
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
                    VisitedCount = table.Column<int>(type: "INTEGER", nullable: false),
                    FirstDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MaxVisitCount = table.Column<int>(type: "INTEGER", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
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
                name: "EmailAccounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    Domain = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Remark = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    EmailGroupId = table.Column<long>(type: "INTEGER", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    OrganizationId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailAccounts_EmailGroups_EmailGroupId",
                        column: x => x.EmailGroupId,
                        principalTable: "EmailGroups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RecipientContacts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmailGroupId = table.Column<long>(type: "INTEGER", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    Domain = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Remark = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    LastSuccessDeliveryDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastDeliveredAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MinimumCooldownHours = table.Column<long>(type: "INTEGER", nullable: false),
                    ValidationStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    ValidationFailureReason = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    OrganizationId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipientContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipientContacts_EmailGroups_EmailGroupId",
                        column: x => x.EmailGroupId,
                        principalTable: "EmailGroups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FileUsages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OwnerUserId = table.Column<long>(type: "INTEGER", nullable: false),
                    CategoryId = table.Column<long>(type: "INTEGER", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayNameKey = table.Column<string>(type: "TEXT", nullable: true),
                    FileObjectId = table.Column<long>(type: "INTEGER", nullable: false),
                    IsPublic = table.Column<bool>(type: "INTEGER", nullable: false),
                    Scope = table.Column<int>(type: "INTEGER", nullable: false),
                    ReferenceCount = table.Column<long>(type: "INTEGER", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileUsages_FileCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "FileCategories",
                        principalColumn: "Id");
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
                name: "RoleUserRoles",
                columns: table => new
                {
                    RolesId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserRolesId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleUserRoles", x => new { x.RolesId, x.UserRolesId });
                    table.ForeignKey(
                        name: "FK_RoleUserRoles_Roles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RoleUserRoles_UserRole_UserRolesId",
                        column: x => x.UserRolesId,
                        principalTable: "UserRole",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EmailAccountOAuthCredentials",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmailAccountId = table.Column<long>(type: "INTEGER", nullable: false),
                    Provider = table.Column<int>(type: "INTEGER", nullable: false),
                    ApplicationSource = table.Column<int>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ClientId = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    EncryptedClientSecret = table.Column<string>(type: "TEXT", nullable: true),
                    EncryptedAccessToken = table.Column<string>(type: "TEXT", nullable: true),
                    EncryptedRefreshToken = table.Column<string>(type: "TEXT", nullable: true),
                    AccessTokenExpiresAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuthorizedScopes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    TokenEndpoint = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    EncryptionKeyVersion = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CredentialUpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailAccountOAuthCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailAccountOAuthCredentials_EmailAccounts_EmailAccountId",
                        column: x => x.EmailAccountId,
                        principalTable: "EmailAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReceivingAccounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmailAccountId = table.Column<long>(type: "INTEGER", nullable: false),
                    Protocol = table.Column<int>(type: "INTEGER", nullable: false),
                    AuthenticationMethod = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ContentRetentionDays = table.Column<int>(type: "INTEGER", nullable: false),
                    LastSuccessfulSyncAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastSyncAttemptAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NextSyncAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastConnectedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastError = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceivingAccounts", x => x.Id);
                    table.CheckConstraint("CK_ReceivingAccounts_ProtocolAuthentication", "(\"Protocol\" = 0 AND \"AuthenticationMethod\" IN (0, 1)) OR (\"Protocol\" = 1 AND \"AuthenticationMethod\" = 1)");
                    table.ForeignKey(
                        name: "FK_ReceivingAccounts_EmailAccounts_EmailAccountId",
                        column: x => x.EmailAccountId,
                        principalTable: "EmailAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SenderAccounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmailAccountId = table.Column<long>(type: "INTEGER", nullable: false),
                    Protocol = table.Column<int>(type: "INTEGER", nullable: false),
                    AuthenticationMethod = table.Column<int>(type: "INTEGER", nullable: false),
                    ProxyId = table.Column<long>(type: "INTEGER", nullable: true),
                    MaxSendCountPerDay = table.Column<int>(type: "INTEGER", nullable: false),
                    SentTotalToday = table.Column<int>(type: "INTEGER", nullable: false),
                    SentCountDateUtc = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    ReplyToEmails = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Weight = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ValidationFailureReason = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SenderAccounts", x => x.Id);
                    table.CheckConstraint("CK_SenderAccounts_ProtocolAuthentication", "(\"Protocol\" = 0 AND \"AuthenticationMethod\" = 0) OR (\"Protocol\" = 1 AND \"AuthenticationMethod\" = 1)");
                    table.ForeignKey(
                        name: "FK_SenderAccounts_EmailAccounts_EmailAccountId",
                        column: x => x.EmailAccountId,
                        principalTable: "EmailAccounts",
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
                name: "ImapMailboxes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReceivingAccountId = table.Column<long>(type: "INTEGER", nullable: false),
                    ParentId = table.Column<long>(type: "INTEGER", nullable: true),
                    RemoteFullName = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    HierarchyDelimiter = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true),
                    SpecialUse = table.Column<int>(type: "INTEGER", nullable: false),
                    Attributes = table.Column<int>(type: "INTEGER", nullable: false),
                    IsSubscribed = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSynchronizationEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    UidValidity = table.Column<long>(type: "INTEGER", nullable: true),
                    UidNext = table.Column<long>(type: "INTEGER", nullable: true),
                    HighestModSequence = table.Column<long>(type: "INTEGER", nullable: true),
                    RemoteMessageCount = table.Column<int>(type: "INTEGER", nullable: false),
                    RemoteUnreadCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LastSuccessfulSyncAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastSyncStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    LastSyncError = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImapMailboxes", x => x.Id);
                    table.UniqueConstraint("AK_ImapMailboxes_Id_ReceivingAccountId", x => new { x.Id, x.ReceivingAccountId });
                    table.ForeignKey(
                        name: "FK_ImapMailboxes_ImapMailboxes_ParentId_ReceivingAccountId",
                        columns: x => new { x.ParentId, x.ReceivingAccountId },
                        principalTable: "ImapMailboxes",
                        principalColumns: new[] { "Id", "ReceivingAccountId" });
                    table.ForeignKey(
                        name: "FK_ImapMailboxes_ReceivingAccounts_ReceivingAccountId",
                        column: x => x.ReceivingAccountId,
                        principalTable: "ReceivingAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ImapSyncRuns",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReceivingAccountId = table.Column<long>(type: "INTEGER", nullable: false),
                    Trigger = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MailboxesAttempted = table.Column<int>(type: "INTEGER", nullable: false),
                    MessagesDiscovered = table.Column<int>(type: "INTEGER", nullable: false),
                    MessagesCreated = table.Column<int>(type: "INTEGER", nullable: false),
                    MessagesUpdated = table.Column<int>(type: "INTEGER", nullable: false),
                    MessageLocationsRemoved = table.Column<int>(type: "INTEGER", nullable: false),
                    DownloadedBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    ErrorSummary = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImapSyncRuns", x => x.Id);
                    table.UniqueConstraint("AK_ImapSyncRuns_Id_ReceivingAccountId", x => new { x.Id, x.ReceivingAccountId });
                    table.ForeignKey(
                        name: "FK_ImapSyncRuns_ReceivingAccounts_ReceivingAccountId",
                        column: x => x.ReceivingAccountId,
                        principalTable: "ReceivingAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IncomingMailMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReceivingAccountId = table.Column<long>(type: "INTEGER", nullable: false),
                    InternetMessageId = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    InternetMessageIdKey = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ContentSha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    SentAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReceivedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    BodyContentStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    AttachmentCount = table.Column<int>(type: "INTEGER", nullable: false),
                    InlineResourceCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentPrimaryClassification = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentBounceType = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentSpamScore = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: true),
                    LastAnalyzedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CurrentClassificationUpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AnalysisSummary = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingMailMessages", x => x.Id);
                    table.UniqueConstraint("AK_IncomingMailMessages_Id_ReceivingAccountId", x => new { x.Id, x.ReceivingAccountId });
                    table.ForeignKey(
                        name: "FK_IncomingMailMessages_ReceivingAccounts_ReceivingAccountId",
                        column: x => x.ReceivingAccountId,
                        principalTable: "ReceivingAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReceivingAccountImapCredentials",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReceivingAccountId = table.Column<long>(type: "INTEGER", nullable: false),
                    Host = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Port = table.Column<int>(type: "INTEGER", nullable: false),
                    ConnectionSecurity = table.Column<int>(type: "INTEGER", nullable: false),
                    LoginName = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    EncryptedPassword = table.Column<string>(type: "TEXT", nullable: true),
                    EncryptionKeyVersion = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CredentialUpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceivingAccountImapCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceivingAccountImapCredentials_ReceivingAccounts_ReceivingAccountId",
                        column: x => x.ReceivingAccountId,
                        principalTable: "ReceivingAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SenderAccountSendingGroup",
                columns: table => new
                {
                    SenderAccountsId = table.Column<long>(type: "INTEGER", nullable: false),
                    SendingGroupId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SenderAccountSendingGroup", x => new { x.SenderAccountsId, x.SendingGroupId });
                    table.ForeignKey(
                        name: "FK_SenderAccountSendingGroup_SenderAccounts_SenderAccountsId",
                        column: x => x.SenderAccountsId,
                        principalTable: "SenderAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SenderAccountSendingGroup_SendingGroups_SendingGroupId",
                        column: x => x.SendingGroupId,
                        principalTable: "SendingGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SenderAccountSmtpCredentials",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SenderAccountId = table.Column<long>(type: "INTEGER", nullable: false),
                    Host = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Port = table.Column<int>(type: "INTEGER", nullable: false),
                    ConnectionSecurity = table.Column<int>(type: "INTEGER", nullable: false),
                    LoginName = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    EncryptedPassword = table.Column<string>(type: "TEXT", nullable: false),
                    EncryptionKeyVersion = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CredentialUpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SenderAccountSmtpCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SenderAccountSmtpCredentials_SenderAccounts_SenderAccountId",
                        column: x => x.SenderAccountId,
                        principalTable: "SenderAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ImapMailboxSyncCheckpoints",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ImapMailboxId = table.Column<long>(type: "INTEGER", nullable: false),
                    UidValidity = table.Column<long>(type: "INTEGER", nullable: false),
                    LastCommittedUid = table.Column<long>(type: "INTEGER", nullable: true),
                    LastCommittedModSequence = table.Column<long>(type: "INTEGER", nullable: true),
                    SynchronizationGeneration = table.Column<long>(type: "INTEGER", nullable: false),
                    LastFullReconciliationAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastCommittedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImapMailboxSyncCheckpoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImapMailboxSyncCheckpoints_ImapMailboxes_ImapMailboxId",
                        column: x => x.ImapMailboxId,
                        principalTable: "ImapMailboxes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ImapMailboxSyncRuns",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReceivingAccountId = table.Column<long>(type: "INTEGER", nullable: false),
                    ImapSyncRunId = table.Column<long>(type: "INTEGER", nullable: false),
                    ImapMailboxId = table.Column<long>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    StartUid = table.Column<long>(type: "INTEGER", nullable: true),
                    EndUid = table.Column<long>(type: "INTEGER", nullable: true),
                    StartCommittedModSequence = table.Column<long>(type: "INTEGER", nullable: true),
                    EndCommittedModSequence = table.Column<long>(type: "INTEGER", nullable: true),
                    CheckpointAdvanced = table.Column<bool>(type: "INTEGER", nullable: false),
                    MessagesDiscovered = table.Column<int>(type: "INTEGER", nullable: false),
                    LocationsCreated = table.Column<int>(type: "INTEGER", nullable: false),
                    LocationsUpdated = table.Column<int>(type: "INTEGER", nullable: false),
                    DownloadedBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    ErrorSummary = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImapMailboxSyncRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImapMailboxSyncRuns_ImapMailboxes_ImapMailboxId_ReceivingAccountId",
                        columns: x => new { x.ImapMailboxId, x.ReceivingAccountId },
                        principalTable: "ImapMailboxes",
                        principalColumns: new[] { "Id", "ReceivingAccountId" });
                    table.ForeignKey(
                        name: "FK_ImapMailboxSyncRuns_ImapSyncRuns_ImapSyncRunId_ReceivingAccountId",
                        columns: x => new { x.ImapSyncRunId, x.ReceivingAccountId },
                        principalTable: "ImapSyncRuns",
                        principalColumns: new[] { "Id", "ReceivingAccountId" });
                });

            migrationBuilder.CreateTable(
                name: "IncomingMailAddresses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IncomingMailMessageId = table.Column<long>(type: "INTEGER", nullable: false),
                    AddressType = table.Column<int>(type: "INTEGER", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingMailAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomingMailAddresses_IncomingMailMessages_IncomingMailMessageId",
                        column: x => x.IncomingMailMessageId,
                        principalTable: "IncomingMailMessages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IncomingMailAnalyses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IncomingMailMessageId = table.Column<long>(type: "INTEGER", nullable: false),
                    Source = table.Column<int>(type: "INTEGER", nullable: false),
                    AnalyzerVersion = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    BounceType = table.Column<int>(type: "INTEGER", nullable: false),
                    SpamScore = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: true),
                    ResultSummary = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    FailureReason = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    StartedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    InputContentSha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingMailAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomingMailAnalyses_IncomingMailMessages_IncomingMailMessageId",
                        column: x => x.IncomingMailMessageId,
                        principalTable: "IncomingMailMessages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IncomingMailLocations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReceivingAccountId = table.Column<long>(type: "INTEGER", nullable: false),
                    IncomingMailMessageId = table.Column<long>(type: "INTEGER", nullable: false),
                    ImapMailboxId = table.Column<long>(type: "INTEGER", nullable: false),
                    UidValidity = table.Column<long>(type: "INTEGER", nullable: false),
                    Uid = table.Column<long>(type: "INTEGER", nullable: false),
                    ModSequence = table.Column<long>(type: "INTEGER", nullable: true),
                    Flags = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPresentOnServer = table.Column<bool>(type: "INTEGER", nullable: false),
                    FirstSeenAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastSynchronizedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingMailLocations", x => x.Id);
                    table.UniqueConstraint("AK_IncomingMailLocations_Id_ReceivingAccountId", x => new { x.Id, x.ReceivingAccountId });
                    table.ForeignKey(
                        name: "FK_IncomingMailLocations_ImapMailboxes_ImapMailboxId_ReceivingAccountId",
                        columns: x => new { x.ImapMailboxId, x.ReceivingAccountId },
                        principalTable: "ImapMailboxes",
                        principalColumns: new[] { "Id", "ReceivingAccountId" });
                    table.ForeignKey(
                        name: "FK_IncomingMailLocations_IncomingMailMessages_IncomingMailMessageId_ReceivingAccountId",
                        columns: x => new { x.IncomingMailMessageId, x.ReceivingAccountId },
                        principalTable: "IncomingMailMessages",
                        principalColumns: new[] { "Id", "ReceivingAccountId" });
                });

            migrationBuilder.CreateTable(
                name: "IncomingMailMimeParts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IncomingMailMessageId = table.Column<long>(type: "INTEGER", nullable: false),
                    MimePartPath = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ParentMimePartPath = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    PartKind = table.Column<int>(type: "INTEGER", nullable: false),
                    ContentDisposition = table.Column<int>(type: "INTEGER", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    MediaType = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ContentId = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    DeclaredSize = table.Column<long>(type: "INTEGER", nullable: true),
                    FetchStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    FileUsageId = table.Column<long>(type: "INTEGER", nullable: true),
                    ContentSha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    DownloadedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExpiresAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastFetchError = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingMailMimeParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomingMailMimeParts_FileUsages_FileUsageId",
                        column: x => x.FileUsageId,
                        principalTable: "FileUsages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IncomingMailMimeParts_IncomingMailMessages_IncomingMailMessageId",
                        column: x => x.IncomingMailMessageId,
                        principalTable: "IncomingMailMessages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IncomingMailReferences",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IncomingMailMessageId = table.Column<long>(type: "INTEGER", nullable: false),
                    ReferenceType = table.Column<int>(type: "INTEGER", nullable: false),
                    InternetMessageId = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingMailReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomingMailReferences_IncomingMailMessages_IncomingMailMessageId",
                        column: x => x.IncomingMailMessageId,
                        principalTable: "IncomingMailMessages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IncomingMailSendingItemLinks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IncomingMailMessageId = table.Column<long>(type: "INTEGER", nullable: false),
                    SendingItemId = table.Column<long>(type: "INTEGER", nullable: false),
                    SendingItemRecipientId = table.Column<long>(type: "INTEGER", nullable: true),
                    LinkType = table.Column<int>(type: "INTEGER", nullable: false),
                    MatchMethod = table.Column<int>(type: "INTEGER", nullable: false),
                    Confidence = table.Column<decimal>(type: "TEXT", precision: 5, scale: 4, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ReviewedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    ReviewedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MatchReason = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingMailSendingItemLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomingMailSendingItemLinks_IncomingMailMessages_IncomingMailMessageId",
                        column: x => x.IncomingMailMessageId,
                        principalTable: "IncomingMailMessages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IncomingMailSendingItemLinks_SendingItemRecipients_SendingItemRecipientId",
                        column: x => x.SendingItemRecipientId,
                        principalTable: "SendingItemRecipients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IncomingMailSendingItemLinks_SendingItems_SendingItemId",
                        column: x => x.SendingItemId,
                        principalTable: "SendingItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IncomingMailAnalysisClassifications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IncomingMailAnalysisId = table.Column<long>(type: "INTEGER", nullable: false),
                    Classification = table.Column<int>(type: "INTEGER", nullable: false),
                    Confidence = table.Column<decimal>(type: "TEXT", precision: 5, scale: 4, nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingMailAnalysisClassifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomingMailAnalysisClassifications_IncomingMailAnalyses_IncomingMailAnalysisId",
                        column: x => x.IncomingMailAnalysisId,
                        principalTable: "IncomingMailAnalyses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IncomingMailClassificationEvidences",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IncomingMailAnalysisId = table.Column<long>(type: "INTEGER", nullable: false),
                    EvidenceType = table.Column<int>(type: "INTEGER", nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Summary = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    ValueSha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingMailClassificationEvidences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomingMailClassificationEvidences_IncomingMailAnalyses_IncomingMailAnalysisId",
                        column: x => x.IncomingMailAnalysisId,
                        principalTable: "IncomingMailAnalyses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IncomingMailCurrentClassifications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IncomingMailMessageId = table.Column<long>(type: "INTEGER", nullable: false),
                    IncomingMailAnalysisId = table.Column<long>(type: "INTEGER", nullable: false),
                    Classification = table.Column<int>(type: "INTEGER", nullable: false),
                    Confidence = table.Column<decimal>(type: "TEXT", precision: 5, scale: 4, nullable: false),
                    AppliedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingMailCurrentClassifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomingMailCurrentClassifications_IncomingMailAnalyses_IncomingMailAnalysisId",
                        column: x => x.IncomingMailAnalysisId,
                        principalTable: "IncomingMailAnalyses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IncomingMailCurrentClassifications_IncomingMailMessages_IncomingMailMessageId",
                        column: x => x.IncomingMailMessageId,
                        principalTable: "IncomingMailMessages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IncomingMailDeliveryStatuses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IncomingMailAnalysisId = table.Column<long>(type: "INTEGER", nullable: false),
                    MimePartPath = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    Action = table.Column<int>(type: "INTEGER", nullable: false),
                    OriginalRecipient = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    FinalRecipient = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    FinalRecipientEmail = table.Column<string>(type: "TEXT", maxLength: 320, nullable: true),
                    EnhancedStatusCode = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    DiagnosticCode = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    RemoteMta = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingMailDeliveryStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomingMailDeliveryStatuses_IncomingMailAnalyses_IncomingMailAnalysisId",
                        column: x => x.IncomingMailAnalysisId,
                        principalTable: "IncomingMailAnalyses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IncomingMailFeedbackReports",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IncomingMailAnalysisId = table.Column<long>(type: "INTEGER", nullable: false),
                    MimePartPath = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    FeedbackType = table.Column<int>(type: "INTEGER", nullable: false),
                    OriginalRecipient = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    OriginalRecipientEmail = table.Column<string>(type: "TEXT", maxLength: 320, nullable: true),
                    ReportedDomain = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    UserAgent = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingMailFeedbackReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomingMailFeedbackReports_IncomingMailAnalyses_IncomingMailAnalysisId",
                        column: x => x.IncomingMailAnalysisId,
                        principalTable: "IncomingMailAnalyses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ImapSyncCommands",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReceivingAccountId = table.Column<long>(type: "INTEGER", nullable: false),
                    ImapMailboxId = table.Column<long>(type: "INTEGER", nullable: false),
                    IncomingMailLocationId = table.Column<long>(type: "INTEGER", nullable: true),
                    CommandType = table.Column<int>(type: "INTEGER", nullable: false),
                    FlagMutationMode = table.Column<int>(type: "INTEGER", nullable: true),
                    RequestedFlags = table.Column<int>(type: "INTEGER", nullable: false),
                    Keyword = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DestinationMailboxId = table.Column<long>(type: "INTEGER", nullable: true),
                    ExpectedUidValidity = table.Column<long>(type: "INTEGER", nullable: true),
                    ExpectedUid = table.Column<long>(type: "INTEGER", nullable: true),
                    ExpectedModSequence = table.Column<long>(type: "INTEGER", nullable: true),
                    IdempotencyKey = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    AttemptCount = table.Column<int>(type: "INTEGER", nullable: false),
                    NextAttemptAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastError = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImapSyncCommands", x => x.Id);
                    table.UniqueConstraint("AK_ImapSyncCommands_Id_ReceivingAccountId", x => new { x.Id, x.ReceivingAccountId });
                    table.ForeignKey(
                        name: "FK_ImapSyncCommands_ImapMailboxes_DestinationMailboxId_ReceivingAccountId",
                        columns: x => new { x.DestinationMailboxId, x.ReceivingAccountId },
                        principalTable: "ImapMailboxes",
                        principalColumns: new[] { "Id", "ReceivingAccountId" });
                    table.ForeignKey(
                        name: "FK_ImapSyncCommands_ImapMailboxes_ImapMailboxId_ReceivingAccountId",
                        columns: x => new { x.ImapMailboxId, x.ReceivingAccountId },
                        principalTable: "ImapMailboxes",
                        principalColumns: new[] { "Id", "ReceivingAccountId" });
                    table.ForeignKey(
                        name: "FK_ImapSyncCommands_IncomingMailLocations_IncomingMailLocationId_ReceivingAccountId",
                        columns: x => new { x.IncomingMailLocationId, x.ReceivingAccountId },
                        principalTable: "IncomingMailLocations",
                        principalColumns: new[] { "Id", "ReceivingAccountId" });
                    table.ForeignKey(
                        name: "FK_ImapSyncCommands_ReceivingAccounts_ReceivingAccountId",
                        column: x => x.ReceivingAccountId,
                        principalTable: "ReceivingAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IncomingMailKeywords",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IncomingMailLocationId = table.Column<long>(type: "INTEGER", nullable: false),
                    Keyword = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingMailKeywords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomingMailKeywords_IncomingMailLocations_IncomingMailLocationId",
                        column: x => x.IncomingMailLocationId,
                        principalTable: "IncomingMailLocations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IncomingMailAuditEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReceivingAccountId = table.Column<long>(type: "INTEGER", nullable: false),
                    IncomingMailMessageId = table.Column<long>(type: "INTEGER", nullable: true),
                    IncomingMailLocationId = table.Column<long>(type: "INTEGER", nullable: true),
                    ImapSyncRunId = table.Column<long>(type: "INTEGER", nullable: true),
                    ImapSyncCommandId = table.Column<long>(type: "INTEGER", nullable: true),
                    ActorType = table.Column<int>(type: "INTEGER", nullable: false),
                    ActorUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    EventType = table.Column<int>(type: "INTEGER", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingMailAuditEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomingMailAuditEvents_ImapSyncCommands_ImapSyncCommandId_ReceivingAccountId",
                        columns: x => new { x.ImapSyncCommandId, x.ReceivingAccountId },
                        principalTable: "ImapSyncCommands",
                        principalColumns: new[] { "Id", "ReceivingAccountId" });
                    table.ForeignKey(
                        name: "FK_IncomingMailAuditEvents_ImapSyncRuns_ImapSyncRunId_ReceivingAccountId",
                        columns: x => new { x.ImapSyncRunId, x.ReceivingAccountId },
                        principalTable: "ImapSyncRuns",
                        principalColumns: new[] { "Id", "ReceivingAccountId" });
                    table.ForeignKey(
                        name: "FK_IncomingMailAuditEvents_IncomingMailLocations_IncomingMailLocationId_ReceivingAccountId",
                        columns: x => new { x.IncomingMailLocationId, x.ReceivingAccountId },
                        principalTable: "IncomingMailLocations",
                        principalColumns: new[] { "Id", "ReceivingAccountId" });
                    table.ForeignKey(
                        name: "FK_IncomingMailAuditEvents_IncomingMailMessages_IncomingMailMessageId_ReceivingAccountId",
                        columns: x => new { x.IncomingMailMessageId, x.ReceivingAccountId },
                        principalTable: "IncomingMailMessages",
                        principalColumns: new[] { "Id", "ReceivingAccountId" });
                    table.ForeignKey(
                        name: "FK_IncomingMailAuditEvents_ReceivingAccounts_ReceivingAccountId",
                        column: x => x.ReceivingAccountId,
                        principalTable: "ReceivingAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentEmailTemplate_ShareToOrganizationsId",
                table: "DepartmentEmailTemplate",
                column: "ShareToOrganizationsId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailAccountOAuthCredentials_EmailAccountId",
                table: "EmailAccountOAuthCredentials",
                column: "EmailAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailAccounts_EmailGroupId_Id",
                table: "EmailAccounts",
                columns: new[] { "EmailGroupId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailAccounts_OrganizationId_Domain",
                table: "EmailAccounts",
                columns: new[] { "OrganizationId", "Domain" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailAccounts_UserId_NormalizedEmail",
                table: "EmailAccounts",
                columns: new[] { "UserId", "NormalizedEmail" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailGroups_UserId",
                table: "EmailGroups",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplateSendingGroup_TemplatesId",
                table: "EmailTemplateSendingGroup",
                column: "TemplatesId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplateUser_ShareToUsersId",
                table: "EmailTemplateUser",
                column: "ShareToUsersId");

            migrationBuilder.CreateIndex(
                name: "IX_FileCategories_OwnerUserId_ParentId_Sort",
                table: "FileCategories",
                columns: new[] { "OwnerUserId", "ParentId", "Sort" });

            migrationBuilder.CreateIndex(
                name: "IX_FileCategories_ParentId",
                table: "FileCategories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_FileObjects_FileBucketId",
                table: "FileObjects",
                column: "FileBucketId");

            migrationBuilder.CreateIndex(
                name: "IX_FileObjects_Sha256",
                table: "FileObjects",
                column: "Sha256",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileReaders_FileObjectId",
                table: "FileReaders",
                column: "FileObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_FileUsages_CategoryId",
                table: "FileUsages",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FileUsages_FileObjectId",
                table: "FileUsages",
                column: "FileObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_FileUsages_OwnerUserId_CategoryId_CreateDate",
                table: "FileUsages",
                columns: new[] { "OwnerUserId", "CategoryId", "CreateDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FileUsages_OwnerUserId_DisplayNameKey",
                table: "FileUsages",
                columns: new[] { "OwnerUserId", "DisplayNameKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileUsages_OwnerUserId_FileObjectId",
                table: "FileUsages",
                columns: new[] { "OwnerUserId", "FileObjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileUsages_OwnerUserId_Scope_CreateDate",
                table: "FileUsages",
                columns: new[] { "OwnerUserId", "Scope", "CreateDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FileUsageSendingGroup_SendingGroupId",
                table: "FileUsageSendingGroup",
                column: "SendingGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_FileUsageSendingItem_SendingItemId",
                table: "FileUsageSendingItem",
                column: "SendingItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ImapMailboxes_ParentId_ReceivingAccountId",
                table: "ImapMailboxes",
                columns: new[] { "ParentId", "ReceivingAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_ImapMailboxes_ReceivingAccountId_IsSynchronizationEnabled",
                table: "ImapMailboxes",
                columns: new[] { "ReceivingAccountId", "IsSynchronizationEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_ImapMailboxes_ReceivingAccountId_RemoteFullName",
                table: "ImapMailboxes",
                columns: new[] { "ReceivingAccountId", "RemoteFullName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImapMailboxSyncCheckpoints_ImapMailboxId",
                table: "ImapMailboxSyncCheckpoints",
                column: "ImapMailboxId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImapMailboxSyncRuns_ImapMailboxId_ReceivingAccountId",
                table: "ImapMailboxSyncRuns",
                columns: new[] { "ImapMailboxId", "ReceivingAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_ImapMailboxSyncRuns_ImapSyncRunId_ImapMailboxId",
                table: "ImapMailboxSyncRuns",
                columns: new[] { "ImapSyncRunId", "ImapMailboxId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImapMailboxSyncRuns_ImapSyncRunId_ReceivingAccountId",
                table: "ImapMailboxSyncRuns",
                columns: new[] { "ImapSyncRunId", "ReceivingAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_ImapSyncCommands_DestinationMailboxId_ReceivingAccountId",
                table: "ImapSyncCommands",
                columns: new[] { "DestinationMailboxId", "ReceivingAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_ImapSyncCommands_ImapMailboxId_IncomingMailLocationId",
                table: "ImapSyncCommands",
                columns: new[] { "ImapMailboxId", "IncomingMailLocationId" });

            migrationBuilder.CreateIndex(
                name: "IX_ImapSyncCommands_ImapMailboxId_ReceivingAccountId",
                table: "ImapSyncCommands",
                columns: new[] { "ImapMailboxId", "ReceivingAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_ImapSyncCommands_IncomingMailLocationId_ReceivingAccountId",
                table: "ImapSyncCommands",
                columns: new[] { "IncomingMailLocationId", "ReceivingAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_ImapSyncCommands_ReceivingAccountId_IdempotencyKey",
                table: "ImapSyncCommands",
                columns: new[] { "ReceivingAccountId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImapSyncCommands_Status_NextAttemptAtUtc",
                table: "ImapSyncCommands",
                columns: new[] { "Status", "NextAttemptAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ImapSyncRuns_ReceivingAccountId_StartedAtUtc",
                table: "ImapSyncRuns",
                columns: new[] { "ReceivingAccountId", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailAddresses_Email",
                table: "IncomingMailAddresses",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailAddresses_IncomingMailMessageId_AddressType_Position",
                table: "IncomingMailAddresses",
                columns: new[] { "IncomingMailMessageId", "AddressType", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailAnalyses_IncomingMailMessageId_StartedAtUtc",
                table: "IncomingMailAnalyses",
                columns: new[] { "IncomingMailMessageId", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailAnalyses_Status_StartedAtUtc",
                table: "IncomingMailAnalyses",
                columns: new[] { "Status", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailAnalysisClassifications_IncomingMailAnalysisId_Classification",
                table: "IncomingMailAnalysisClassifications",
                columns: new[] { "IncomingMailAnalysisId", "Classification" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailAuditEvents_ImapSyncCommandId_ReceivingAccountId",
                table: "IncomingMailAuditEvents",
                columns: new[] { "ImapSyncCommandId", "ReceivingAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailAuditEvents_ImapSyncRunId_ReceivingAccountId",
                table: "IncomingMailAuditEvents",
                columns: new[] { "ImapSyncRunId", "ReceivingAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailAuditEvents_IncomingMailLocationId_ReceivingAccountId",
                table: "IncomingMailAuditEvents",
                columns: new[] { "IncomingMailLocationId", "ReceivingAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailAuditEvents_IncomingMailMessageId_OccurredAtUtc",
                table: "IncomingMailAuditEvents",
                columns: new[] { "IncomingMailMessageId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailAuditEvents_IncomingMailMessageId_ReceivingAccountId",
                table: "IncomingMailAuditEvents",
                columns: new[] { "IncomingMailMessageId", "ReceivingAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailAuditEvents_ReceivingAccountId_OccurredAtUtc",
                table: "IncomingMailAuditEvents",
                columns: new[] { "ReceivingAccountId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailClassificationEvidences_IncomingMailAnalysisId_EvidenceType",
                table: "IncomingMailClassificationEvidences",
                columns: new[] { "IncomingMailAnalysisId", "EvidenceType" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailCurrentClassifications_Classification_AppliedAtUtc",
                table: "IncomingMailCurrentClassifications",
                columns: new[] { "Classification", "AppliedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailCurrentClassifications_IncomingMailAnalysisId",
                table: "IncomingMailCurrentClassifications",
                column: "IncomingMailAnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailCurrentClassifications_IncomingMailMessageId_Classification",
                table: "IncomingMailCurrentClassifications",
                columns: new[] { "IncomingMailMessageId", "Classification" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailDeliveryStatuses_FinalRecipientEmail_Action",
                table: "IncomingMailDeliveryStatuses",
                columns: new[] { "FinalRecipientEmail", "Action" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailDeliveryStatuses_IncomingMailAnalysisId_MimePartPath_Position",
                table: "IncomingMailDeliveryStatuses",
                columns: new[] { "IncomingMailAnalysisId", "MimePartPath", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailFeedbackReports_IncomingMailAnalysisId",
                table: "IncomingMailFeedbackReports",
                column: "IncomingMailAnalysisId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailFeedbackReports_OriginalRecipientEmail_FeedbackType",
                table: "IncomingMailFeedbackReports",
                columns: new[] { "OriginalRecipientEmail", "FeedbackType" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailKeywords_IncomingMailLocationId_Keyword",
                table: "IncomingMailKeywords",
                columns: new[] { "IncomingMailLocationId", "Keyword" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailLocations_ImapMailboxId_IsPresentOnServer_Uid",
                table: "IncomingMailLocations",
                columns: new[] { "ImapMailboxId", "IsPresentOnServer", "Uid" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailLocations_ImapMailboxId_ModSequence",
                table: "IncomingMailLocations",
                columns: new[] { "ImapMailboxId", "ModSequence" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailLocations_ImapMailboxId_ReceivingAccountId",
                table: "IncomingMailLocations",
                columns: new[] { "ImapMailboxId", "ReceivingAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailLocations_ImapMailboxId_UidValidity_Uid",
                table: "IncomingMailLocations",
                columns: new[] { "ImapMailboxId", "UidValidity", "Uid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailLocations_IncomingMailMessageId_ReceivingAccountId",
                table: "IncomingMailLocations",
                columns: new[] { "IncomingMailMessageId", "ReceivingAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailMessages_ReceivingAccountId_BodyContentStatus_ReceivedAtUtc",
                table: "IncomingMailMessages",
                columns: new[] { "ReceivingAccountId", "BodyContentStatus", "ReceivedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailMessages_ReceivingAccountId_ContentSha256",
                table: "IncomingMailMessages",
                columns: new[] { "ReceivingAccountId", "ContentSha256" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailMessages_ReceivingAccountId_CurrentBounceType_ReceivedAtUtc",
                table: "IncomingMailMessages",
                columns: new[] { "ReceivingAccountId", "CurrentBounceType", "ReceivedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailMessages_ReceivingAccountId_CurrentPrimaryClassification_ReceivedAtUtc",
                table: "IncomingMailMessages",
                columns: new[] { "ReceivingAccountId", "CurrentPrimaryClassification", "ReceivedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailMessages_ReceivingAccountId_CurrentSpamScore",
                table: "IncomingMailMessages",
                columns: new[] { "ReceivingAccountId", "CurrentSpamScore" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailMessages_ReceivingAccountId_InternetMessageIdKey",
                table: "IncomingMailMessages",
                columns: new[] { "ReceivingAccountId", "InternetMessageIdKey" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailMessages_ReceivingAccountId_ReceivedAtUtc",
                table: "IncomingMailMessages",
                columns: new[] { "ReceivingAccountId", "ReceivedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailMimeParts_FetchStatus_ExpiresAtUtc",
                table: "IncomingMailMimeParts",
                columns: new[] { "FetchStatus", "ExpiresAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailMimeParts_FileUsageId",
                table: "IncomingMailMimeParts",
                column: "FileUsageId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailMimeParts_IncomingMailMessageId_MimePartPath",
                table: "IncomingMailMimeParts",
                columns: new[] { "IncomingMailMessageId", "MimePartPath" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailReferences_IncomingMailMessageId_ReferenceType_Position",
                table: "IncomingMailReferences",
                columns: new[] { "IncomingMailMessageId", "ReferenceType", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailReferences_InternetMessageId",
                table: "IncomingMailReferences",
                column: "InternetMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailSendingItemLinks_IncomingMailMessageId_SendingItemId_SendingItemRecipientId_LinkType",
                table: "IncomingMailSendingItemLinks",
                columns: new[] { "IncomingMailMessageId", "SendingItemId", "SendingItemRecipientId", "LinkType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailSendingItemLinks_SendingItemId_LinkType_Status",
                table: "IncomingMailSendingItemLinks",
                columns: new[] { "SendingItemId", "LinkType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailSendingItemLinks_SendingItemRecipientId",
                table: "IncomingMailSendingItemLinks",
                column: "SendingItemRecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionCodeRole_RolesId",
                table: "PermissionCodeRole",
                column: "RolesId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionCodes_Code",
                table: "PermissionCodes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceivingAccountImapCredentials_ReceivingAccountId",
                table: "ReceivingAccountImapCredentials",
                column: "ReceivingAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceivingAccounts_EmailAccountId",
                table: "ReceivingAccounts",
                column: "EmailAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceivingAccounts_Status_NextSyncAtUtc",
                table: "ReceivingAccounts",
                columns: new[] { "Status", "NextSyncAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_RecipientContacts_EmailGroupId_ValidationStatus_Id",
                table: "RecipientContacts",
                columns: new[] { "EmailGroupId", "ValidationStatus", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_RecipientContacts_UserId_NormalizedEmail",
                table: "RecipientContacts",
                columns: new[] { "UserId", "NormalizedEmail" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecipientSuppressions_OrganizationId_Email",
                table: "RecipientSuppressions",
                columns: new[] { "OrganizationId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecipientSuppressions_OrganizationId_IsActive_Email",
                table: "RecipientSuppressions",
                columns: new[] { "OrganizationId", "IsActive", "Email" });

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleUserRoles_UserRolesId",
                table: "RoleUserRoles",
                column: "UserRolesId");

            migrationBuilder.CreateIndex(
                name: "IX_SenderAccounts_EmailAccountId",
                table: "SenderAccounts",
                column: "EmailAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SenderAccounts_Status_Id",
                table: "SenderAccounts",
                columns: new[] { "Status", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_SenderAccountSendingGroup_SendingGroupId",
                table: "SenderAccountSendingGroup",
                column: "SendingGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SenderAccountSmtpCredentials_SenderAccountId",
                table: "SenderAccountSmtpCredentials",
                column: "SenderAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SendingGroups_Status_ResumeAtUtc_Id",
                table: "SendingGroups",
                columns: new[] { "Status", "ResumeAtUtc", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_SendingItems_Group_Status_SenderAccount_Id",
                table: "SendingItems",
                columns: new[] { "SendingGroupId", "Status", "SenderAccountId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_SendingItems_InternetMessageIdKey",
                table: "SendingItems",
                column: "InternetMessageIdKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_UserId",
                table: "UserRole",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "DepartmentEmailTemplate");

            migrationBuilder.DropTable(
                name: "EmailAccountOAuthCredentials");

            migrationBuilder.DropTable(
                name: "EmailAddress");

            migrationBuilder.DropTable(
                name: "EmailTemplateSendingGroup");

            migrationBuilder.DropTable(
                name: "EmailTemplateUser");

            migrationBuilder.DropTable(
                name: "FileReaders");

            migrationBuilder.DropTable(
                name: "FileUsageSendingGroup");

            migrationBuilder.DropTable(
                name: "FileUsageSendingItem");

            migrationBuilder.DropTable(
                name: "IdAndName");

            migrationBuilder.DropTable(
                name: "ImapMailboxSyncCheckpoints");

            migrationBuilder.DropTable(
                name: "ImapMailboxSyncRuns");

            migrationBuilder.DropTable(
                name: "IncomingMailAddresses");

            migrationBuilder.DropTable(
                name: "IncomingMailAnalysisClassifications");

            migrationBuilder.DropTable(
                name: "IncomingMailAuditEvents");

            migrationBuilder.DropTable(
                name: "IncomingMailClassificationEvidences");

            migrationBuilder.DropTable(
                name: "IncomingMailCurrentClassifications");

            migrationBuilder.DropTable(
                name: "IncomingMailDeliveryStatuses");

            migrationBuilder.DropTable(
                name: "IncomingMailFeedbackReports");

            migrationBuilder.DropTable(
                name: "IncomingMailKeywords");

            migrationBuilder.DropTable(
                name: "IncomingMailMimeParts");

            migrationBuilder.DropTable(
                name: "IncomingMailReferences");

            migrationBuilder.DropTable(
                name: "IncomingMailSendingItemLinks");

            migrationBuilder.DropTable(
                name: "PermissionCodeRole");

            migrationBuilder.DropTable(
                name: "Proxies");

            migrationBuilder.DropTable(
                name: "ReceivingAccountImapCredentials");

            migrationBuilder.DropTable(
                name: "RecipientContacts");

            migrationBuilder.DropTable(
                name: "RecipientSuppressions");

            migrationBuilder.DropTable(
                name: "RoleUserRoles");

            migrationBuilder.DropTable(
                name: "SenderAccountSendingGroup");

            migrationBuilder.DropTable(
                name: "SenderAccountSmtpCredentials");

            migrationBuilder.DropTable(
                name: "SmtpInfos");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "EmailTemplates");

            migrationBuilder.DropTable(
                name: "ImapSyncCommands");

            migrationBuilder.DropTable(
                name: "ImapSyncRuns");

            migrationBuilder.DropTable(
                name: "IncomingMailAnalyses");

            migrationBuilder.DropTable(
                name: "FileUsages");

            migrationBuilder.DropTable(
                name: "SendingItemRecipients");

            migrationBuilder.DropTable(
                name: "SendingItems");

            migrationBuilder.DropTable(
                name: "PermissionCodes");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "SenderAccounts");

            migrationBuilder.DropTable(
                name: "IncomingMailLocations");

            migrationBuilder.DropTable(
                name: "FileCategories");

            migrationBuilder.DropTable(
                name: "FileObjects");

            migrationBuilder.DropTable(
                name: "SendingGroups");

            migrationBuilder.DropTable(
                name: "ImapMailboxes");

            migrationBuilder.DropTable(
                name: "IncomingMailMessages");

            migrationBuilder.DropTable(
                name: "FileBuckets");

            migrationBuilder.DropTable(
                name: "ReceivingAccounts");

            migrationBuilder.DropTable(
                name: "EmailAccounts");

            migrationBuilder.DropTable(
                name: "EmailGroups");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
