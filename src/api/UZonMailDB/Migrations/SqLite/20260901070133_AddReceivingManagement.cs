using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UzonMail.DB.Migrations.SqLite
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
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceInternetMessageIds",
                table: "SendingItems",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "BodyCachedAtUtc",
                table: "IncomingMailMessages",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BodyExpiresAtUtc",
                table: "IncomingMailMessages",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CachedHtmlBody",
                table: "IncomingMailMessages",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CachedTextBody",
                table: "IncomingMailMessages",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Direction",
                table: "IncomingMailMessages",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "SendingItemId",
                table: "IncomingMailMessages",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MailContacts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    LastInteractionAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    OrganizationId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailContacts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MailConversations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmailAccountId = table.Column<long>(type: "INTEGER", nullable: false),
                    ConversationType = table.Column<int>(type: "INTEGER", nullable: false),
                    ParticipantSetKey = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    DisplayTitle = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    LastMessageAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastMessagePreview = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    UnreadCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LastReadAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    OrganizationId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false)
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
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Color = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    OrganizationId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailTags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TodoTasks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 10000, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    DueAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    OrganizationId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoTasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MailConversationMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MailConversationId = table.Column<long>(type: "INTEGER", nullable: false),
                    IncomingMailMessageId = table.Column<long>(type: "INTEGER", nullable: true),
                    SendingItemId = table.Column<long>(type: "INTEGER", nullable: true),
                    SourceKey = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Direction = table.Column<int>(type: "INTEGER", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReplyToConversationMessageId = table.Column<long>(type: "INTEGER", nullable: true),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailConversationMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MailConversationMessages_IncomingMailMessages_IncomingMailMessageId",
                        column: x => x.IncomingMailMessageId,
                        principalTable: "IncomingMailMessages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MailConversationMessages_MailConversationMessages_ReplyToConversationMessageId",
                        column: x => x.ReplyToConversationMessageId,
                        principalTable: "MailConversationMessages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MailConversationMessages_MailConversations_MailConversationId",
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
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MailConversationId = table.Column<long>(type: "INTEGER", nullable: false),
                    MailContactId = table.Column<long>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    JoinedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LeftAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
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
                        name: "FK_MailConversationParticipants_MailConversations_MailConversationId",
                        column: x => x.MailConversationId,
                        principalTable: "MailConversations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MailContactTags",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MailContactId = table.Column<long>(type: "INTEGER", nullable: false),
                    MailTagId = table.Column<long>(type: "INTEGER", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
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
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TodoTaskId = table.Column<long>(type: "INTEGER", nullable: false),
                    SourceConversationId = table.Column<long>(type: "INTEGER", nullable: false),
                    BranchSubject = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    RootSendingItemId = table.Column<long>(type: "INTEGER", nullable: true),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
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
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TodoMailBranchId = table.Column<long>(type: "INTEGER", nullable: false),
                    MailConversationMessageId = table.Column<long>(type: "INTEGER", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoMailBranchMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TodoMailBranchMessages_MailConversationMessages_MailConversationMessageId",
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
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TodoMailBranchId = table.Column<long>(type: "INTEGER", nullable: false),
                    MailConversationMessageId = table.Column<long>(type: "INTEGER", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoMailBranchSourceMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TodoMailBranchSourceMessages_MailConversationMessages_MailConversationMessageId",
                        column: x => x.MailConversationMessageId,
                        principalTable: "MailConversationMessages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TodoMailBranchSourceMessages_TodoMailBranches_TodoMailBranchId",
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
                name: "IX_MailConversationParticipants_MailConversationId_MailContactId",
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
                name: "IX_TodoMailBranchMessages_TodoMailBranchId_MailConversationMessageId",
                table: "TodoMailBranchMessages",
                columns: new[] { "TodoMailBranchId", "MailConversationMessageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TodoMailBranchSourceMessages_MailConversationMessageId",
                table: "TodoMailBranchSourceMessages",
                column: "MailConversationMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_TodoMailBranchSourceMessages_TodoMailBranchId_MailConversationMessageId",
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
