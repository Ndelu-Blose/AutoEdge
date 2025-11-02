using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoEdge.Migrations
{
    /// <inheritdoc />
    public partial class AddEmploymentOnboardingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmploymentOffers",
                columns: table => new
                {
                    OfferId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SalaryOffered = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmploymentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WorkLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContractFilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OfferSentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OfferExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AccessToken = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ContractAccepted = table.Column<bool>(type: "bit", nullable: false),
                    ContractAcceptedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ContractSignature = table.Column<string>(type: "text", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmploymentOffers", x => x.OfferId);
                    table.ForeignKey(
                        name: "FK_EmploymentOffers_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "ApplicationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PreEmploymentDocumentations",
                columns: table => new
                {
                    DocumentationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfferId = table.Column<int>(type: "int", nullable: false),
                    IdNumber = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nationality = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaritalStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ResidentialAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Province = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AlternativePhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EmergencyContactName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EmergencyContactRelationship = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmergencyContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EmergencyContactAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SecondaryEmergencyContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SecondaryEmergencyContactName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SecondaryEmergencyContactRelationship = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SecondaryEmergencyContactAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AccountType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BranchCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    AccountHolderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BankStatementPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TaxNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RegisteredForTax = table.Column<bool>(type: "bit", nullable: false),
                    TaxClearancePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TaxDirectiveNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HasMedicalAid = table.Column<bool>(type: "bit", nullable: false),
                    MedicalAidProvider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MedicalAidNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MedicalAidMemberType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    HasChronicConditions = table.Column<bool>(type: "bit", nullable: false),
                    ChronicConditionsDetails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OnChronicMedication = table.Column<bool>(type: "bit", nullable: false),
                    HasDisabilities = table.Column<bool>(type: "bit", nullable: false),
                    DisabilitiesDetails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NumberOfDependents = table.Column<int>(type: "int", nullable: false),
                    DependentsDetails = table.Column<string>(type: "text", nullable: true),
                    CertifiedIdPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProofOfAddressPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    QualificationCertificatesPath = table.Column<string>(type: "text", nullable: true),
                    DriversLicensePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OptionalDocumentsPath = table.Column<string>(type: "text", nullable: true),
                    HasCriminalRecord = table.Column<bool>(type: "bit", nullable: false),
                    CriminalRecordDetails = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DeclareAccurate = table.Column<bool>(type: "bit", nullable: false),
                    ConsentBackgroundCheck = table.Column<bool>(type: "bit", nullable: false),
                    ConsentDataProcessing = table.Column<bool>(type: "bit", nullable: false),
                    DocumentAuthenticity = table.Column<bool>(type: "bit", nullable: false),
                    DigitalSignature = table.Column<string>(type: "text", nullable: true),
                    SignedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdminReviewed = table.Column<bool>(type: "bit", nullable: false),
                    AdminReviewedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Approved = table.Column<bool>(type: "bit", nullable: false),
                    AdminNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CorrectionRequests = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreEmploymentDocumentations", x => x.DocumentationId);
                    table.ForeignKey(
                        name: "FK_PreEmploymentDocumentations_AspNetUsers_ReviewedBy",
                        column: x => x.ReviewedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PreEmploymentDocumentations_EmploymentOffers_OfferId",
                        column: x => x.OfferId,
                        principalTable: "EmploymentOffers",
                        principalColumn: "OfferId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentOffers_AccessToken",
                table: "EmploymentOffers",
                column: "AccessToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentOffers_ApplicationId",
                table: "EmploymentOffers",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentOffers_ContractAccepted",
                table: "EmploymentOffers",
                column: "ContractAccepted");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentOffers_IsActive",
                table: "EmploymentOffers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentOffers_OfferExpiryDate",
                table: "EmploymentOffers",
                column: "OfferExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentOffers_OfferSentDate",
                table: "EmploymentOffers",
                column: "OfferSentDate");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentOffers_Status",
                table: "EmploymentOffers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PreEmploymentDocumentations_AdminReviewed",
                table: "PreEmploymentDocumentations",
                column: "AdminReviewed");

            migrationBuilder.CreateIndex(
                name: "IX_PreEmploymentDocumentations_Approved",
                table: "PreEmploymentDocumentations",
                column: "Approved");

            migrationBuilder.CreateIndex(
                name: "IX_PreEmploymentDocumentations_CompletedDate",
                table: "PreEmploymentDocumentations",
                column: "CompletedDate");

            migrationBuilder.CreateIndex(
                name: "IX_PreEmploymentDocumentations_IdNumber",
                table: "PreEmploymentDocumentations",
                column: "IdNumber");

            migrationBuilder.CreateIndex(
                name: "IX_PreEmploymentDocumentations_IsActive",
                table: "PreEmploymentDocumentations",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PreEmploymentDocumentations_IsCompleted",
                table: "PreEmploymentDocumentations",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_PreEmploymentDocumentations_OfferId",
                table: "PreEmploymentDocumentations",
                column: "OfferId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreEmploymentDocumentations_ReviewedBy",
                table: "PreEmploymentDocumentations",
                column: "ReviewedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PreEmploymentDocumentations");

            migrationBuilder.DropTable(
                name: "EmploymentOffers");
        }
    }
}
