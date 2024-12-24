using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMail.DB.Migrations.SqLite
{
    /// <inheritdoc />
    public partial class updateForv011 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DepartmentEmailTemplate",
                columns: table => new
                {
                    EmailTemplateId = table.Column<long>(type: "INTEGER", nullable: false),
                    ShareToOrganizationsId = table.Column<long>(type: "INTEGER", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "EmailTemplateUser",
                columns: table => new
                {
                    EmailTemplateId = table.Column<long>(type: "INTEGER", nullable: false),
                    ShareToUsersId = table.Column<long>(type: "INTEGER", nullable: false)
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
                });

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentEmailTemplate_ShareToOrganizationsId",
                table: "DepartmentEmailTemplate",
                column: "ShareToOrganizationsId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplateUser_ShareToUsersId",
                table: "EmailTemplateUser",
                column: "ShareToUsersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DepartmentEmailTemplate");

            migrationBuilder.DropTable(
                name: "EmailTemplateUser");
        }
    }
}
