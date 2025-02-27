using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMailService.Migrations.SqLite
{
    /// <inheritdoc />
    public partial class changeOutboxIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Outboxes_Email",
                table: "Outboxes");

            migrationBuilder.DropIndex(
                name: "IX_Inboxes_Email",
                table: "Inboxes");

            migrationBuilder.CreateIndex(
                name: "IX_Outboxes_Email_UserId",
                table: "Outboxes",
                columns: new[] { "Email", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inboxes_Email_UserId",
                table: "Inboxes",
                columns: new[] { "Email", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Outboxes_Email_UserId",
                table: "Outboxes");

            migrationBuilder.DropIndex(
                name: "IX_Inboxes_Email_UserId",
                table: "Inboxes");

            migrationBuilder.CreateIndex(
                name: "IX_Outboxes_Email",
                table: "Outboxes",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inboxes_Email",
                table: "Inboxes",
                column: "Email",
                unique: true);
        }
    }
}
