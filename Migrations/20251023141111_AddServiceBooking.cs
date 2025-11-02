using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AutoEdge.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceBookings_AspNetUsers_CustomerId",
                table: "ServiceBookings");

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

            migrationBuilder.DropIndex(
                name: "IX_ServiceBookings_BookingDate",
                table: "ServiceBookings");

            migrationBuilder.DropIndex(
                name: "IX_ServiceBookings_BookingStatus",
                table: "ServiceBookings");

            migrationBuilder.DropIndex(
                name: "IX_ServiceBookings_CustomerId",
                table: "ServiceBookings");

            migrationBuilder.DropIndex(
                name: "IX_ServiceBookings_ReferenceNumber",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "BookingDate",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "BookingStatus",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "EstimatedCostMax",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "EstimatedCostMin",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "ServiceNotes",
                table: "ServiceBookings");

            migrationBuilder.RenameColumn(
                name: "VehicleYear",
                table: "ServiceBookings",
                newName: "Year");

            migrationBuilder.RenameColumn(
                name: "VehicleVin",
                table: "ServiceBookings",
                newName: "VIN");

            migrationBuilder.RenameColumn(
                name: "VehicleModel",
                table: "ServiceBookings",
                newName: "Model");

            migrationBuilder.RenameColumn(
                name: "VehicleMileage",
                table: "ServiceBookings",
                newName: "Mileage");

            migrationBuilder.RenameColumn(
                name: "VehicleMake",
                table: "ServiceBookings",
                newName: "Make");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "ServiceBookings",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "EstimatedDurationMinutes",
                table: "ServiceBookings",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "BookingTime",
                table: "ServiceBookings",
                newName: "PreferredStart");

            migrationBuilder.RenameColumn(
                name: "ServiceBookingId",
                table: "ServiceBookings",
                newName: "Id");

            migrationBuilder.AlterColumn<int>(
                name: "ServiceType",
                table: "ServiceBookings",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerId",
                table: "ServiceBookings",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckedInDate",
                table: "ServiceBookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ServiceBookings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerEmail",
                table: "ServiceBookings",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "ServiceBookings",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerPhone",
                table: "ServiceBookings",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryMethod",
                table: "ServiceBookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedCost",
                table: "ServiceBookings",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedDurationMin",
                table: "ServiceBookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsCheckedIn",
                table: "ServiceBookings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsServiceStarted",
                table: "ServiceBookings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "ServiceBookings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "PreferredDate",
                table: "ServiceBookings",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "QRCode",
                table: "ServiceBookings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reference",
                table: "ServiceBookings",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ServiceStartedDate",
                table: "ServiceBookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Mechanics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Skills = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rating = table.Column<double>(type: "float", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mechanics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MechanicUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    MechanicId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MechanicUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MechanicUsers_Mechanics_MechanicId",
                        column: x => x.MechanicId,
                        principalTable: "Mechanics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceBookingId = table.Column<int>(type: "int", nullable: false),
                    MechanicId = table.Column<int>(type: "int", nullable: true),
                    ScheduledDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ScheduledStart = table.Column<TimeOnly>(type: "time", nullable: false),
                    DurationMin = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceJobs_Mechanics_MechanicId",
                        column: x => x.MechanicId,
                        principalTable: "Mechanics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ServiceJobs_ServiceBookings_ServiceBookingId",
                        column: x => x.ServiceBookingId,
                        principalTable: "ServiceBookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceChecklists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceJobId = table.Column<int>(type: "int", nullable: false),
                    MechanicId = table.Column<int>(type: "int", nullable: false),
                    ServiceType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    TotalEstimatedCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalActualCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalEstimatedDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    TotalActualDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    CompletedItemsCount = table.Column<int>(type: "int", nullable: false),
                    TotalItemsCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ServiceJobId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceChecklists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceChecklists_Mechanics_MechanicId",
                        column: x => x.MechanicId,
                        principalTable: "Mechanics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceChecklists_ServiceJobs_ServiceJobId",
                        column: x => x.ServiceJobId,
                        principalTable: "ServiceJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceChecklists_ServiceJobs_ServiceJobId1",
                        column: x => x.ServiceJobId1,
                        principalTable: "ServiceJobs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ServiceChecklistItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceChecklistId = table.Column<int>(type: "int", nullable: false),
                    TaskName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EstimatedCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ActualCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EstimatedDurationMinutes = table.Column<int>(type: "int", nullable: true),
                    ActualDurationMinutes = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceChecklistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceChecklistItems_ServiceChecklists_ServiceChecklistId",
                        column: x => x.ServiceChecklistId,
                        principalTable: "ServiceChecklists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServicePhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceChecklistId = table.Column<int>(type: "int", nullable: false),
                    PhotoType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TakenAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TakenBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServicePhotos_ServiceChecklists_ServiceChecklistId",
                        column: x => x.ServiceChecklistId,
                        principalTable: "ServiceChecklists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Mechanics",
                columns: new[] { "Id", "IsAvailable", "Name", "Rating", "Skills" },
                values: new object[,]
                {
                    { 1, true, "Alice Khumalo", 4.7000000000000002, "Engine|Diagnostics|Maintenance" },
                    { 2, true, "Brian Moyo", 4.2999999999999998, "Electrical|Brakes" },
                    { 3, false, "Cindy Dlamini", 4.5, "Suspension|Transmission" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBookings_CreatedAtUtc",
                table: "ServiceBookings",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBookings_PreferredDate",
                table: "ServiceBookings",
                column: "PreferredDate");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBookings_Reference",
                table: "ServiceBookings",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBookings_Status",
                table: "ServiceBookings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Mechanics_IsAvailable",
                table: "Mechanics",
                column: "IsAvailable");

            migrationBuilder.CreateIndex(
                name: "IX_Mechanics_Name",
                table: "Mechanics",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_MechanicUsers_MechanicId",
                table: "MechanicUsers",
                column: "MechanicId");

            migrationBuilder.CreateIndex(
                name: "IX_MechanicUsers_UserId",
                table: "MechanicUsers",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceChecklistItems_IsCompleted",
                table: "ServiceChecklistItems",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceChecklistItems_ServiceChecklistId",
                table: "ServiceChecklistItems",
                column: "ServiceChecklistId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceChecklists_IsCompleted",
                table: "ServiceChecklists",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceChecklists_MechanicId",
                table: "ServiceChecklists",
                column: "MechanicId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceChecklists_ServiceJobId",
                table: "ServiceChecklists",
                column: "ServiceJobId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceChecklists_ServiceJobId1",
                table: "ServiceChecklists",
                column: "ServiceJobId1",
                unique: true,
                filter: "[ServiceJobId1] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceChecklists_StartedAt",
                table: "ServiceChecklists",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceJobs_MechanicId",
                table: "ServiceJobs",
                column: "MechanicId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceJobs_ScheduledDate",
                table: "ServiceJobs",
                column: "ScheduledDate");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceJobs_ScheduledDate_MechanicId",
                table: "ServiceJobs",
                columns: new[] { "ScheduledDate", "MechanicId" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceJobs_ServiceBookingId",
                table: "ServiceJobs",
                column: "ServiceBookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServicePhotos_PhotoType",
                table: "ServicePhotos",
                column: "PhotoType");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePhotos_ServiceChecklistId",
                table: "ServicePhotos",
                column: "ServiceChecklistId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePhotos_TakenAt",
                table: "ServicePhotos",
                column: "TakenAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MechanicUsers");

            migrationBuilder.DropTable(
                name: "ServiceChecklistItems");

            migrationBuilder.DropTable(
                name: "ServicePhotos");

            migrationBuilder.DropTable(
                name: "ServiceChecklists");

            migrationBuilder.DropTable(
                name: "ServiceJobs");

            migrationBuilder.DropTable(
                name: "Mechanics");

            migrationBuilder.DropIndex(
                name: "IX_ServiceBookings_CreatedAtUtc",
                table: "ServiceBookings");

            migrationBuilder.DropIndex(
                name: "IX_ServiceBookings_PreferredDate",
                table: "ServiceBookings");

            migrationBuilder.DropIndex(
                name: "IX_ServiceBookings_Reference",
                table: "ServiceBookings");

            migrationBuilder.DropIndex(
                name: "IX_ServiceBookings_Status",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "CheckedInDate",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "CustomerEmail",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "CustomerPhone",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "DeliveryMethod",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "EstimatedCost",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "EstimatedDurationMin",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "IsCheckedIn",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "IsServiceStarted",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "PreferredDate",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "QRCode",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "Reference",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "ServiceStartedDate",
                table: "ServiceBookings");

            migrationBuilder.RenameColumn(
                name: "Year",
                table: "ServiceBookings",
                newName: "VehicleYear");

            migrationBuilder.RenameColumn(
                name: "VIN",
                table: "ServiceBookings",
                newName: "VehicleVin");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "ServiceBookings",
                newName: "EstimatedDurationMinutes");

            migrationBuilder.RenameColumn(
                name: "PreferredStart",
                table: "ServiceBookings",
                newName: "BookingTime");

            migrationBuilder.RenameColumn(
                name: "Model",
                table: "ServiceBookings",
                newName: "VehicleModel");

            migrationBuilder.RenameColumn(
                name: "Mileage",
                table: "ServiceBookings",
                newName: "VehicleMileage");

            migrationBuilder.RenameColumn(
                name: "Make",
                table: "ServiceBookings",
                newName: "VehicleMake");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "ServiceBookings",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ServiceBookings",
                newName: "ServiceBookingId");

            migrationBuilder.AlterColumn<string>(
                name: "ServiceType",
                table: "ServiceBookings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerId",
                table: "ServiceBookings",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BookingDate",
                table: "ServiceBookings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "BookingStatus",
                table: "ServiceBookings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ServiceBookings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedCostMax",
                table: "ServiceBookings",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedCostMin",
                table: "ServiceBookings",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNumber",
                table: "ServiceBookings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ServiceNotes",
                table: "ServiceBookings",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ServiceExecutions",
                columns: table => new
                {
                    ServiceExecutionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApprovedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ServiceBookingId = table.Column<int>(type: "int", nullable: false),
                    TechnicianId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AdditionalIssues = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AdditionalWorkApproved = table.Column<bool>(type: "bit", nullable: false),
                    AdvisorApproval = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExecutionStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LaborHours = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    LaborRate = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    PartsUsed = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    QualityCheckPassed = table.Column<bool>(type: "bit", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TasksCompleted = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TestDriveNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TotalCost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
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
                    MechanicId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ServiceBookingId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstimatedCompletion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScheduleStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    ServiceBay = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
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
                    InspectorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ServiceBookingId = table.Column<int>(type: "int", nullable: false),
                    CheckInTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DamageDocumentation = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ExteriorCondition = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FluidLevels = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FuelLevel = table.Column<int>(type: "int", nullable: true),
                    InspectionPhotos = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    InspectionReportUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InspectionStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InteriorCondition = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    OdometerReading = table.Column<int>(type: "int", nullable: true),
                    RequiredMaintenance = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    DriverId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ServiceBookingId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerSignatures = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DropoffDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DropoffPhotos = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DropoffTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    FuelLevelDropoff = table.Column<int>(type: "int", nullable: true),
                    FuelLevelPickup = table.Column<int>(type: "int", nullable: true),
                    MileageDropoff = table.Column<int>(type: "int", nullable: true),
                    MileagePickup = table.Column<int>(type: "int", nullable: true),
                    PickupDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PickupLocation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PickupPhotos = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PickupStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PickupTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VehicleConditionDropoff = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    VehicleConditionPickup = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
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
                    AdditionalFees = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InvoiceStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LaborCost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    PartsCost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
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
                    CustomerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ServiceBookingId = table.Column<int>(type: "int", nullable: false),
                    ServiceInvoiceId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentGatewayResponse = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProofOfPaymentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReceiptUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
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

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceBookings_AspNetUsers_CustomerId",
                table: "ServiceBookings",
                column: "CustomerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
