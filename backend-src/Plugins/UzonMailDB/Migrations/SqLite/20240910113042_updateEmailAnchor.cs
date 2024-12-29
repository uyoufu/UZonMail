using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMail.DB.Migrations.SqLite
{
    /// <inheritdoc />
    public partial class updateEmailAnchor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InboxEmail",
                table: "EmailAnchors",
                newName: "InboxEmails");

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "EmailAnchors",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "EmailAnchors");

            migrationBuilder.RenameColumn(
                name: "InboxEmails",
                table: "EmailAnchors",
                newName: "InboxEmail");
        }
    }
}
