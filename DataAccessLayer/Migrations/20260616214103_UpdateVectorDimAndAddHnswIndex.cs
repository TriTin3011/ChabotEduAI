using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVectorDimAndAddHnswIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"DocumentChunks\" SET \"Embedding\" = NULL;");

            migrationBuilder.AlterColumn<Vector>(
                name: "Embedding",
                table: "DocumentChunks",
                type: "vector(768)",
                nullable: true,
                oldClrType: typeof(Vector),
                oldType: "vector(3072)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 1,
                column: "UploadedAt",
                value: new DateTime(2026, 6, 16, 21, 41, 2, 973, DateTimeKind.Utc).AddTicks(2115));

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 2,
                column: "UploadedAt",
                value: new DateTime(2026, 6, 16, 21, 41, 2, 973, DateTimeKind.Utc).AddTicks(2120));

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 3,
                column: "UploadedAt",
                value: new DateTime(2026, 6, 16, 21, 41, 2, 973, DateTimeKind.Utc).AddTicks(2122));

            migrationBuilder.CreateIndex(
                name: "IX_DocumentChunks_Embedding",
                table: "DocumentChunks",
                column: "Embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DocumentChunks_Embedding",
                table: "DocumentChunks");

            migrationBuilder.AlterColumn<Vector>(
                name: "Embedding",
                table: "DocumentChunks",
                type: "vector(3072)",
                nullable: true,
                oldClrType: typeof(Vector),
                oldType: "vector(768)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 1,
                column: "UploadedAt",
                value: new DateTime(2026, 6, 13, 11, 42, 8, 491, DateTimeKind.Utc).AddTicks(5552));

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 2,
                column: "UploadedAt",
                value: new DateTime(2026, 6, 13, 11, 42, 8, 491, DateTimeKind.Utc).AddTicks(5559));

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 3,
                column: "UploadedAt",
                value: new DateTime(2026, 6, 13, 11, 42, 8, 491, DateTimeKind.Utc).AddTicks(5562));
        }
    }
}
