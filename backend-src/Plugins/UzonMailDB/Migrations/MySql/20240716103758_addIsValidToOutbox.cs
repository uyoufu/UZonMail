using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMailService.Migrations.Mysql
{
    /// <inheritdoc />
    public partial class addIsValidToOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsValid",
                table: "Outboxes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ValidFailReason",
                table: "Outboxes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsValid",
                table: "Outboxes");

            migrationBuilder.DropColumn(
                name: "ValidFailReason",
                table: "Outboxes");
        }
    }
}
