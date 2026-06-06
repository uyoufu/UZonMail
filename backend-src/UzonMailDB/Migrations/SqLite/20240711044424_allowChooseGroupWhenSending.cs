using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMailService.Migrations.SqLite
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
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InboxesCount",
                table: "SendingGroups",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OutboxGroups",
                table: "SendingGroups",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OutboxesCount",
                table: "SendingGroups",
                type: "INTEGER",
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
