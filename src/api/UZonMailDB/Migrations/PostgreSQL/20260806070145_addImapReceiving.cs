using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UzonMail.DB.Migrations.PostgreSQL
{
    /// <inheritdoc />
    public partial class addImapReceiving : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InternetMessageId",
                table: "SendingItems",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InternetMessageIdKey",
                table: "SendingItems",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Scope",
                table: "FileUsages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ImapAccounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    Host = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    ConnectionSecurity = table.Column<int>(type: "integer", nullable: false),
                    AuthenticationType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ContentRetentionDays = table.Column<int>(type: "integer", nullable: false),
                    LastSuccessfulSyncAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSyncAttemptAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextSyncAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastConnectedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false),
                    OrganizationId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImapAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImapAccountCredentials",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ImapAccountId = table.Column<long>(type: "bigint", nullable: false),
                    LoginName = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    EncryptedPassword = table.Column<string>(type: "text", nullable: true),
                    OAuthProvider = table.Column<int>(type: "integer", nullable: false),
                    EncryptedOAuthAccessToken = table.Column<string>(type: "text", nullable: true),
                    EncryptedOAuthRefreshToken = table.Column<string>(type: "text", nullable: true),
                    OAuthAccessTokenExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OAuthClientId = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EncryptedOAuthClientSecret = table.Column<string>(type: "text", nullable: true),
                    EncryptionKeyVersion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CredentialUpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OAuthTenantId = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OAuthTokenEndpoint = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImapAccountCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImapAccountCredentials_ImapAccounts_ImapAccountId",
                        column: x => x.ImapAccountId,
                        principalTable: "ImapAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ImapAccountOutboxLinks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ImapAccountId = table.Column<long>(type: "bigint", nullable: false),
                    OutboxId = table.Column<long>(type: "bigint", nullable: false),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImapAccountOutboxLinks", x => x.Id);
                    table.UniqueConstraint("AK_ImapAccountOutboxLinks_Id_ImapAccountId", x => new { x.Id, x.ImapAccountId });
                    table.ForeignKey(
                        name: "FK_ImapAccountOutboxLinks_ImapAccounts_ImapAccountId",
                        column: x => x.ImapAccountId,
                        principalTable: "ImapAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ImapAccountOutboxLinks_Outboxes_OutboxId",
                        column: x => x.OutboxId,
                        principalTable: "Outboxes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ImapMailboxes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ImapAccountId = table.Column<long>(type: "bigint", nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    RemoteFullName = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    HierarchyDelimiter = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    SpecialUse = table.Column<int>(type: "integer", nullable: false),
                    Attributes = table.Column<int>(type: "integer", nullable: false),
                    IsSubscribed = table.Column<bool>(type: "boolean", nullable: false),
                    IsSynchronizationEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    UidValidity = table.Column<long>(type: "bigint", nullable: true),
                    UidNext = table.Column<long>(type: "bigint", nullable: true),
                    HighestModSequence = table.Column<long>(type: "bigint", nullable: true),
                    RemoteMessageCount = table.Column<int>(type: "integer", nullable: false),
                    RemoteUnreadCount = table.Column<int>(type: "integer", nullable: false),
                    LastSuccessfulSyncAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSyncStatus = table.Column<int>(type: "integer", nullable: false),
                    LastSyncError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImapMailboxes", x => x.Id);
                    table.UniqueConstraint("AK_ImapMailboxes_Id_ImapAccountId", x => new { x.Id, x.ImapAccountId });
                    table.ForeignKey(
                        name: "FK_ImapMailboxes_ImapAccounts_ImapAccountId",
                        column: x => x.ImapAccountId,
                        principalTable: "ImapAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ImapMailboxes_ImapMailboxes_ParentId_ImapAccountId",
                        columns: x => new { x.ParentId, x.ImapAccountId },
                        principalTable: "ImapMailboxes",
                        principalColumns: new[] { "Id", "ImapAccountId" });
                });

            migrationBuilder.CreateTable(
                name: "ImapSyncRuns",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ImapAccountId = table.Column<long>(type: "bigint", nullable: false),
                    Trigger = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MailboxesAttempted = table.Column<int>(type: "integer", nullable: false),
                    MessagesDiscovered = table.Column<int>(type: "integer", nullable: false),
                    MessagesCreated = table.Column<int>(type: "integer", nullable: false),
                    MessagesUpdated = table.Column<int>(type: "integer", nullable: false),
                    MessageLocationsRemoved = table.Column<int>(type: "integer", nullable: false),
                    DownloadedBytes = table.Column<long>(type: "bigint", nullable: false),
                    ErrorSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImapSyncRuns", x => x.Id);
                    table.UniqueConstraint("AK_ImapSyncRuns_Id_ImapAccountId", x => new { x.Id, x.ImapAccountId });
                    table.ForeignKey(
                        name: "FK_ImapSyncRuns_ImapAccounts_ImapAccountId",
                        column: x => x.ImapAccountId,
                        principalTable: "ImapAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IncomingMailMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ImapAccountId = table.Column<long>(type: "bigint", nullable: false),
                    InternetMessageId = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    InternetMessageIdKey = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ContentSha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Subject = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SentAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReceivedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    BodyContentStatus = table.Column<int>(type: "integer", nullable: false),
                    AttachmentCount = table.Column<int>(type: "integer", nullable: false),
                    InlineResourceCount = table.Column<int>(type: "integer", nullable: false),
                    CurrentPrimaryClassification = table.Column<int>(type: "integer", nullable: false),
                    CurrentBounceType = table.Column<int>(type: "integer", nullable: false),
                    CurrentSpamScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    LastAnalyzedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CurrentClassificationUpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AnalysisSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingMailMessages", x => x.Id);
                    table.UniqueConstraint("AK_IncomingMailMessages_Id_ImapAccountId", x => new { x.Id, x.ImapAccountId });
                    table.ForeignKey(
                        name: "FK_IncomingMailMessages_ImapAccounts_ImapAccountId",
                        column: x => x.ImapAccountId,
                        principalTable: "ImapAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ImapAccountPrimaryOutboxes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ImapAccountId = table.Column<long>(type: "bigint", nullable: false),
                    ImapAccountOutboxLinkId = table.Column<long>(type: "bigint", nullable: false),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImapAccountPrimaryOutboxes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImapAccountPrimaryOutboxes_ImapAccountOutboxLinks_ImapAccou~",
                        columns: x => new { x.ImapAccountOutboxLinkId, x.ImapAccountId },
                        principalTable: "ImapAccountOutboxLinks",
                        principalColumns: new[] { "Id", "ImapAccountId" });
                    table.ForeignKey(
                        name: "FK_ImapAccountPrimaryOutboxes_ImapAccounts_ImapAccountId",
                        column: x => x.ImapAccountId,
                        principalTable: "ImapAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ImapMailboxSyncCheckpoints",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ImapMailboxId = table.Column<long>(type: "bigint", nullable: false),
                    UidValidity = table.Column<long>(type: "bigint", nullable: false),
                    LastCommittedUid = table.Column<long>(type: "bigint", nullable: true),
                    LastCommittedModSequence = table.Column<long>(type: "bigint", nullable: true),
                    SynchronizationGeneration = table.Column<long>(type: "bigint", nullable: false),
                    LastFullReconciliationAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastCommittedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
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
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ImapAccountId = table.Column<long>(type: "bigint", nullable: false),
                    ImapSyncRunId = table.Column<long>(type: "bigint", nullable: false),
                    ImapMailboxId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartUid = table.Column<long>(type: "bigint", nullable: true),
                    EndUid = table.Column<long>(type: "bigint", nullable: true),
                    StartCommittedModSequence = table.Column<long>(type: "bigint", nullable: true),
                    EndCommittedModSequence = table.Column<long>(type: "bigint", nullable: true),
                    CheckpointAdvanced = table.Column<bool>(type: "boolean", nullable: false),
                    MessagesDiscovered = table.Column<int>(type: "integer", nullable: false),
                    LocationsCreated = table.Column<int>(type: "integer", nullable: false),
                    LocationsUpdated = table.Column<int>(type: "integer", nullable: false),
                    DownloadedBytes = table.Column<long>(type: "bigint", nullable: false),
                    ErrorSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImapMailboxSyncRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImapMailboxSyncRuns_ImapMailboxes_ImapMailboxId_ImapAccount~",
                        columns: x => new { x.ImapMailboxId, x.ImapAccountId },
                        principalTable: "ImapMailboxes",
                        principalColumns: new[] { "Id", "ImapAccountId" });
                    table.ForeignKey(
                        name: "FK_ImapMailboxSyncRuns_ImapSyncRuns_ImapSyncRunId_ImapAccountId",
                        columns: x => new { x.ImapSyncRunId, x.ImapAccountId },
                        principalTable: "ImapSyncRuns",
                        principalColumns: new[] { "Id", "ImapAccountId" });
                });

            migrationBuilder.CreateTable(
                name: "IncomingMailAddresses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IncomingMailMessageId = table.Column<long>(type: "bigint", nullable: false),
                    AddressType = table.Column<int>(type: "integer", nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingMailAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomingMailAddresses_IncomingMailMessages_IncomingMailMess~",
                        column: x => x.IncomingMailMessageId,
                        principalTable: "IncomingMailMessages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IncomingMailAnalyses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IncomingMailMessageId = table.Column<long>(type: "bigint", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    AnalyzerVersion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    BounceType = table.Column<int>(type: "integer", nullable: false),
                    SpamScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    ResultSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InputContentSha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingMailAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomingMailAnalyses_IncomingMailMessages_IncomingMailMessa~",
                        column: x => x.IncomingMailMessageId,
                        principalTable: "IncomingMailMessages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IncomingMailLocations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ImapAccountId = table.Column<long>(type: "bigint", nullable: false),
                    IncomingMailMessageId = table.Column<long>(type: "bigint", nullable: false),
                    ImapMailboxId = table.Column<long>(type: "bigint", nullable: false),
                    UidValidity = table.Column<long>(type: "bigint", nullable: false),
                    Uid = table.Column<long>(type: "bigint", nullable: false),
                    ModSequence = table.Column<long>(type: "bigint", nullable: true),
                    Flags = table.Column<int>(type: "integer", nullable: false),
                    IsPresentOnServer = table.Column<bool>(type: "boolean", nullable: false),
                    FirstSeenAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSynchronizedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingMailLocations", x => x.Id);
                    table.UniqueConstraint("AK_IncomingMailLocations_Id_ImapAccountId", x => new { x.Id, x.ImapAccountId });
                    table.ForeignKey(
                        name: "FK_IncomingMailLocations_ImapMailboxes_ImapMailboxId_ImapAccou~",
                        columns: x => new { x.ImapMailboxId, x.ImapAccountId },
                        principalTable: "ImapMailboxes",
                        principalColumns: new[] { "Id", "ImapAccountId" });
                    table.ForeignKey(
                        name: "FK_IncomingMailLocations_IncomingMailMessages_IncomingMailMess~",
                        columns: x => new { x.IncomingMailMessageId, x.ImapAccountId },
                        principalTable: "IncomingMailMessages",
                        principalColumns: new[] { "Id", "ImapAccountId" });
                });

            migrationBuilder.CreateTable(
                name: "IncomingMailMimeParts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IncomingMailMessageId = table.Column<long>(type: "bigint", nullable: false),
                    MimePartPath = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ParentMimePartPath = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PartKind = table.Column<int>(type: "integer", nullable: false),
                    ContentDisposition = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    MediaType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ContentId = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DeclaredSize = table.Column<long>(type: "bigint", nullable: true),
                    FetchStatus = table.Column<int>(type: "integer", nullable: false),
                    FileUsageId = table.Column<long>(type: "bigint", nullable: true),
                    ContentSha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DownloadedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastFetchError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
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
                        name: "FK_IncomingMailMimeParts_IncomingMailMessages_IncomingMailMess~",
                        column: x => x.IncomingMailMessageId,
                        principalTable: "IncomingMailMessages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IncomingMailReferences",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IncomingMailMessageId = table.Column<long>(type: "bigint", nullable: false),
                    ReferenceType = table.Column<int>(type: "integer", nullable: false),
                    InternetMessageId = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingMailReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomingMailReferences_IncomingMailMessages_IncomingMailMes~",
                        column: x => x.IncomingMailMessageId,
                        principalTable: "IncomingMailMessages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IncomingMailSendingItemLinks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IncomingMailMessageId = table.Column<long>(type: "bigint", nullable: false),
                    SendingItemId = table.Column<long>(type: "bigint", nullable: false),
                    SendingItemInboxId = table.Column<long>(type: "bigint", nullable: true),
                    LinkType = table.Column<int>(type: "integer", nullable: false),
                    MatchMethod = table.Column<int>(type: "integer", nullable: false),
                    Confidence = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReviewedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    ReviewedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MatchReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingMailSendingItemLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomingMailSendingItemLinks_IncomingMailMessages_IncomingM~",
                        column: x => x.IncomingMailMessageId,
                        principalTable: "IncomingMailMessages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IncomingMailSendingItemLinks_SendingItemInboxes_SendingItem~",
                        column: x => x.SendingItemInboxId,
                        principalTable: "SendingItemInboxes",
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
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IncomingMailAnalysisId = table.Column<long>(type: "bigint", nullable: false),
                    Classification = table.Column<int>(type: "integer", nullable: false),
                    Confidence = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingMailAnalysisClassifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomingMailAnalysisClassifications_IncomingMailAnalyses_In~",
                        column: x => x.IncomingMailAnalysisId,
                        principalTable: "IncomingMailAnalyses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IncomingMailClassificationEvidences",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IncomingMailAnalysisId = table.Column<long>(type: "bigint", nullable: false),
                    EvidenceType = table.Column<int>(type: "integer", nullable: false),
                    Location = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ValueSha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingMailClassificationEvidences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomingMailClassificationEvidences_IncomingMailAnalyses_In~",
                        column: x => x.IncomingMailAnalysisId,
                        principalTable: "IncomingMailAnalyses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IncomingMailCurrentClassifications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IncomingMailMessageId = table.Column<long>(type: "bigint", nullable: false),
                    IncomingMailAnalysisId = table.Column<long>(type: "bigint", nullable: false),
                    Classification = table.Column<int>(type: "integer", nullable: false),
                    Confidence = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    AppliedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingMailCurrentClassifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomingMailCurrentClassifications_IncomingMailAnalyses_Inc~",
                        column: x => x.IncomingMailAnalysisId,
                        principalTable: "IncomingMailAnalyses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IncomingMailCurrentClassifications_IncomingMailMessages_Inc~",
                        column: x => x.IncomingMailMessageId,
                        principalTable: "IncomingMailMessages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IncomingMailDeliveryStatuses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IncomingMailAnalysisId = table.Column<long>(type: "bigint", nullable: false),
                    MimePartPath = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    OriginalRecipient = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FinalRecipient = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FinalRecipientEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    EnhancedStatusCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    DiagnosticCode = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RemoteMta = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingMailDeliveryStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomingMailDeliveryStatuses_IncomingMailAnalyses_IncomingM~",
                        column: x => x.IncomingMailAnalysisId,
                        principalTable: "IncomingMailAnalyses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IncomingMailFeedbackReports",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IncomingMailAnalysisId = table.Column<long>(type: "bigint", nullable: false),
                    MimePartPath = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FeedbackType = table.Column<int>(type: "integer", nullable: false),
                    OriginalRecipient = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OriginalRecipientEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    ReportedDomain = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingMailFeedbackReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomingMailFeedbackReports_IncomingMailAnalyses_IncomingMa~",
                        column: x => x.IncomingMailAnalysisId,
                        principalTable: "IncomingMailAnalyses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ImapSyncCommands",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ImapAccountId = table.Column<long>(type: "bigint", nullable: false),
                    ImapMailboxId = table.Column<long>(type: "bigint", nullable: false),
                    IncomingMailLocationId = table.Column<long>(type: "bigint", nullable: true),
                    CommandType = table.Column<int>(type: "integer", nullable: false),
                    FlagMutationMode = table.Column<int>(type: "integer", nullable: true),
                    RequestedFlags = table.Column<int>(type: "integer", nullable: false),
                    Keyword = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DestinationMailboxId = table.Column<long>(type: "bigint", nullable: true),
                    ExpectedUidValidity = table.Column<long>(type: "bigint", nullable: true),
                    ExpectedUid = table.Column<long>(type: "bigint", nullable: true),
                    ExpectedModSequence = table.Column<long>(type: "bigint", nullable: true),
                    IdempotencyKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    NextAttemptAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImapSyncCommands", x => x.Id);
                    table.UniqueConstraint("AK_ImapSyncCommands_Id_ImapAccountId", x => new { x.Id, x.ImapAccountId });
                    table.ForeignKey(
                        name: "FK_ImapSyncCommands_ImapAccounts_ImapAccountId",
                        column: x => x.ImapAccountId,
                        principalTable: "ImapAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ImapSyncCommands_ImapMailboxes_DestinationMailboxId_ImapAcc~",
                        columns: x => new { x.DestinationMailboxId, x.ImapAccountId },
                        principalTable: "ImapMailboxes",
                        principalColumns: new[] { "Id", "ImapAccountId" });
                    table.ForeignKey(
                        name: "FK_ImapSyncCommands_ImapMailboxes_ImapMailboxId_ImapAccountId",
                        columns: x => new { x.ImapMailboxId, x.ImapAccountId },
                        principalTable: "ImapMailboxes",
                        principalColumns: new[] { "Id", "ImapAccountId" });
                    table.ForeignKey(
                        name: "FK_ImapSyncCommands_IncomingMailLocations_IncomingMailLocation~",
                        columns: x => new { x.IncomingMailLocationId, x.ImapAccountId },
                        principalTable: "IncomingMailLocations",
                        principalColumns: new[] { "Id", "ImapAccountId" });
                });

            migrationBuilder.CreateTable(
                name: "IncomingMailKeywords",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IncomingMailLocationId = table.Column<long>(type: "bigint", nullable: false),
                    Keyword = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingMailKeywords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomingMailKeywords_IncomingMailLocations_IncomingMailLoca~",
                        column: x => x.IncomingMailLocationId,
                        principalTable: "IncomingMailLocations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IncomingMailAuditEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ImapAccountId = table.Column<long>(type: "bigint", nullable: false),
                    IncomingMailMessageId = table.Column<long>(type: "bigint", nullable: true),
                    IncomingMailLocationId = table.Column<long>(type: "bigint", nullable: true),
                    ImapSyncRunId = table.Column<long>(type: "bigint", nullable: true),
                    ImapSyncCommandId = table.Column<long>(type: "bigint", nullable: true),
                    ActorType = table.Column<int>(type: "integer", nullable: false),
                    ActorUserId = table.Column<long>(type: "bigint", nullable: true),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingMailAuditEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomingMailAuditEvents_ImapAccounts_ImapAccountId",
                        column: x => x.ImapAccountId,
                        principalTable: "ImapAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IncomingMailAuditEvents_ImapSyncCommands_ImapSyncCommandId_~",
                        columns: x => new { x.ImapSyncCommandId, x.ImapAccountId },
                        principalTable: "ImapSyncCommands",
                        principalColumns: new[] { "Id", "ImapAccountId" });
                    table.ForeignKey(
                        name: "FK_IncomingMailAuditEvents_ImapSyncRuns_ImapSyncRunId_ImapAcco~",
                        columns: x => new { x.ImapSyncRunId, x.ImapAccountId },
                        principalTable: "ImapSyncRuns",
                        principalColumns: new[] { "Id", "ImapAccountId" });
                    table.ForeignKey(
                        name: "FK_IncomingMailAuditEvents_IncomingMailLocations_IncomingMailL~",
                        columns: x => new { x.IncomingMailLocationId, x.ImapAccountId },
                        principalTable: "IncomingMailLocations",
                        principalColumns: new[] { "Id", "ImapAccountId" });
                    table.ForeignKey(
                        name: "FK_IncomingMailAuditEvents_IncomingMailMessages_IncomingMailMe~",
                        columns: x => new { x.IncomingMailMessageId, x.ImapAccountId },
                        principalTable: "IncomingMailMessages",
                        principalColumns: new[] { "Id", "ImapAccountId" });
                });

            migrationBuilder.CreateIndex(
                name: "IX_SendingItems_InternetMessageIdKey",
                table: "SendingItems",
                column: "InternetMessageIdKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileUsages_OwnerUserId_Scope_CreateDate",
                table: "FileUsages",
                columns: new[] { "OwnerUserId", "Scope", "CreateDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ImapAccountCredentials_ImapAccountId",
                table: "ImapAccountCredentials",
                column: "ImapAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImapAccountOutboxLinks_ImapAccountId_OutboxId",
                table: "ImapAccountOutboxLinks",
                columns: new[] { "ImapAccountId", "OutboxId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImapAccountOutboxLinks_OutboxId",
                table: "ImapAccountOutboxLinks",
                column: "OutboxId");

            migrationBuilder.CreateIndex(
                name: "IX_ImapAccountPrimaryOutboxes_ImapAccountId",
                table: "ImapAccountPrimaryOutboxes",
                column: "ImapAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImapAccountPrimaryOutboxes_ImapAccountOutboxLinkId",
                table: "ImapAccountPrimaryOutboxes",
                column: "ImapAccountOutboxLinkId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImapAccountPrimaryOutboxes_ImapAccountOutboxLinkId_ImapAcco~",
                table: "ImapAccountPrimaryOutboxes",
                columns: new[] { "ImapAccountOutboxLinkId", "ImapAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_ImapAccounts_Status_NextSyncAtUtc",
                table: "ImapAccounts",
                columns: new[] { "Status", "NextSyncAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ImapAccounts_User_Email_Host_Port",
                table: "ImapAccounts",
                columns: new[] { "UserId", "Email", "Host", "Port" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImapMailboxes_ImapAccountId_IsSynchronizationEnabled",
                table: "ImapMailboxes",
                columns: new[] { "ImapAccountId", "IsSynchronizationEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_ImapMailboxes_ImapAccountId_RemoteFullName",
                table: "ImapMailboxes",
                columns: new[] { "ImapAccountId", "RemoteFullName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImapMailboxes_ParentId_ImapAccountId",
                table: "ImapMailboxes",
                columns: new[] { "ParentId", "ImapAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_ImapMailboxSyncCheckpoints_ImapMailboxId",
                table: "ImapMailboxSyncCheckpoints",
                column: "ImapMailboxId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImapMailboxSyncRuns_ImapMailboxId_ImapAccountId",
                table: "ImapMailboxSyncRuns",
                columns: new[] { "ImapMailboxId", "ImapAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_ImapMailboxSyncRuns_ImapSyncRunId_ImapAccountId",
                table: "ImapMailboxSyncRuns",
                columns: new[] { "ImapSyncRunId", "ImapAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_ImapMailboxSyncRuns_ImapSyncRunId_ImapMailboxId",
                table: "ImapMailboxSyncRuns",
                columns: new[] { "ImapSyncRunId", "ImapMailboxId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImapSyncCommands_DestinationMailboxId_ImapAccountId",
                table: "ImapSyncCommands",
                columns: new[] { "DestinationMailboxId", "ImapAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_ImapSyncCommands_ImapAccountId_IdempotencyKey",
                table: "ImapSyncCommands",
                columns: new[] { "ImapAccountId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImapSyncCommands_ImapMailboxId_ImapAccountId",
                table: "ImapSyncCommands",
                columns: new[] { "ImapMailboxId", "ImapAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_ImapSyncCommands_ImapMailboxId_IncomingMailLocationId",
                table: "ImapSyncCommands",
                columns: new[] { "ImapMailboxId", "IncomingMailLocationId" });

            migrationBuilder.CreateIndex(
                name: "IX_ImapSyncCommands_IncomingMailLocationId_ImapAccountId",
                table: "ImapSyncCommands",
                columns: new[] { "IncomingMailLocationId", "ImapAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_ImapSyncCommands_Status_NextAttemptAtUtc",
                table: "ImapSyncCommands",
                columns: new[] { "Status", "NextAttemptAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ImapSyncRuns_ImapAccountId_StartedAtUtc",
                table: "ImapSyncRuns",
                columns: new[] { "ImapAccountId", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailAddresses_Email",
                table: "IncomingMailAddresses",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailAddresses_IncomingMailMessageId_AddressType_Pos~",
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
                name: "IX_IncomingMailAnalysisClassifications_IncomingMailAnalysisId_~",
                table: "IncomingMailAnalysisClassifications",
                columns: new[] { "IncomingMailAnalysisId", "Classification" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailAuditEvents_ImapAccountId_OccurredAtUtc",
                table: "IncomingMailAuditEvents",
                columns: new[] { "ImapAccountId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailAuditEvents_ImapSyncCommandId_ImapAccountId",
                table: "IncomingMailAuditEvents",
                columns: new[] { "ImapSyncCommandId", "ImapAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailAuditEvents_ImapSyncRunId_ImapAccountId",
                table: "IncomingMailAuditEvents",
                columns: new[] { "ImapSyncRunId", "ImapAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailAuditEvents_IncomingMailLocationId_ImapAccountId",
                table: "IncomingMailAuditEvents",
                columns: new[] { "IncomingMailLocationId", "ImapAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailAuditEvents_IncomingMailMessageId_ImapAccountId",
                table: "IncomingMailAuditEvents",
                columns: new[] { "IncomingMailMessageId", "ImapAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailAuditEvents_IncomingMailMessageId_OccurredAtUtc",
                table: "IncomingMailAuditEvents",
                columns: new[] { "IncomingMailMessageId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailClassificationEvidences_IncomingMailAnalysisId_~",
                table: "IncomingMailClassificationEvidences",
                columns: new[] { "IncomingMailAnalysisId", "EvidenceType" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailCurrentClassifications_Classification_AppliedAt~",
                table: "IncomingMailCurrentClassifications",
                columns: new[] { "Classification", "AppliedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailCurrentClassifications_IncomingMailAnalysisId",
                table: "IncomingMailCurrentClassifications",
                column: "IncomingMailAnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailCurrentClassifications_IncomingMailMessageId_Cl~",
                table: "IncomingMailCurrentClassifications",
                columns: new[] { "IncomingMailMessageId", "Classification" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailDeliveryStatuses_FinalRecipientEmail_Action",
                table: "IncomingMailDeliveryStatuses",
                columns: new[] { "FinalRecipientEmail", "Action" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailDeliveryStatuses_IncomingMailAnalysisId_MimePar~",
                table: "IncomingMailDeliveryStatuses",
                columns: new[] { "IncomingMailAnalysisId", "MimePartPath", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailFeedbackReports_IncomingMailAnalysisId",
                table: "IncomingMailFeedbackReports",
                column: "IncomingMailAnalysisId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailFeedbackReports_OriginalRecipientEmail_Feedback~",
                table: "IncomingMailFeedbackReports",
                columns: new[] { "OriginalRecipientEmail", "FeedbackType" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailKeywords_IncomingMailLocationId_Keyword",
                table: "IncomingMailKeywords",
                columns: new[] { "IncomingMailLocationId", "Keyword" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailLocations_ImapMailboxId_ImapAccountId",
                table: "IncomingMailLocations",
                columns: new[] { "ImapMailboxId", "ImapAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailLocations_ImapMailboxId_IsPresentOnServer_Uid",
                table: "IncomingMailLocations",
                columns: new[] { "ImapMailboxId", "IsPresentOnServer", "Uid" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailLocations_ImapMailboxId_ModSequence",
                table: "IncomingMailLocations",
                columns: new[] { "ImapMailboxId", "ModSequence" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailLocations_ImapMailboxId_UidValidity_Uid",
                table: "IncomingMailLocations",
                columns: new[] { "ImapMailboxId", "UidValidity", "Uid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailLocations_IncomingMailMessageId_ImapAccountId",
                table: "IncomingMailLocations",
                columns: new[] { "IncomingMailMessageId", "ImapAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailMessages_ImapAccountId_BodyContentStatus_Receiv~",
                table: "IncomingMailMessages",
                columns: new[] { "ImapAccountId", "BodyContentStatus", "ReceivedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailMessages_ImapAccountId_ContentSha256",
                table: "IncomingMailMessages",
                columns: new[] { "ImapAccountId", "ContentSha256" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailMessages_ImapAccountId_CurrentBounceType_Receiv~",
                table: "IncomingMailMessages",
                columns: new[] { "ImapAccountId", "CurrentBounceType", "ReceivedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailMessages_ImapAccountId_CurrentPrimaryClassifica~",
                table: "IncomingMailMessages",
                columns: new[] { "ImapAccountId", "CurrentPrimaryClassification", "ReceivedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailMessages_ImapAccountId_CurrentSpamScore",
                table: "IncomingMailMessages",
                columns: new[] { "ImapAccountId", "CurrentSpamScore" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailMessages_ImapAccountId_InternetMessageIdKey",
                table: "IncomingMailMessages",
                columns: new[] { "ImapAccountId", "InternetMessageIdKey" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailMessages_ImapAccountId_ReceivedAtUtc",
                table: "IncomingMailMessages",
                columns: new[] { "ImapAccountId", "ReceivedAtUtc" });

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
                name: "IX_IncomingMailReferences_IncomingMailMessageId_ReferenceType_~",
                table: "IncomingMailReferences",
                columns: new[] { "IncomingMailMessageId", "ReferenceType", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailReferences_InternetMessageId",
                table: "IncomingMailReferences",
                column: "InternetMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailSendingItemLinks_IncomingMailMessageId_SendingI~",
                table: "IncomingMailSendingItemLinks",
                columns: new[] { "IncomingMailMessageId", "SendingItemId", "SendingItemInboxId", "LinkType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailSendingItemLinks_SendingItemId_LinkType_Status",
                table: "IncomingMailSendingItemLinks",
                columns: new[] { "SendingItemId", "LinkType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailSendingItemLinks_SendingItemInboxId",
                table: "IncomingMailSendingItemLinks",
                column: "SendingItemInboxId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImapAccountCredentials");

            migrationBuilder.DropTable(
                name: "ImapAccountPrimaryOutboxes");

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
                name: "ImapAccountOutboxLinks");

            migrationBuilder.DropTable(
                name: "ImapSyncCommands");

            migrationBuilder.DropTable(
                name: "ImapSyncRuns");

            migrationBuilder.DropTable(
                name: "IncomingMailAnalyses");

            migrationBuilder.DropTable(
                name: "IncomingMailLocations");

            migrationBuilder.DropTable(
                name: "ImapMailboxes");

            migrationBuilder.DropTable(
                name: "IncomingMailMessages");

            migrationBuilder.DropTable(
                name: "ImapAccounts");

            migrationBuilder.DropIndex(
                name: "IX_SendingItems_InternetMessageIdKey",
                table: "SendingItems");

            migrationBuilder.DropIndex(
                name: "IX_FileUsages_OwnerUserId_Scope_CreateDate",
                table: "FileUsages");

            migrationBuilder.DropColumn(
                name: "InternetMessageId",
                table: "SendingItems");

            migrationBuilder.DropColumn(
                name: "InternetMessageIdKey",
                table: "SendingItems");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "FileUsages");
        }
    }
}
