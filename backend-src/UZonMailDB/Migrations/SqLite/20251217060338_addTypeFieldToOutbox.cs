using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMail.DB.Migrations.SqLite
{
    /// <inheritdoc />
    public partial class addTypeFieldToOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SecurityProtocol",
                table: "SmtpInfos",
                newName: "ConnectionSecurity");

            migrationBuilder.RenameColumn(
                name: "EnableSSL",
                table: "Outboxes",
                newName: "Type");

            migrationBuilder.AddColumn<int>(
                name: "ConnectionSecurity",
                table: "Outboxes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConnectionSecurity",
                table: "Outboxes");

            migrationBuilder.RenameColumn(
                name: "ConnectionSecurity",
                table: "SmtpInfos",
                newName: "SecurityProtocol");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Outboxes",
                newName: "EnableSSL");
        }
    }
}
