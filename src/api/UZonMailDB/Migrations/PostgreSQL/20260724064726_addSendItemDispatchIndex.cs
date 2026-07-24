using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UzonMail.DB.Migrations.PostgreSQL
{
    /// <inheritdoc />
    public partial class addSendItemDispatchIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SendingItems_SendingGroupId",
                table: "SendingItems"
            );

            migrationBuilder.CreateIndex(
                name: "IX_SendingItems_Group_Status_Outbox_Id",
                table: "SendingItems",
                columns: new[] { "SendingGroupId", "Status", "OutBoxId", "Id" }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SendingItems_Group_Status_Outbox_Id",
                table: "SendingItems"
            );

            migrationBuilder.CreateIndex(
                name: "IX_SendingItems_SendingGroupId",
                table: "SendingItems",
                column: "SendingGroupId"
            );
        }
    }
}
