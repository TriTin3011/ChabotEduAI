using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectAndChapter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChapterId",
                table: "Documents",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileUrl",
                table: "Documents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubjectId",
                table: "Documents",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chapters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubjectId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chapters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chapters_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ChapterId", "FileUrl", "SubjectId", "UploadedAt" },
                values: new object[] { 3, "/files/ai101.pdf", 2, new DateTime(2026, 6, 9, 15, 56, 46, 989, DateTimeKind.Utc).AddTicks(4943) });

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ChapterId", "FileUrl", "SubjectId", "UploadedAt" },
                values: new object[] { 1, "/files/prn222_btl.docx", 1, new DateTime(2026, 6, 9, 15, 56, 46, 989, DateTimeKind.Utc).AddTicks(4949) });

            migrationBuilder.InsertData(
                table: "Subjects",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { 1, "PRN222", "C# Nâng cao" },
                    { 2, "AI101", "Nhập môn AI" }
                });

            migrationBuilder.InsertData(
                table: "Chapters",
                columns: new[] { "Id", "OrderIndex", "SubjectId", "Title" },
                values: new object[,]
                {
                    { 1, 1, 1, "Chương 1: .NET Core và C# Nâng cao" },
                    { 2, 2, 1, "Chương 2: Entity Framework Core" },
                    { 3, 1, 2, "Chương 1: Tổng quan AI" }
                });

            migrationBuilder.InsertData(
                table: "Documents",
                columns: new[] { "Id", "ChapterId", "Content", "FileType", "FileUrl", "Status", "SubjectId", "Title", "UploadedAt" },
                values: new object[] { 3, 2, "Slide bài giảng Entity Framework Core...", "pptx", "/files/prn222_efcore.pptx", "Indexed", 1, "Slide EF Core", new DateTime(2026, 6, 9, 15, 56, 46, 989, DateTimeKind.Utc).AddTicks(4952) });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ChapterId",
                table: "Documents",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_SubjectId",
                table: "Documents",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_SubjectId",
                table: "Chapters",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Chapters_ChapterId",
                table: "Documents",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Subjects_SubjectId",
                table: "Documents",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Chapters_ChapterId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Subjects_SubjectId",
                table: "Documents");

            migrationBuilder.DropTable(
                name: "Chapters");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Documents_ChapterId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_SubjectId",
                table: "Documents");

            migrationBuilder.DeleteData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "ChapterId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "FileUrl",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "SubjectId",
                table: "Documents");

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 1,
                column: "UploadedAt",
                value: new DateTime(2026, 6, 9, 15, 30, 50, 391, DateTimeKind.Utc).AddTicks(2600));

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 2,
                column: "UploadedAt",
                value: new DateTime(2026, 6, 9, 15, 30, 50, 391, DateTimeKind.Utc).AddTicks(2606));
        }
    }
}
