using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMail.DB.Migrations.Mysql
{
    /// <inheritdoc />
    public partial class perfSendingItemInbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SendingItemInboxes_Inboxes_InboxId",
                table: "SendingItemInboxes");

            migrationBuilder.DropForeignKey(
                name: "FK_SendingItemInboxes_SendingItems_SendingItemId",
                table: "SendingItemInboxes");

            migrationBuilder.DropIndex(
                name: "IX_SendingItemInboxes_InboxId",
                table: "SendingItemInboxes");

            migrationBuilder.DropIndex(
                name: "IX_SendingItemInboxes_SendingItemId",
                table: "SendingItemInboxes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SendingItemInboxes_InboxId",
                table: "SendingItemInboxes",
                column: "InboxId");

            migrationBuilder.CreateIndex(
                name: "IX_SendingItemInboxes_SendingItemId",
                table: "SendingItemInboxes",
                column: "SendingItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_SendingItemInboxes_Inboxes_InboxId",
                table: "SendingItemInboxes",
                column: "InboxId",
                principalTable: "Inboxes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SendingItemInboxes_SendingItems_SendingItemId",
                table: "SendingItemInboxes",
                column: "SendingItemId",
                principalTable: "SendingItems",
                principalColumn: "Id");
        }
    }
}
