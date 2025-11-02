using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AutoEdge.Migrations
{
    /// <inheritdoc />
    public partial class AddRecruitmentEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobPostings",
                columns: table => new
                {
                    JobId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    JobDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Requirements = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Responsibilities = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinYearsExperience = table.Column<int>(type: "int", nullable: false),
                    RequiredQualifications = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PositionsAvailable = table.Column<int>(type: "int", nullable: false),
                    PostedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPostings", x => x.JobId);
                    table.ForeignKey(
                        name: "FK_JobPostings_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    ApplicationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ResumeFilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CoverLetterPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IdDocumentPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CertificatesPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ParsedResumeText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YearsOfExperience = table.Column<int>(type: "int", nullable: false),
                    ExtractedSkills = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtractedEducation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MatchScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SubmittedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdminNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    WhySuitableForRole = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    HighestQualification = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.ApplicationId);
                    table.ForeignKey(
                        name: "FK_Applications_JobPostings_JobId",
                        column: x => x.JobId,
                        principalTable: "JobPostings",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Assessments",
                columns: table => new
                {
                    AssessmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    AssessmentTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AccessToken = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    QuestionsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnswersJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailSent = table.Column<bool>(type: "bit", nullable: false),
                    EmailSentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssessmentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TimeLimitMinutes = table.Column<int>(type: "int", nullable: false),
                    IsPassed = table.Column<bool>(type: "bit", nullable: false),
                    GradingNotes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessments", x => x.AssessmentId);
                    table.ForeignKey(
                        name: "FK_Assessments_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "ApplicationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Interviews",
                columns: table => new
                {
                    InterviewId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    ScheduledDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    MeetingLink = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MeetingPassword = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RecruiterName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RecruiterEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InterviewNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InterviewRating = table.Column<int>(type: "int", nullable: false),
                    EmailSent = table.Column<bool>(type: "bit", nullable: false),
                    EmailSentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AdditionalInstructions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MeetingPlatform = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interviews", x => x.InterviewId);
                    table.ForeignKey(
                        name: "FK_Interviews_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "ApplicationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Interviews_JobPostings_JobId",
                        column: x => x.JobId,
                        principalTable: "JobPostings",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InterviewSlots",
                columns: table => new
                {
                    SlotId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsBooked = table.Column<bool>(type: "bit", nullable: false),
                    InterviewId = table.Column<int>(type: "int", nullable: true),
                    RecruiterName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RecruiterEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewSlots", x => x.SlotId);
                    table.ForeignKey(
                        name: "FK_InterviewSlots_Interviews_InterviewId",
                        column: x => x.InterviewId,
                        principalTable: "Interviews",
                        principalColumn: "InterviewId",
                        onDelete: ReferentialAction.SetNull);
                });

            // Roles already exist, no need to insert them

            migrationBuilder.CreateIndex(
                name: "IX_Applications_Email",
                table: "Applications",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_IsActive",
                table: "Applications",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_JobId",
                table: "Applications",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_MatchScore",
                table: "Applications",
                column: "MatchScore");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_Status",
                table: "Applications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_SubmittedDate",
                table: "Applications",
                column: "SubmittedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_AccessToken",
                table: "Assessments",
                column: "AccessToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_ApplicationId",
                table: "Assessments",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_AssessmentType",
                table: "Assessments",
                column: "AssessmentType");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_DueDate",
                table: "Assessments",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_IsActive",
                table: "Assessments",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_IsCompleted",
                table: "Assessments",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_ApplicationId",
                table: "Interviews",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_IsActive",
                table: "Interviews",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_IsCompleted",
                table: "Interviews",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_JobId",
                table: "Interviews",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_RecruiterEmail",
                table: "Interviews",
                column: "RecruiterEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_ScheduledDateTime",
                table: "Interviews",
                column: "ScheduledDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewSlots_EndTime",
                table: "InterviewSlots",
                column: "EndTime");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewSlots_InterviewId",
                table: "InterviewSlots",
                column: "InterviewId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewSlots_IsActive",
                table: "InterviewSlots",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewSlots_IsBooked",
                table: "InterviewSlots",
                column: "IsBooked");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewSlots_RecruiterName",
                table: "InterviewSlots",
                column: "RecruiterName");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewSlots_StartTime",
                table: "InterviewSlots",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostings_ClosingDate",
                table: "JobPostings",
                column: "ClosingDate");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostings_CreatedByUserId",
                table: "JobPostings",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostings_Department",
                table: "JobPostings",
                column: "Department");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostings_IsActive",
                table: "JobPostings",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostings_PostedDate",
                table: "JobPostings",
                column: "PostedDate");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostings_Status",
                table: "JobPostings",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assessments");

            migrationBuilder.DropTable(
                name: "InterviewSlots");

            migrationBuilder.DropTable(
                name: "Interviews");

            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropTable(
                name: "JobPostings");

            // Roles should not be deleted as they may be used elsewhere
        }
    }
}
