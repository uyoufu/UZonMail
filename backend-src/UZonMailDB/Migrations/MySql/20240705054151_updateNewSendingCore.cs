using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMailService.Migrations.Mysql
{
    /// <inheritdoc />
    public partial class updateNewSendingCore : Migration
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

            migrationBuilder.AddColumn<int>(
                name: "MaxRetryCount",
                table: "UserSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "UserSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Weight",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Weight",
                table: "Outboxes",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.DropColumn(
                name: "MaxRetryCount",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Outboxes");

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
