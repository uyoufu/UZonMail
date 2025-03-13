using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMail.DB.Migrations.SqLite
{
    /// <inheritdoc />
    public partial class removePro2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SendingGroupId",
                table: "Outboxes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Outboxes_SendingGroupId",
                table: "Outboxes",
                column: "SendingGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Outboxes_SendingGroups_SendingGroupId",
                table: "Outboxes",
                column: "SendingGroupId",
                principalTable: "SendingGroups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Outboxes_SendingGroups_SendingGroupId",
                table: "Outboxes");

            migrationBuilder.DropIndex(
                name: "IX_Outboxes_SendingGroupId",
                table: "Outboxes");

            migrationBuilder.DropColumn(
                name: "SendingGroupId",
                table: "Outboxes");
        }
    }
}
