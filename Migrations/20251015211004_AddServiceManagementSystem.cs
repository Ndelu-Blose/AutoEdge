using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoEdge.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceManagementSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceBookings",
                columns: table => new
                {
                    ServiceBookingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    VehicleMake = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VehicleModel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VehicleYear = table.Column<int>(type: "int", nullable: false),
                    VehicleVin = table.Column<string>(type: "nvarchar(17)", maxLength: 17, nullable: true),
                    VehicleMileage = table.Column<int>(type: "int", nullable: true),
                    ServiceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BookingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BookingTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    ServiceNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EstimatedDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    EstimatedCostMin = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    EstimatedCostMax = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    BookingStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceBookings", x => x.ServiceBookingId);
                    table.ForeignKey(
                        name: "FK_ServiceBookings_AspNetUsers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    LaborHours = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    LaborRate = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    AdditionalIssues = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AdditionalWorkApproved = table.Column<bool>(type: "bit", nullable: false),
                    QualityCheckPassed = table.Column<bool>(type: "bit", nullable: false),
                    TestDriveNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TotalCost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceExecutions_AspNetUsers_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceExecutions_ServiceBookings_ServiceBookingId",
                        column: x => x.ServiceBookingId,
                        principalTable: "ServiceBookings",
                        principalColumn: "ServiceBookingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceSchedules",
                columns: table => new
                {
                    ServiceScheduleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceBookingId = table.Column<int>(type: "int", nullable: false),
                    MechanicId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    ServiceBay = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EstimatedCompletion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScheduleStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceSchedules", x => x.ServiceScheduleId);
                    table.ForeignKey(
                        name: "FK_ServiceSchedules_AspNetUsers_MechanicId",
                        column: x => x.MechanicId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceSchedules_ServiceBookings_ServiceBookingId",
                        column: x => x.ServiceBookingId,
                        principalTable: "ServiceBookings",
                        principalColumn: "ServiceBookingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleInspections",
                columns: table => new
                {
                    VehicleInspectionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceBookingId = table.Column<int>(type: "int", nullable: false),
                    InspectorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CheckInTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OdometerReading = table.Column<int>(type: "int", nullable: true),
                    ExteriorCondition = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    InteriorCondition = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DamageDocumentation = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FluidLevels = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FuelLevel = table.Column<int>(type: "int", nullable: true),
                    RequiredMaintenance = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    InspectionPhotos = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    InspectionReportUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InspectionStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleInspections", x => x.VehicleInspectionId);
                    table.ForeignKey(
                        name: "FK_VehicleInspections_AspNetUsers_InspectorId",
                        column: x => x.InspectorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VehicleInspections_ServiceBookings_ServiceBookingId",
                        column: x => x.ServiceBookingId,
                        principalTable: "ServiceBookings",
                        principalColumn: "ServiceBookingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehiclePickups",
                columns: table => new
                {
                    VehiclePickupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceBookingId = table.Column<int>(type: "int", nullable: false),
                    DriverId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
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
                        name: "FK_VehiclePickups_AspNetUsers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VehiclePickups_ServiceBookings_ServiceBookingId",
                        column: x => x.ServiceBookingId,
                        principalTable: "ServiceBookings",
                        principalColumn: "ServiceBookingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceInvoices",
                columns: table => new
                {
                    ServiceInvoiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceBookingId = table.Column<int>(type: "int", nullable: false),
                    ServiceExecutionId = table.Column<int>(type: "int", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PartsCost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    LaborCost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    AdditionalFees = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvoiceStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceInvoices", x => x.ServiceInvoiceId);
                    table.ForeignKey(
                        name: "FK_ServiceInvoices_ServiceBookings_ServiceBookingId",
                        column: x => x.ServiceBookingId,
                        principalTable: "ServiceBookings",
                        principalColumn: "ServiceBookingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceInvoices_ServiceExecutions_ServiceExecutionId",
                        column: x => x.ServiceExecutionId,
                        principalTable: "ServiceExecutions",
                        principalColumn: "ServiceExecutionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServicePayments",
                columns: table => new
                {
                    ServicePaymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceBookingId = table.Column<int>(type: "int", nullable: false),
                    ServiceInvoiceId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    PaymentAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaymentGatewayResponse = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ProofOfPaymentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReceiptUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePayments", x => x.ServicePaymentId);
                    table.ForeignKey(
                        name: "FK_ServicePayments_AspNetUsers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServicePayments_ServiceBookings_ServiceBookingId",
                        column: x => x.ServiceBookingId,
                        principalTable: "ServiceBookings",
                        principalColumn: "ServiceBookingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServicePayments_ServiceInvoices_ServiceInvoiceId",
                        column: x => x.ServiceInvoiceId,
                        principalTable: "ServiceInvoices",
                        principalColumn: "ServiceInvoiceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBookings_BookingDate",
                table: "ServiceBookings",
                column: "BookingDate");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBookings_BookingStatus",
                table: "ServiceBookings",
                column: "BookingStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBookings_CustomerId",
                table: "ServiceBookings",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBookings_ReferenceNumber",
                table: "ServiceBookings",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceExecutions_ApprovedBy",
                table: "ServiceExecutions",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceExecutions_ExecutionStatus",
                table: "ServiceExecutions",
                column: "ExecutionStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceExecutions_ServiceBookingId",
                table: "ServiceExecutions",
                column: "ServiceBookingId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceExecutions_StartTime",
                table: "ServiceExecutions",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceExecutions_TechnicianId",
                table: "ServiceExecutions",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceInvoices_InvoiceNumber",
                table: "ServiceInvoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceInvoices_InvoiceStatus",
                table: "ServiceInvoices",
                column: "InvoiceStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceInvoices_ServiceBookingId",
                table: "ServiceInvoices",
                column: "ServiceBookingId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceInvoices_ServiceExecutionId",
                table: "ServiceInvoices",
                column: "ServiceExecutionId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePayments_CustomerId",
                table: "ServicePayments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePayments_PaymentStatus",
                table: "ServicePayments",
                column: "PaymentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePayments_ServiceBookingId",
                table: "ServicePayments",
                column: "ServiceBookingId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePayments_ServiceInvoiceId",
                table: "ServicePayments",
                column: "ServiceInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePayments_TransactionId",
                table: "ServicePayments",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceSchedules_MechanicId",
                table: "ServiceSchedules",
                column: "MechanicId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceSchedules_ScheduledDate",
                table: "ServiceSchedules",
                column: "ScheduledDate");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceSchedules_ScheduleStatus",
                table: "ServiceSchedules",
                column: "ScheduleStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceSchedules_ServiceBookingId",
                table: "ServiceSchedules",
                column: "ServiceBookingId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleInspections_CheckInTime",
                table: "VehicleInspections",
                column: "CheckInTime");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleInspections_InspectionStatus",
                table: "VehicleInspections",
                column: "InspectionStatus");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleInspections_InspectorId",
                table: "VehicleInspections",
                column: "InspectorId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleInspections_ServiceBookingId",
                table: "VehicleInspections",
                column: "ServiceBookingId");

            migrationBuilder.CreateIndex(
                name: "IX_VehiclePickups_DriverId",
                table: "VehiclePickups",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_VehiclePickups_PickupDate",
                table: "VehiclePickups",
                column: "PickupDate");

            migrationBuilder.CreateIndex(
                name: "IX_VehiclePickups_PickupStatus",
                table: "VehiclePickups",
                column: "PickupStatus");

            migrationBuilder.CreateIndex(
                name: "IX_VehiclePickups_ServiceBookingId",
                table: "VehiclePickups",
                column: "ServiceBookingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServicePayments");

            migrationBuilder.DropTable(
                name: "ServiceSchedules");

            migrationBuilder.DropTable(
                name: "VehicleInspections");

            migrationBuilder.DropTable(
                name: "VehiclePickups");

            migrationBuilder.DropTable(
                name: "ServiceInvoices");

            migrationBuilder.DropTable(
                name: "ServiceExecutions");

            migrationBuilder.DropTable(
                name: "ServiceBookings");
        }
    }
}
