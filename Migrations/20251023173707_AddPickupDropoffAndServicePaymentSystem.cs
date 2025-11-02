using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoEdge.Migrations
{
    /// <inheritdoc />
    public partial class AddPickupDropoffAndServicePaymentSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ServiceBookings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CustomerComments",
                table: "ServiceBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomerRating",
                table: "ServiceBookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RatedAt",
                table: "ServiceBookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ServiceBookings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Drivers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LicenseNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VehicleRegistration = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VehicleMake = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VehicleModel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VehicleColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    Rating = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    TotalPickups = table.Column<int>(type: "int", nullable: false),
                    SuccessfulPickups = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Drivers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceExecutions",
                columns: table => new
                {
                    ServiceExecutionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceBookingId = table.Column<int>(type: "int", nullable: false),
                    TechnicianId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TasksCompleted = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PartsUsed = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    LaborHours = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    LaborRate = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    AdditionalIssues = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AdditionalWorkApproved = table.Column<bool>(type: "bit", nullable: false),
                    QualityCheckPassed = table.Column<bool>(type: "bit", nullable: false),
                    TestDriveNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TotalCost = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    AdvisorApproval = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExecutionStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceExecutions", x => x.ServiceExecutionId);
                    table.ForeignKey(
                        name: "FK_ServiceExecutions_AspNetUsers_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServiceExecutions_AspNetUsers_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceExecutions_ServiceBookings_ServiceBookingId",
                        column: x => x.ServiceBookingId,
                        principalTable: "ServiceBookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceInvoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceBookingId = table.Column<int>(type: "int", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PDFPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceInvoices_ServiceBookings_ServiceBookingId",
                        column: x => x.ServiceBookingId,
                        principalTable: "ServiceBookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehiclePickups",
                columns: table => new
                {
                    VehiclePickupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceBookingId = table.Column<int>(type: "int", nullable: false),
                    DriverId = table.Column<int>(type: "int", nullable: false),
                    PickupLocation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PickupDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PickupTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    DropoffDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DropoffTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    PickupStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VehicleConditionPickup = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    VehicleConditionDropoff = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    MileagePickup = table.Column<int>(type: "int", nullable: true),
                    MileageDropoff = table.Column<int>(type: "int", nullable: true),
                    FuelLevelPickup = table.Column<int>(type: "int", nullable: true),
                    FuelLevelDropoff = table.Column<int>(type: "int", nullable: true),
                    PickupPhotos = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DropoffPhotos = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CustomerSignatures = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehiclePickups", x => x.VehiclePickupId);
                    table.ForeignKey(
                        name: "FK_VehiclePickups_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VehiclePickups_ServiceBookings_ServiceBookingId",
                        column: x => x.ServiceBookingId,
                        principalTable: "ServiceBookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServicePayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceInvoiceId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StripePaymentIntentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServicePayments_ServiceInvoices_ServiceInvoiceId",
                        column: x => x.ServiceInvoiceId,
                        principalTable: "ServiceInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBookings_CustomerId",
                table: "ServiceBookings",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_UserId",
                table: "Drivers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceExecutions_ApprovedBy",
                table: "ServiceExecutions",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceExecutions_ServiceBookingId",
                table: "ServiceExecutions",
                column: "ServiceBookingId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceExecutions_TechnicianId",
                table: "ServiceExecutions",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceInvoices_ServiceBookingId",
                table: "ServiceInvoices",
                column: "ServiceBookingId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePayments_ServiceInvoiceId",
                table: "ServicePayments",
                column: "ServiceInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_VehiclePickups_DriverId",
                table: "VehiclePickups",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_VehiclePickups_ServiceBookingId",
                table: "VehiclePickups",
                column: "ServiceBookingId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceBookings_AspNetUsers_CustomerId",
                table: "ServiceBookings",
                column: "CustomerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceBookings_AspNetUsers_CustomerId",
                table: "ServiceBookings");

            migrationBuilder.DropTable(
                name: "ServiceExecutions");

            migrationBuilder.DropTable(
                name: "ServicePayments");

            migrationBuilder.DropTable(
                name: "VehiclePickups");

            migrationBuilder.DropTable(
                name: "ServiceInvoices");

            migrationBuilder.DropTable(
                name: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_ServiceBookings_CustomerId",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "CustomerComments",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "CustomerRating",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "RatedAt",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ServiceBookings");
        }
    }
}
