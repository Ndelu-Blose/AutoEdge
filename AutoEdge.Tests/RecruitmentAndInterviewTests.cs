using AutoEdge.Models.Entities;
using AutoEdge.Services;
using AutoEdge.Tests.TestSupport;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutoEdge.Tests;

public class RecruitmentAndInterviewTests
{
    [Fact]
    public async Task ManuallyShortlistApplicationsAsync_ShouldMarkSelectedApplicationsAsShortlisted()
    {
        using var context = TestDbFactory.CreateContext();
        var applicationOne = SeedApplication(context, "Submitted");
        var applicationTwo = SeedApplication(context, "Submitted");
        await context.SaveChangesAsync();

        var service = CreateResumeService(context);
        var result = await service.ManuallyShortlistApplicationsAsync([applicationOne.ApplicationId]);

        Assert.True(result);
        Assert.Equal("Shortlisted", context.Applications.Single(a => a.ApplicationId == applicationOne.ApplicationId).Status);
        Assert.Equal("Submitted", context.Applications.Single(a => a.ApplicationId == applicationTwo.ApplicationId).Status);
    }

    [Fact]
    public async Task CreateInterviewSlotsAsync_ShouldFail_WhenStartDateIsWeekend()
    {
        using var context = TestDbFactory.CreateContext();
        var app = SeedApplication(context, "Shortlisted");
        await context.SaveChangesAsync();

        var service = CreateInterviewService(context);
        var weekendDate = NextDay(DateTime.Today, DayOfWeek.Saturday).AddHours(10);

        var result = await service.CreateInterviewSlotsAsync([app], weekendDate, 60, "Recruiter", "recruiter@autoedge.dev");

        Assert.False(result.Success);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task HasTimeConflictAsync_ShouldReturnTrue_ForOverlappingInterview()
    {
        using var context = TestDbFactory.CreateContext();
        var existingStart = DateTime.Today.AddDays(2).AddHours(10);
        context.Interviews.Add(new Interview
        {
            ApplicationId = 1,
            JobId = 1,
            ScheduledDateTime = existingStart,
            DurationMinutes = 60,
            RecruiterName = "Alex",
            RecruiterEmail = "alex@autoedge.dev",
            IsActive = true
        });
        await context.SaveChangesAsync();

        var service = CreateInterviewService(context);
        var hasConflict = await service.HasTimeConflictAsync(existingStart.AddMinutes(30), 45, "Alex");

        Assert.True(hasConflict);
    }

    [Fact]
    public async Task CreateInterviewSlotsAsync_ShouldScheduleInterviewAndUpdateStatus_WhenInputIsValid()
    {
        using var context = TestDbFactory.CreateContext();
        var app = SeedApplication(context, "Submitted");
        await context.SaveChangesAsync();

        var service = CreateInterviewService(context);
        var weekday = NextDay(DateTime.Today, DayOfWeek.Tuesday).AddHours(10);
        var result = await service.CreateInterviewSlotsAsync([app], weekday, 45, "Sam Recruiter", "sam@autoedge.dev");

        Assert.True(result.Success);
        Assert.Single(result.CreatedInterviews);
        Assert.Equal("Interview Scheduled", context.Applications.Single(a => a.ApplicationId == app.ApplicationId).Status);
    }

    private static ResumeParserService CreateResumeService(AutoEdge.Data.ApplicationDbContext context)
    {
        var logger = new Mock<ILogger<ResumeParserService>>();
        var ai = new Mock<IAIRecruitmentService>();
        return new ResumeParserService(context, logger.Object, ai.Object);
    }

    private static InterviewSchedulingService CreateInterviewService(AutoEdge.Data.ApplicationDbContext context)
    {
        var logger = new Mock<ILogger<InterviewSchedulingService>>();
        var video = new Mock<IVideoMeetingService>();
        video.Setup(x => x.CreateMeetingAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new MeetingDetails
            {
                MeetingId = "meeting-1",
                MeetingUrl = "https://example.com/meeting",
                MeetingPassword = "pass",
                Platform = "Zoom"
            });
        return new InterviewSchedulingService(context, logger.Object, video.Object);
    }

    private static Application SeedApplication(AutoEdge.Data.ApplicationDbContext context, string status)
    {
        var job = new JobPosting
        {
            JobTitle = "Service Advisor",
            Department = "Service",
            JobDescription = "Test job",
            Requirements = "Communication, Sales",
            Responsibilities = "Assist customers",
            MinYearsExperience = 1,
            RequiredQualifications = "Diploma",
            PositionsAvailable = 1,
            ClosingDate = DateTime.Today.AddDays(30),
            Status = "Active",
            CreatedByUserId = "creator"
        };
        context.JobPostings.Add(job);
        context.SaveChanges();

        var app = new Application
        {
            JobId = job.JobId,
            FirstName = "Ava",
            LastName = "Candidate",
            Email = $"ava-{Guid.NewGuid():N}@example.com",
            PhoneNumber = "0111111111",
            Address = "123 Test St",
            Status = status,
            IsActive = true
        };
        context.Applications.Add(app);
        context.SaveChanges();
        return app;
    }

    private static DateTime NextDay(DateTime fromDate, DayOfWeek target)
    {
        var date = fromDate.Date.AddDays(1);
        while (date.DayOfWeek != target)
        {
            date = date.AddDays(1);
        }
        return date;
    }
}
