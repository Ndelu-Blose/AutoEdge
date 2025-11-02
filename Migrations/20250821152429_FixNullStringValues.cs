using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoEdge.Migrations
{
    /// <inheritdoc />
    public partial class FixNullStringValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix NULL string values in existing data before adding new column
            migrationBuilder.Sql(@"
                UPDATE Vehicles SET Make = '' WHERE Make IS NULL;
                UPDATE Vehicles SET Model = '' WHERE Model IS NULL;
                UPDATE Vehicles SET EngineType = '' WHERE EngineType IS NULL;
                UPDATE Vehicles SET Transmission = '' WHERE Transmission IS NULL;
                UPDATE Vehicles SET InteriorColor = '' WHERE InteriorColor IS NULL;
                UPDATE Vehicles SET ExteriorColor = '' WHERE ExteriorColor IS NULL;
                UPDATE Vehicles SET Condition = '' WHERE Condition IS NULL;
                UPDATE Vehicles SET Features = '' WHERE Features IS NULL;
                UPDATE Vehicles SET Status = '' WHERE Status IS NULL;
                UPDATE Vehicles SET Location = '' WHERE Location IS NULL;
                UPDATE Vehicles SET Source = '' WHERE Source IS NULL;
                UPDATE Vehicles SET CreatedBy = '' WHERE CreatedBy IS NULL;
                UPDATE Vehicles SET VIN = '' WHERE VIN IS NULL;
            ");

            migrationBuilder.AddColumn<string>(
                name: "LicensePlate",
                table: "Vehicles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CustomerFeedbacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    ServiceRating = table.Column<int>(type: "int", nullable: false),
                    VehicleRating = table.Column<int>(type: "int", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RecommendToOthers = table.Column<bool>(type: "bit", nullable: false),
                    ImprovementSuggestions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AllowFollowUp = table.Column<bool>(type: "bit", nullable: false),
                    SubmittedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ContractId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerFeedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerFeedbacks_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerFeedbacks_Contracts_ContractId1",
                        column: x => x.ContractId1,
                        principalTable: "Contracts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ServiceReminders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    ServiceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Mileage = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ServiceProvider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ServiceLocation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EstimatedCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ActualCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastReminderSent = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RemindersSent = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceReminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceReminders_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Warranties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    WarrantyType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DurationMonths = table.Column<int>(type: "int", nullable: false),
                    DurationKilometers = table.Column<int>(type: "int", nullable: false),
                    CoverageDetails = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Terms = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContractId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warranties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Warranties_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Warranties_Contracts_ContractId1",
                        column: x => x.ContractId1,
                        principalTable: "Contracts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFeedbacks_ContractId",
                table: "CustomerFeedbacks",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFeedbacks_ContractId1",
                table: "CustomerFeedbacks",
                column: "ContractId1");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFeedbacks_IsActive",
                table: "CustomerFeedbacks",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFeedbacks_SubmittedDate",
                table: "CustomerFeedbacks",
                column: "SubmittedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceReminders_ContractId",
                table: "ServiceReminders",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceReminders_CreatedDate",
                table: "ServiceReminders",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceReminders_IsActive",
                table: "ServiceReminders",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceReminders_ScheduledDate",
                table: "ServiceReminders",
                column: "ScheduledDate");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceReminders_ServiceType",
                table: "ServiceReminders",
                column: "ServiceType");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceReminders_Status",
                table: "ServiceReminders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_ContractId",
                table: "Warranties",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_ContractId1",
                table: "Warranties",
                column: "ContractId1");

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_EndDate",
                table: "Warranties",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_IsActive",
                table: "Warranties",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_StartDate",
                table: "Warranties",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_Status",
                table: "Warranties",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_WarrantyType",
                table: "Warranties",
                column: "WarrantyType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerFeedbacks");

            migrationBuilder.DropTable(
                name: "ServiceReminders");

            migrationBuilder.DropTable(
                name: "Warranties");

            migrationBuilder.DropColumn(
                name: "LicensePlate",
                table: "Vehicles");
        }
    }
}
