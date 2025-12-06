using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMail.DB.Migrations.Mysql
{
    /// <inheritdoc />
    public partial class addExtraFieldToInbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Outboxes_Email_UserId",
                table: "Outboxes");

            migrationBuilder.DropIndex(
                name: "IX_Inboxes_Email_UserId",
                table: "Inboxes");

            migrationBuilder.AddColumn<string>(
                name: "Extra",
                table: "EmailGroups",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "AppSettings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Outboxes_Email",
                table: "Outboxes",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Outboxes_UserId",
                table: "Outboxes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Inboxes_Email",
                table: "Inboxes",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Inboxes_UserId",
                table: "Inboxes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Outboxes_Email",
                table: "Outboxes");

            migrationBuilder.DropIndex(
                name: "IX_Outboxes_UserId",
                table: "Outboxes");

            migrationBuilder.DropIndex(
                name: "IX_Inboxes_Email",
                table: "Inboxes");

            migrationBuilder.DropIndex(
                name: "IX_Inboxes_UserId",
                table: "Inboxes");

            migrationBuilder.DropColumn(
                name: "Extra",
                table: "EmailGroups");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "AppSettings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

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
    }
}
