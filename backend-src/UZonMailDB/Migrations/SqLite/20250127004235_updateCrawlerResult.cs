using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMail.DB.Migrations.SqLite
{
    /// <inheritdoc />
    public partial class updateCrawlerResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAttachingInbox",
                table: "CrawlerTaskResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "OutboxGroupId",
                table: "CrawlerTaskInfos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_CrawlerTaskResults_TikTokAuthorId",
                table: "CrawlerTaskResults",
                column: "TikTokAuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_CrawlerTaskResults_TiktokAuthors_TikTokAuthorId",
                table: "CrawlerTaskResults",
                column: "TikTokAuthorId",
                principalTable: "TiktokAuthors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CrawlerTaskResults_TiktokAuthors_TikTokAuthorId",
                table: "CrawlerTaskResults");

            migrationBuilder.DropIndex(
                name: "IX_CrawlerTaskResults_TikTokAuthorId",
                table: "CrawlerTaskResults");

            migrationBuilder.DropColumn(
                name: "IsAttachingInbox",
                table: "CrawlerTaskResults");

            migrationBuilder.DropColumn(
                name: "OutboxGroupId",
                table: "CrawlerTaskInfos");
        }
    }
}
