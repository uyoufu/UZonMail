using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMail.DB.Migrations.Mysql
{
    /// <inheritdoc />
    public partial class setExtraFieldNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Extra",
                table: "EmailGroups",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "EmailGroups",
                keyColumn: "Extra",
                keyValue: null,
                column: "Extra",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Extra",
                table: "EmailGroups",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
