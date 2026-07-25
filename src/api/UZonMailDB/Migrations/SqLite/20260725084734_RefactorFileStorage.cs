using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UzonMail.DB.Migrations.SqLite
{
    /// <inheritdoc />
    public partial class RefactorFileStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_FileUsages_OwnerUserId", table: "FileUsages");

            migrationBuilder.RenameColumn(
                name: "UniqueName",
                table: "FileUsages",
                newName: "DisplayNameKey"
            );

            migrationBuilder.RenameColumn(
                name: "LinkCount",
                table: "FileObjects",
                newName: "StorageState"
            );

            migrationBuilder.AddColumn<long>(
                name: "CategoryId",
                table: "FileUsages",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L
            );

            migrationBuilder.AddColumn<long>(
                name: "ReferenceCount",
                table: "FileUsages",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L
            );

            migrationBuilder.CreateTable(
                name: "FileCategories",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OwnerUserId = table.Column<long>(type: "INTEGER", nullable: false),
                    ParentId = table.Column<long>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Sort = table.Column<long>(type: "INTEGER", nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileCategories_FileCategories_ParentId",
                        column: x => x.ParentId,
                        principalTable: "FileCategories",
                        principalColumn: "Id"
                    );
                    table.ForeignKey(
                        name: "FK_FileCategories_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "Users",
                        principalColumn: "Id"
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_FileUsages_CategoryId",
                table: "FileUsages",
                column: "CategoryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_FileUsages_OwnerUserId_CategoryId_CreateDate",
                table: "FileUsages",
                columns: new[] { "OwnerUserId", "CategoryId", "CreateDate" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_FileUsages_OwnerUserId_DisplayNameKey",
                table: "FileUsages",
                columns: new[] { "OwnerUserId", "DisplayNameKey" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_FileUsages_OwnerUserId_FileObjectId",
                table: "FileUsages",
                columns: new[] { "OwnerUserId", "FileObjectId" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_FileObjects_Sha256",
                table: "FileObjects",
                column: "Sha256",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_FileCategories_OwnerUserId_ParentId_Sort",
                table: "FileCategories",
                columns: new[] { "OwnerUserId", "ParentId", "Sort" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_FileCategories_ParentId",
                table: "FileCategories",
                column: "ParentId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_FileUsages_FileCategories_CategoryId",
                table: "FileUsages",
                column: "CategoryId",
                principalTable: "FileCategories",
                principalColumn: "Id"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileUsages_FileCategories_CategoryId",
                table: "FileUsages"
            );

            migrationBuilder.DropTable(name: "FileCategories");

            migrationBuilder.DropIndex(name: "IX_FileUsages_CategoryId", table: "FileUsages");

            migrationBuilder.DropIndex(
                name: "IX_FileUsages_OwnerUserId_CategoryId_CreateDate",
                table: "FileUsages"
            );

            migrationBuilder.DropIndex(
                name: "IX_FileUsages_OwnerUserId_DisplayNameKey",
                table: "FileUsages"
            );

            migrationBuilder.DropIndex(
                name: "IX_FileUsages_OwnerUserId_FileObjectId",
                table: "FileUsages"
            );

            migrationBuilder.DropIndex(name: "IX_FileObjects_Sha256", table: "FileObjects");

            migrationBuilder.DropColumn(name: "CategoryId", table: "FileUsages");

            migrationBuilder.DropColumn(name: "ReferenceCount", table: "FileUsages");

            migrationBuilder.RenameColumn(
                name: "DisplayNameKey",
                table: "FileUsages",
                newName: "UniqueName"
            );

            migrationBuilder.RenameColumn(
                name: "StorageState",
                table: "FileObjects",
                newName: "LinkCount"
            );

            migrationBuilder.CreateIndex(
                name: "IX_FileUsages_OwnerUserId",
                table: "FileUsages",
                column: "OwnerUserId"
            );
        }
    }
}
