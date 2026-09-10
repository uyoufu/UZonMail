using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UzonMail.DB.Migrations.SqLite
{
    /// <inheritdoc />
    public partial class AddIncomingMailPreviewText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreviewText",
                table: "IncomingMailMessages",
                type: "TEXT",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviewText",
                table: "IncomingMailMessages");
        }
    }
}
