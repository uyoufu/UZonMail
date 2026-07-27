using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using UzonMail.DB.SqLite;

namespace UzonMail.DB.Migrations.SqLite;

[DbContext(typeof(SqLiteContext))]
[Migration("20260727120000_addInboxHardBounce")]
public sealed class addInboxHardBounce : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AddColumn<bool>(
            "IsHardBounce",
            "SendingItems",
            "INTEGER",
            nullable: false,
            defaultValue: false
        );

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropColumn("IsHardBounce", "SendingItems");
}
