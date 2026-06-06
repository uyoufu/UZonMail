using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMail.DB.Migrations.PostgreSQL
{
    /// <inheritdoc />
    public partial class addTypeFieldToOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnableSSL",
                table: "Outboxes");

            migrationBuilder.RenameColumn(
                name: "SecurityProtocol",
                table: "SmtpInfos",
                newName: "ConnectionSecurity");

            migrationBuilder.AddColumn<int>(
                name: "ConnectionSecurity",
                table: "Outboxes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Outboxes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConnectionSecurity",
                table: "Outboxes");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Outboxes");

            migrationBuilder.RenameColumn(
                name: "ConnectionSecurity",
                table: "SmtpInfos",
                newName: "SecurityProtocol");

            migrationBuilder.AddColumn<bool>(
                name: "EnableSSL",
                table: "Outboxes",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
