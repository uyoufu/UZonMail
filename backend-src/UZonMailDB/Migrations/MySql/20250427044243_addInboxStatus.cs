using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMail.DB.Migrations.Mysql
{
    /// <inheritdoc />
    public partial class addInboxStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Inboxes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValidFailReason",
                table: "Inboxes",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Inboxes");

            migrationBuilder.DropColumn(
                name: "ValidFailReason",
                table: "Inboxes");
        }
    }
}
