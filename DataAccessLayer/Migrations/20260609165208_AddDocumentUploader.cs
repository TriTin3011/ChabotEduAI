using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentUploader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UploaderId",
                table: "Documents",
                type: "integer",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "UploadedAt", "UploaderId" },
                values: new object[] { new DateTime(2026, 6, 9, 16, 52, 8, 273, DateTimeKind.Utc).AddTicks(6145), null });

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "UploadedAt", "UploaderId" },
                values: new object[] { new DateTime(2026, 6, 9, 16, 52, 8, 273, DateTimeKind.Utc).AddTicks(6183), null });

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "UploadedAt", "UploaderId" },
                values: new object[] { new DateTime(2026, 6, 9, 16, 52, 8, 273, DateTimeKind.Utc).AddTicks(6186), null });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_UploaderId",
                table: "Documents",
                column: "UploaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Users_UploaderId",
                table: "Documents",
                column: "UploaderId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Users_UploaderId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_UploaderId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "UploaderId",
                table: "Documents");

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 1,
                column: "UploadedAt",
                value: new DateTime(2026, 6, 9, 16, 36, 30, 985, DateTimeKind.Utc).AddTicks(1295));

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 2,
                column: "UploadedAt",
                value: new DateTime(2026, 6, 9, 16, 36, 30, 985, DateTimeKind.Utc).AddTicks(1300));

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 3,
                column: "UploadedAt",
                value: new DateTime(2026, 6, 9, 16, 36, 30, 985, DateTimeKind.Utc).AddTicks(1303));
        }
    }
}
