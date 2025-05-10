using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMail.DB.Migrations.Mysql
{
    /// <inheritdoc />
    public partial class fixManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_EmailTemplates_EmailTemplateId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailTemplates_SendingGroups_SendingGroupId",
                table: "EmailTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_FileUsages_SendingGroups_SendingGroupId",
                table: "FileUsages");

            migrationBuilder.DropForeignKey(
                name: "FK_FileUsages_SendingItems_SendingItemId",
                table: "FileUsages");

            migrationBuilder.DropForeignKey(
                name: "FK_Outboxes_SendingGroups_SendingGroupId",
                table: "Outboxes");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_EmailTemplates_EmailTemplateId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_EmailTemplateId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Outboxes_SendingGroupId",
                table: "Outboxes");

            migrationBuilder.DropIndex(
                name: "IX_FileUsages_SendingGroupId",
                table: "FileUsages");

            migrationBuilder.DropIndex(
                name: "IX_FileUsages_SendingItemId",
                table: "FileUsages");

            migrationBuilder.DropIndex(
                name: "IX_EmailTemplates_SendingGroupId",
                table: "EmailTemplates");

            migrationBuilder.DropIndex(
                name: "IX_Departments_EmailTemplateId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "EmailTemplateId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SendingGroupId",
                table: "Outboxes");

            migrationBuilder.DropColumn(
                name: "SendingGroupId",
                table: "FileUsages");

            migrationBuilder.DropColumn(
                name: "SendingItemId",
                table: "FileUsages");

            migrationBuilder.DropColumn(
                name: "SendingGroupId",
                table: "EmailTemplates");

            migrationBuilder.DropColumn(
                name: "EmailTemplateId",
                table: "Departments");

            migrationBuilder.CreateTable(
                name: "DepartmentEmailTemplate",
                columns: table => new
                {
                    EmailTemplateId = table.Column<long>(type: "bigint", nullable: false),
                    ShareToOrganizationsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentEmailTemplate", x => new { x.EmailTemplateId, x.ShareToOrganizationsId });
                    table.ForeignKey(
                        name: "FK_DepartmentEmailTemplate_Departments_ShareToOrganizationsId",
                        column: x => x.ShareToOrganizationsId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepartmentEmailTemplate_EmailTemplates_EmailTemplateId",
                        column: x => x.EmailTemplateId,
                        principalTable: "EmailTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmailTemplateSendingGroup",
                columns: table => new
                {
                    SendingGroupId = table.Column<long>(type: "bigint", nullable: false),
                    TemplatesId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplateSendingGroup", x => new { x.SendingGroupId, x.TemplatesId });
                    table.ForeignKey(
                        name: "FK_EmailTemplateSendingGroup_EmailTemplates_TemplatesId",
                        column: x => x.TemplatesId,
                        principalTable: "EmailTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmailTemplateSendingGroup_SendingGroups_SendingGroupId",
                        column: x => x.SendingGroupId,
                        principalTable: "SendingGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmailTemplateUser",
                columns: table => new
                {
                    EmailTemplateId = table.Column<long>(type: "bigint", nullable: false),
                    ShareToUsersId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplateUser", x => new { x.EmailTemplateId, x.ShareToUsersId });
                    table.ForeignKey(
                        name: "FK_EmailTemplateUser_EmailTemplates_EmailTemplateId",
                        column: x => x.EmailTemplateId,
                        principalTable: "EmailTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmailTemplateUser_Users_ShareToUsersId",
                        column: x => x.ShareToUsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FileUsageSendingGroup",
                columns: table => new
                {
                    AttachmentsId = table.Column<long>(type: "bigint", nullable: false),
                    SendingGroupId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileUsageSendingGroup", x => new { x.AttachmentsId, x.SendingGroupId });
                    table.ForeignKey(
                        name: "FK_FileUsageSendingGroup_FileUsages_AttachmentsId",
                        column: x => x.AttachmentsId,
                        principalTable: "FileUsages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FileUsageSendingGroup_SendingGroups_SendingGroupId",
                        column: x => x.SendingGroupId,
                        principalTable: "SendingGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FileUsageSendingItem",
                columns: table => new
                {
                    AttachmentsId = table.Column<long>(type: "bigint", nullable: false),
                    SendingItemId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileUsageSendingItem", x => new { x.AttachmentsId, x.SendingItemId });
                    table.ForeignKey(
                        name: "FK_FileUsageSendingItem_FileUsages_AttachmentsId",
                        column: x => x.AttachmentsId,
                        principalTable: "FileUsages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FileUsageSendingItem_SendingItems_SendingItemId",
                        column: x => x.SendingItemId,
                        principalTable: "SendingItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OutboxSendingGroup",
                columns: table => new
                {
                    OutboxesId = table.Column<long>(type: "bigint", nullable: false),
                    SendingGroupId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxSendingGroup", x => new { x.OutboxesId, x.SendingGroupId });
                    table.ForeignKey(
                        name: "FK_OutboxSendingGroup_Outboxes_OutboxesId",
                        column: x => x.OutboxesId,
                        principalTable: "Outboxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OutboxSendingGroup_SendingGroups_SendingGroupId",
                        column: x => x.SendingGroupId,
                        principalTable: "SendingGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentEmailTemplate_ShareToOrganizationsId",
                table: "DepartmentEmailTemplate",
                column: "ShareToOrganizationsId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplateSendingGroup_TemplatesId",
                table: "EmailTemplateSendingGroup",
                column: "TemplatesId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplateUser_ShareToUsersId",
                table: "EmailTemplateUser",
                column: "ShareToUsersId");

            migrationBuilder.CreateIndex(
                name: "IX_FileUsageSendingGroup_SendingGroupId",
                table: "FileUsageSendingGroup",
                column: "SendingGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_FileUsageSendingItem_SendingItemId",
                table: "FileUsageSendingItem",
                column: "SendingItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxSendingGroup_SendingGroupId",
                table: "OutboxSendingGroup",
                column: "SendingGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DepartmentEmailTemplate");

            migrationBuilder.DropTable(
                name: "EmailTemplateSendingGroup");

            migrationBuilder.DropTable(
                name: "EmailTemplateUser");

            migrationBuilder.DropTable(
                name: "FileUsageSendingGroup");

            migrationBuilder.DropTable(
                name: "FileUsageSendingItem");

            migrationBuilder.DropTable(
                name: "OutboxSendingGroup");

            migrationBuilder.AddColumn<long>(
                name: "EmailTemplateId",
                table: "Users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SendingGroupId",
                table: "Outboxes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SendingGroupId",
                table: "FileUsages",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SendingItemId",
                table: "FileUsages",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SendingGroupId",
                table: "EmailTemplates",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "EmailTemplateId",
                table: "Departments",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmailTemplateId",
                table: "Users",
                column: "EmailTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Outboxes_SendingGroupId",
                table: "Outboxes",
                column: "SendingGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_FileUsages_SendingGroupId",
                table: "FileUsages",
                column: "SendingGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_FileUsages_SendingItemId",
                table: "FileUsages",
                column: "SendingItemId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_SendingGroupId",
                table: "EmailTemplates",
                column: "SendingGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_EmailTemplateId",
                table: "Departments",
                column: "EmailTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_EmailTemplates_EmailTemplateId",
                table: "Departments",
                column: "EmailTemplateId",
                principalTable: "EmailTemplates",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailTemplates_SendingGroups_SendingGroupId",
                table: "EmailTemplates",
                column: "SendingGroupId",
                principalTable: "SendingGroups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FileUsages_SendingGroups_SendingGroupId",
                table: "FileUsages",
                column: "SendingGroupId",
                principalTable: "SendingGroups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FileUsages_SendingItems_SendingItemId",
                table: "FileUsages",
                column: "SendingItemId",
                principalTable: "SendingItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Outboxes_SendingGroups_SendingGroupId",
                table: "Outboxes",
                column: "SendingGroupId",
                principalTable: "SendingGroups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_EmailTemplates_EmailTemplateId",
                table: "Users",
                column: "EmailTemplateId",
                principalTable: "EmailTemplates",
                principalColumn: "Id");
        }
    }
}
