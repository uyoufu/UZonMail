using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UzonMail.DB.Migrations.PostgreSQL
{
    /// <inheritdoc />
    public partial class AddReceivingManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InReplyToInternetMessageId",
                table: "SendingItems",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceInternetMessageIds",
                table: "SendingItems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "BodyCachedAtUtc",
                table: "IncomingMailMessages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BodyExpiresAtUtc",
                table: "IncomingMailMessages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CachedHtmlBody",
                table: "IncomingMailMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CachedTextBody",
                table: "IncomingMailMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Direction",
                table: "IncomingMailMessages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "SendingItemId",
                table: "IncomingMailMessages",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MailContacts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LastInteractionAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false),
                    OrganizationId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailContacts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MailConversations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmailAccountId = table.Column<long>(type: "bigint", nullable: false),
                    ConversationType = table.Column<int>(type: "integer", nullable: false),
                    ParticipantSetKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DisplayTitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LastMessageAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastMessagePreview = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UnreadCount = table.Column<int>(type: "integer", nullable: false),
                    LastReadAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false),
                    OrganizationId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailConversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MailConversations_EmailAccounts_EmailAccountId",
                        column: x => x.EmailAccountId,
                        principalTable: "EmailAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MailTags",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false),
                    OrganizationId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailTags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TodoTasks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    DueAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false),
                    OrganizationId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoTasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MailConversationMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MailConversationId = table.Column<long>(type: "bigint", nullable: false),
                    IncomingMailMessageId = table.Column<long>(type: "bigint", nullable: true),
                    SendingItemId = table.Column<long>(type: "bigint", nullable: true),
                    SourceKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    ReplyToConversationMessageId = table.Column<long>(type: "bigint", nullable: true),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailConversationMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MailConversationMessages_IncomingMailMessages_IncomingMailM~",
                        column: x => x.IncomingMailMessageId,
                        principalTable: "IncomingMailMessages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MailConversationMessages_MailConversationMessages_ReplyToCo~",
                        column: x => x.ReplyToConversationMessageId,
                        principalTable: "MailConversationMessages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MailConversationMessages_MailConversations_MailConversation~",
                        column: x => x.MailConversationId,
                        principalTable: "MailConversations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MailConversationMessages_SendingItems_SendingItemId",
                        column: x => x.SendingItemId,
                        principalTable: "SendingItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MailConversationParticipants",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MailConversationId = table.Column<long>(type: "bigint", nullable: false),
                    MailContactId = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    JoinedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LeftAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailConversationParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MailConversationParticipants_MailContacts_MailContactId",
                        column: x => x.MailContactId,
                        principalTable: "MailContacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MailConversationParticipants_MailConversations_MailConversa~",
                        column: x => x.MailConversationId,
                        principalTable: "MailConversations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MailContactTags",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MailContactId = table.Column<long>(type: "bigint", nullable: false),
                    MailTagId = table.Column<long>(type: "bigint", nullable: false),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailContactTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MailContactTags_MailContacts_MailContactId",
                        column: x => x.MailContactId,
                        principalTable: "MailContacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MailContactTags_MailTags_MailTagId",
                        column: x => x.MailTagId,
                        principalTable: "MailTags",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TodoMailBranches",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TodoTaskId = table.Column<long>(type: "bigint", nullable: false),
                    SourceConversationId = table.Column<long>(type: "bigint", nullable: false),
                    BranchSubject = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    RootSendingItemId = table.Column<long>(type: "bigint", nullable: true),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoMailBranches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TodoMailBranches_MailConversations_SourceConversationId",
                        column: x => x.SourceConversationId,
                        principalTable: "MailConversations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TodoMailBranches_TodoTasks_TodoTaskId",
                        column: x => x.TodoTaskId,
                        principalTable: "TodoTasks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TodoMailBranchMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TodoMailBranchId = table.Column<long>(type: "bigint", nullable: false),
                    MailConversationMessageId = table.Column<long>(type: "bigint", nullable: false),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoMailBranchMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TodoMailBranchMessages_MailConversationMessages_MailConvers~",
                        column: x => x.MailConversationMessageId,
                        principalTable: "MailConversationMessages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TodoMailBranchMessages_TodoMailBranches_TodoMailBranchId",
                        column: x => x.TodoMailBranchId,
                        principalTable: "TodoMailBranches",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TodoMailBranchSourceMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TodoMailBranchId = table.Column<long>(type: "bigint", nullable: false),
                    MailConversationMessageId = table.Column<long>(type: "bigint", nullable: false),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoMailBranchSourceMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TodoMailBranchSourceMessages_MailConversationMessages_MailC~",
                        column: x => x.MailConversationMessageId,
                        principalTable: "MailConversationMessages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TodoMailBranchSourceMessages_TodoMailBranches_TodoMailBranc~",
                        column: x => x.TodoMailBranchId,
                        principalTable: "TodoMailBranches",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMailMessages_SendingItemId",
                table: "IncomingMailMessages",
                column: "SendingItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MailContacts_UserId_LastInteractionAtUtc",
                table: "MailContacts",
                columns: new[] { "UserId", "LastInteractionAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_MailContacts_UserId_NormalizedEmail",
                table: "MailContacts",
                columns: new[] { "UserId", "NormalizedEmail" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MailContactTags_MailContactId_MailTagId",
                table: "MailContactTags",
                columns: new[] { "MailContactId", "MailTagId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MailContactTags_MailTagId",
                table: "MailContactTags",
                column: "MailTagId");

            migrationBuilder.CreateIndex(
                name: "IX_MailConversationMessages_IncomingMailMessageId",
                table: "MailConversationMessages",
                column: "IncomingMailMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_MailConversationMessages_MailConversationId_OccurredAtUtc_Id",
                table: "MailConversationMessages",
                columns: new[] { "MailConversationId", "OccurredAtUtc", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_MailConversationMessages_MailConversationId_SourceKey",
                table: "MailConversationMessages",
                columns: new[] { "MailConversationId", "SourceKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MailConversationMessages_ReplyToConversationMessageId",
                table: "MailConversationMessages",
                column: "ReplyToConversationMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_MailConversationMessages_SendingItemId",
                table: "MailConversationMessages",
                column: "SendingItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MailConversationParticipants_MailContactId_IsActive",
                table: "MailConversationParticipants",
                columns: new[] { "MailContactId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MailConversationParticipants_MailConversationId_MailContact~",
                table: "MailConversationParticipants",
                columns: new[] { "MailConversationId", "MailContactId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MailConversations_EmailAccountId",
                table: "MailConversations",
                column: "EmailAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_MailConversations_UserId_EmailAccountId_ParticipantSetKey",
                table: "MailConversations",
                columns: new[] { "UserId", "EmailAccountId", "ParticipantSetKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MailConversations_UserId_LastMessageAtUtc_Id",
                table: "MailConversations",
                columns: new[] { "UserId", "LastMessageAtUtc", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_MailTags_UserId_NormalizedName",
                table: "MailTags",
                columns: new[] { "UserId", "NormalizedName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TodoMailBranches_SourceConversationId_Id",
                table: "TodoMailBranches",
                columns: new[] { "SourceConversationId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_TodoMailBranches_TodoTaskId",
                table: "TodoMailBranches",
                column: "TodoTaskId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TodoMailBranchMessages_MailConversationMessageId",
                table: "TodoMailBranchMessages",
                column: "MailConversationMessageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TodoMailBranchMessages_TodoMailBranchId_MailConversationMes~",
                table: "TodoMailBranchMessages",
                columns: new[] { "TodoMailBranchId", "MailConversationMessageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TodoMailBranchSourceMessages_MailConversationMessageId",
                table: "TodoMailBranchSourceMessages",
                column: "MailConversationMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_TodoMailBranchSourceMessages_TodoMailBranchId_MailConversat~",
                table: "TodoMailBranchSourceMessages",
                columns: new[] { "TodoMailBranchId", "MailConversationMessageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TodoTasks_UserId_Status_DueAtUtc_Id",
                table: "TodoTasks",
                columns: new[] { "UserId", "Status", "DueAtUtc", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_TodoTasks_UserId_UpdatedAtUtc_Id",
                table: "TodoTasks",
                columns: new[] { "UserId", "UpdatedAtUtc", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_IncomingMailMessages_SendingItems_SendingItemId",
                table: "IncomingMailMessages",
                column: "SendingItemId",
                principalTable: "SendingItems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IncomingMailMessages_SendingItems_SendingItemId",
                table: "IncomingMailMessages");

            migrationBuilder.DropTable(
                name: "MailContactTags");

            migrationBuilder.DropTable(
                name: "MailConversationParticipants");

            migrationBuilder.DropTable(
                name: "TodoMailBranchMessages");

            migrationBuilder.DropTable(
                name: "TodoMailBranchSourceMessages");

            migrationBuilder.DropTable(
                name: "MailTags");

            migrationBuilder.DropTable(
                name: "MailContacts");

            migrationBuilder.DropTable(
                name: "MailConversationMessages");

            migrationBuilder.DropTable(
                name: "TodoMailBranches");

            migrationBuilder.DropTable(
                name: "MailConversations");

            migrationBuilder.DropTable(
                name: "TodoTasks");

            migrationBuilder.DropIndex(
                name: "IX_IncomingMailMessages_SendingItemId",
                table: "IncomingMailMessages");

            migrationBuilder.DropColumn(
                name: "InReplyToInternetMessageId",
                table: "SendingItems");

            migrationBuilder.DropColumn(
                name: "ReferenceInternetMessageIds",
                table: "SendingItems");

            migrationBuilder.DropColumn(
                name: "BodyCachedAtUtc",
                table: "IncomingMailMessages");

            migrationBuilder.DropColumn(
                name: "BodyExpiresAtUtc",
                table: "IncomingMailMessages");

            migrationBuilder.DropColumn(
                name: "CachedHtmlBody",
                table: "IncomingMailMessages");

            migrationBuilder.DropColumn(
                name: "CachedTextBody",
                table: "IncomingMailMessages");

            migrationBuilder.DropColumn(
                name: "Direction",
                table: "IncomingMailMessages");

            migrationBuilder.DropColumn(
                name: "SendingItemId",
                table: "IncomingMailMessages");
        }
    }
}
