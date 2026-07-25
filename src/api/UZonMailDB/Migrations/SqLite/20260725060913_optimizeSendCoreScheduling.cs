using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UzonMail.DB.Migrations.SqLite
{
    /// <inheritdoc />
    public partial class optimizeSendCoreScheduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ResumeAtUtc",
                table: "SendingGroups",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusReason",
                table: "SendingGroups",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "SentCountDateUtc",
                table: "Outboxes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SendingGroups_Status_ResumeAtUtc_Id",
                table: "SendingGroups",
                columns: new[] { "Status", "ResumeAtUtc", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Outboxes_UserId_EmailGroupId_IsValid_Id",
                table: "Outboxes",
                columns: new[] { "UserId", "EmailGroupId", "IsValid", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SendingGroups_Status_ResumeAtUtc_Id",
                table: "SendingGroups");

            migrationBuilder.DropIndex(
                name: "IX_Outboxes_UserId_EmailGroupId_IsValid_Id",
                table: "Outboxes");

            migrationBuilder.DropColumn(
                name: "ResumeAtUtc",
                table: "SendingGroups");

            migrationBuilder.DropColumn(
                name: "StatusReason",
                table: "SendingGroups");

            migrationBuilder.DropColumn(
                name: "SentCountDateUtc",
                table: "Outboxes");
        }
    }
}
