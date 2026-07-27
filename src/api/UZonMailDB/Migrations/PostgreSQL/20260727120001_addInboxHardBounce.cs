using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using UzonMail.DB.PostgreSql;

namespace UzonMail.DB.Migrations.PostgreSQL;

[DbContext(typeof(PostgreSqlContext))]
[Migration("20260727120001_addInboxHardBounce")]
public sealed class addInboxHardBounce : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AddColumn<bool>(
            "IsHardBounce",
            "SendingItems",
            "boolean",
            nullable: false,
            defaultValue: false
        );

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropColumn("IsHardBounce", "SendingItems");
}
