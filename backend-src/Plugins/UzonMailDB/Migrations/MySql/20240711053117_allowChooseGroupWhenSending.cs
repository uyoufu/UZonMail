using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMailService.Migrations.Mysql
{
    /// <inheritdoc />
    public partial class allowChooseGroupWhenSending : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InboxGroups",
                table: "SendingGroups",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "InboxesCount",
                table: "SendingGroups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OutboxGroups",
                table: "SendingGroups",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "OutboxesCount",
                table: "SendingGroups",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InboxGroups",
                table: "SendingGroups");

            migrationBuilder.DropColumn(
                name: "InboxesCount",
                table: "SendingGroups");

            migrationBuilder.DropColumn(
                name: "OutboxGroups",
                table: "SendingGroups");

            migrationBuilder.DropColumn(
                name: "OutboxesCount",
                table: "SendingGroups");
        }
    }
}
