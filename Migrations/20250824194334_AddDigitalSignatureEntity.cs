using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoEdge.Migrations
{
    /// <inheritdoc />
    public partial class AddDigitalSignatureEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DigitalSignatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    SignerName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SignerEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SignatureData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SignedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SignatureType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DocumentHash = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalSignatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DigitalSignatures_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DigitalSignatures_ContractId",
                table: "DigitalSignatures",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalSignatures_CreatedDate",
                table: "DigitalSignatures",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalSignatures_IsActive",
                table: "DigitalSignatures",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalSignatures_SignedDate",
                table: "DigitalSignatures",
                column: "SignedDate");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalSignatures_SignerEmail",
                table: "DigitalSignatures",
                column: "SignerEmail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DigitalSignatures");
        }
    }
}
