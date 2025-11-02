using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoEdge.Migrations
{
    /// <inheritdoc />
    public partial class AddOpenSignFieldsToContract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OpenSignDocumentId",
                table: "Contracts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignedDocumentUrl",
                table: "Contracts",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SigningCompletedDate",
                table: "Contracts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SigningRequestSentDate",
                table: "Contracts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SigningStatus",
                table: "Contracts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SigningUrl",
                table: "Contracts",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OpenSignDocumentId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "SignedDocumentUrl",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "SigningCompletedDate",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "SigningRequestSentDate",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "SigningStatus",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "SigningUrl",
                table: "Contracts");
        }
    }
}
