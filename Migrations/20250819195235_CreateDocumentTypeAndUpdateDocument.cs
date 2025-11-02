using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoEdge.Migrations
{
    /// <inheritdoc />
    public partial class CreateDocumentTypeAndUpdateDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_AspNetUsers_VerifiedByUserId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_ExpiryDate",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_VerificationStatus",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "MimeType",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "VerificationStatus",
                table: "Documents");

            migrationBuilder.RenameColumn(
                name: "VerifiedByUserId",
                table: "Documents",
                newName: "ReviewedBy");

            migrationBuilder.RenameColumn(
                name: "VerificationDate",
                table: "Documents",
                newName: "ReviewDate");

            migrationBuilder.RenameColumn(
                name: "DocumentType",
                table: "Documents",
                newName: "Status");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_VerifiedByUserId",
                table: "Documents",
                newName: "IX_Documents_ReviewedBy");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_DocumentType",
                table: "Documents",
                newName: "IX_Documents_Status");

            migrationBuilder.AlterColumn<string>(
                name: "ExtractedText",
                table: "Documents",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "Documents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Documents",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DocumentTypeId",
                table: "Documents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsOcrProcessed",
                table: "Documents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "Documents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OcrProcessedDate",
                table: "Documents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OcrValidationResults",
                table: "Documents",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValidationNotes",
                table: "Documents",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AllowedFileTypes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MaxFileSizeMB = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DocumentTypeId",
                table: "Documents",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_IsActive",
                table: "Documents",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_UploadDate",
                table: "Documents",
                column: "UploadDate");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_IsActive",
                table: "DocumentTypes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_IsRequired",
                table: "DocumentTypes",
                column: "IsRequired");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_Name",
                table: "DocumentTypes",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_AspNetUsers_ReviewedBy",
                table: "Documents",
                column: "ReviewedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_DocumentTypes_DocumentTypeId",
                table: "Documents",
                column: "DocumentTypeId",
                principalTable: "DocumentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_AspNetUsers_ReviewedBy",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_DocumentTypes_DocumentTypeId",
                table: "Documents");

            migrationBuilder.DropTable(
                name: "DocumentTypes");

            migrationBuilder.DropIndex(
                name: "IX_Documents_DocumentTypeId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_IsActive",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_UploadDate",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "DocumentTypeId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "IsOcrProcessed",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "OcrProcessedDate",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "OcrValidationResults",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ValidationNotes",
                table: "Documents");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Documents",
                newName: "DocumentType");

            migrationBuilder.RenameColumn(
                name: "ReviewedBy",
                table: "Documents",
                newName: "VerifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "ReviewDate",
                table: "Documents",
                newName: "VerificationDate");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_Status",
                table: "Documents",
                newName: "IX_Documents_DocumentType");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_ReviewedBy",
                table: "Documents",
                newName: "IX_Documents_VerifiedByUserId");

            migrationBuilder.AlterColumn<string>(
                name: "ExtractedText",
                table: "Documents",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MimeType",
                table: "Documents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Documents",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Documents",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VerificationStatus",
                table: "Documents",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ExpiryDate",
                table: "Documents",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_VerificationStatus",
                table: "Documents",
                column: "VerificationStatus");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_AspNetUsers_VerifiedByUserId",
                table: "Documents",
                column: "VerifiedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
