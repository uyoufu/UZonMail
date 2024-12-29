using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMail.DB.Migrations.SqLite
{
    /// <inheritdoc />
    public partial class updateUnsusbscribeEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Host",
                table: "UnsubscribeEmails",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnsubscribeEmails_OrganizationId_Email",
                table: "UnsubscribeEmails",
                columns: new[] { "OrganizationId", "Email" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UnsubscribeEmails_OrganizationId_Email",
                table: "UnsubscribeEmails");

            migrationBuilder.DropColumn(
                name: "Host",
                table: "UnsubscribeEmails");
        }
    }
}
