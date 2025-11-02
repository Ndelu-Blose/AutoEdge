using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoEdge.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PurchaseId",
                table: "Documents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PurchaseId",
                table: "Contracts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Purchases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DepositAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RemainingAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FinancingProvider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InterestRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    LoanTermMonths = table.Column<int>(type: "int", nullable: true),
                    MonthlyPayment = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    InitiatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DocumentsSubmittedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DocumentsVerifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ContractGeneratedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ContractSignedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentCompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AssignedSalesRep = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RequiresFinancing = table.Column<bool>(type: "bit", nullable: false),
                    DocumentsVerified = table.Column<bool>(type: "bit", nullable: false),
                    ContractSigned = table.Column<bool>(type: "bit", nullable: false),
                    PaymentCompleted = table.Column<bool>(type: "bit", nullable: false),
                    DeliveryMethod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ScheduledDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Purchases_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseStatusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseId = table.Column<int>(type: "int", nullable: false),
                    FromStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ToStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ChangeReason = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSystemGenerated = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseStatusHistories_Purchases_PurchaseId",
                        column: x => x.PurchaseId,
                        principalTable: "Purchases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_PurchaseId",
                table: "Documents",
                column: "PurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_PurchaseId",
                table: "Contracts",
                column: "PurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_CustomerId",
                table: "Purchases",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_InitiatedDate",
                table: "Purchases",
                column: "InitiatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_IsActive",
                table: "Purchases",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_Status",
                table: "Purchases",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_VehicleId",
                table: "Purchases",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseStatusHistories_ChangedDate",
                table: "PurchaseStatusHistories",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseStatusHistories_PurchaseId",
                table: "PurchaseStatusHistories",
                column: "PurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseStatusHistories_PurchaseId_ChangedDate",
                table: "PurchaseStatusHistories",
                columns: new[] { "PurchaseId", "ChangedDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Purchases_PurchaseId",
                table: "Contracts",
                column: "PurchaseId",
                principalTable: "Purchases",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Purchases_PurchaseId",
                table: "Documents",
                column: "PurchaseId",
                principalTable: "Purchases",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Purchases_PurchaseId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Purchases_PurchaseId",
                table: "Documents");

            migrationBuilder.DropTable(
                name: "PurchaseStatusHistories");

            migrationBuilder.DropTable(
                name: "Purchases");

            migrationBuilder.DropIndex(
                name: "IX_Documents_PurchaseId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_PurchaseId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "PurchaseId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "PurchaseId",
                table: "Contracts");
        }
    }
}
