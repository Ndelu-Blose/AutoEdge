
using AutoEdge.Data;
using AutoEdge.Models.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure localization for South African Rand
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("en-ZA") };
    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en-ZA");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// Configure Entity Framework with Retry Logic
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            sqlOptions.CommandTimeout(60); // Increase command timeout to 60 seconds
        }));

// Configure Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    // Sign-in settings
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// Configure Stripe
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Register Repository pattern
builder.Services.AddScoped(typeof(AutoEdge.Repositories.IRepository<>), typeof(AutoEdge.Repositories.Repository<>));
builder.Services.AddScoped<AutoEdge.Repositories.IUnitOfWork, AutoEdge.Repositories.UnitOfWork>();

// Register OCR Service
builder.Services.AddScoped<AutoEdge.Services.IOcrService, AutoEdge.Services.OcrService>();

// Register Contract Service
builder.Services.AddScoped<AutoEdge.Services.IContractService, AutoEdge.Services.ContractService>();

// Register Payment Service
builder.Services.AddScoped<AutoEdge.Services.IPaymentService, AutoEdge.Services.PaymentService>();

// Register Email Services
builder.Services.AddScoped<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, AutoEdge.Services.EmailService>();
builder.Services.AddScoped<AutoEdge.Services.IEmailService, AutoEdge.Services.CustomEmailService>();

// Register E-Signature Service
builder.Services.AddScoped<AutoEdge.Services.IESignatureService, AutoEdge.Services.ESignatureService>();

// Register QR Code Service
builder.Services.AddScoped<AutoEdge.Services.IQRCodeService, AutoEdge.Services.QRCodeService>();

// Register PDF Service
builder.Services.AddScoped<AutoEdge.Services.IPdfService, AutoEdge.Services.PdfService>();

// Register Recruitment Services
builder.Services.AddScoped<AutoEdge.Services.IResumeParserService, AutoEdge.Services.ResumeParserService>();
builder.Services.AddScoped<AutoEdge.Services.IVideoMeetingService, AutoEdge.Services.VideoMeetingService>();
builder.Services.AddScoped<AutoEdge.Services.IRecruitmentEmailService, AutoEdge.Services.RecruitmentEmailService>();
builder.Services.AddScoped<AutoEdge.Services.IInterviewSchedulingService, AutoEdge.Services.InterviewSchedulingService>();

// AI Configuration
builder.Services.Configure<AutoEdge.Services.AISettings>(
    builder.Configuration.GetSection("AI"));

builder.Services.AddHttpClient<AutoEdge.Services.IAIRecruitmentService, AutoEdge.Services.AIRecruitmentService>();
builder.Services.AddScoped<AutoEdge.Services.IAIRecruitmentService, AutoEdge.Services.AIRecruitmentService>();
builder.Services.AddScoped<AutoEdge.Services.IAssessmentService, AutoEdge.Services.AssessmentService>();

builder.Services.AddHttpClient();

// Register Service Booking & Scheduling Services
builder.Services.AddScoped<AutoEdge.Services.IBookingService, AutoEdge.Services.BookingService>();
builder.Services.AddScoped<AutoEdge.Services.ISchedulingService, AutoEdge.Services.SchedulingService>();

// Register Service Checklist Service
builder.Services.AddScoped<AutoEdge.Services.IServiceChecklistService, AutoEdge.Services.ServiceChecklistService>();

// Register Pickup & Drop-off Services
builder.Services.AddScoped<AutoEdge.Services.IPickupDropoffService, AutoEdge.Services.PickupDropoffService>();

// Register Service Notification Service
builder.Services.AddScoped<AutoEdge.Services.IServiceNotificationService, AutoEdge.Services.ServiceNotificationService>();

// Register QR Code PDF Service
builder.Services.AddScoped<AutoEdge.Services.IQRCodePdfService, AutoEdge.Services.QRCodePdfService>();

// Register AI Assistant Service
builder.Services.AddHttpClient<AutoEdge.Services.IAIAssistantService, AutoEdge.Services.AIAssistantService>();

// Add file upload configuration
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 104857600; // 100 MB
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Use localization
app.UseRequestLocalization();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Custom route for assessment redirect (backward compatibility)
// Assessment routes are now handled through authenticated controller actions

app.MapRazorPages();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Starting database migration and seeding...");

        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure database is created and migrations are applied.
        // In container/dev startup, SQL Server can accept connections a bit later than app boot.
        if (ShouldUseDatabaseStartupRetries(app.Environment))
        {
            await WaitForDatabaseReadyAsync(
                context,
                logger,
                maxAttempts: 24,
                delay: TimeSpan.FromSeconds(5));
        }

        logger.LogInformation("Applying database migrations...");
        await ApplyMigrationsWithRetryAsync(
            context,
            logger,
            maxAttempts: 5,
            delay: TimeSpan.FromSeconds(5));
        logger.LogInformation("Database migrations completed successfully.");

        // Seed document types
        logger.LogInformation("Seeding document types...");
        await SeedData.SeedDocumentTypesAsync(context);

        // Seed vehicles
        logger.LogInformation("Seeding vehicles...");
        await SeedData.SeedVehiclesAsync(context);

        // Seed vehicle images
        logger.LogInformation("Seeding vehicle images...");
        await SeedData.SeedVehicleImagesAsync(context);

        // Seed driver user
        logger.LogInformation("Seeding driver user...");
        await SeedData.SeedDriverUserAsync(context);

        // Seed pickup driver for vehicle service pickups
        logger.LogInformation("Seeding pickup driver...");
        await SeedData.SeedPickupDriverAsync(context, userManager);

        // Seed recruitment users
        logger.LogInformation("Seeding recruitment users...");
        //await SeedData.SeedRecruitmentUsersAsync(context, userManager);

        // 🔧  PASS roleManager TO THE METHOD  🔧
        await SeedData.SeedRecruitmentUsersAsync(context, userManager, roleManager);

        // Seed job postings
        logger.LogInformation("Seeding job postings...");
        await SeedData.SeedJobPostingsAsync(context, userManager);

        // Seed sample applications
        logger.LogInformation("Seeding sample applications...");
        await SeedData.SeedSampleApplicationsAsync(context, userManager);

        // Update existing applications with user IDs
        logger.LogInformation("Updating existing applications...");
        await SeedData.UpdateExistingApplicationsWithUserIdsAsync(context);

        // Seed questions for assessments
        logger.LogInformation("Seeding assessment questions...");
        await SeedData.SeedQuestionsAsync(context);

        // Seed default admin user if it doesn't exist
        logger.LogInformation("Seeding default admin user...");
        await SeedDefaultAdminUser(userManager, roleManager);

        // Seed technician user for scheduling/maintenance access
        logger.LogInformation("Seeding technician user...");
        await SeedData.SeedTechnicianUserAsync(userManager, roleManager);

        // Link tech user to a Mechanic record for dashboard access
        logger.LogInformation("Linking technician to mechanic record...");
        await SeedData.SeedMechanicUserLinkAsync(app.Services);

        logger.LogInformation("Database seeding completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database.");

        // Don't throw the exception - let the app start even if seeding fails
        // This is important for Azure deployment where the first startup might have connectivity issues
        logger.LogWarning("Application will continue despite seeding errors. Please check Azure SQL firewall settings.");
    }
}

app.Run();

// Method to seed default admin user
async Task SeedDefaultAdminUser(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
{
    // Ensure roles exist
    string[] roleNames = { "Administrator", "Customer", "SalesRepresentative", "SupportStaff", "Driver", "Recruiter", "Applicant" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // Create default admin user
    var adminEmail = "admin@autoedge.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "System",
            LastName = "Administrator",
            EmailConfirmed = true,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(adminUser, "Admin@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Administrator");
        }
    }
}

static bool ShouldUseDatabaseStartupRetries(IHostEnvironment environment)
{
    var runningInContainer = string.Equals(
        Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
        "true",
        StringComparison.OrdinalIgnoreCase);

    return environment.IsDevelopment() || runningInContainer;
}

static async Task WaitForDatabaseReadyAsync(
    ApplicationDbContext context,
    ILogger logger,
    int maxAttempts,
    TimeSpan delay)
{
    var masterConnectionString = BuildMasterDatabaseConnectionString(context.Database.GetConnectionString());

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            await using var connection = new SqlConnection(masterConnectionString);
            await connection.OpenAsync();
            await connection.CloseAsync();
            if (connection.State == System.Data.ConnectionState.Closed)
            {
                logger.LogInformation("SQL Server connection is ready on attempt {Attempt}.", attempt);
                return;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database not ready on attempt {Attempt}.", attempt);
        }

        if (attempt < maxAttempts)
        {
            logger.LogInformation(
                "Waiting {DelaySeconds}s before retrying database readiness (attempt {NextAttempt}/{MaxAttempts}).",
                delay.TotalSeconds,
                attempt + 1,
                maxAttempts);
            await Task.Delay(delay);
        }
    }

    logger.LogWarning(
        "Database readiness check did not succeed after {MaxAttempts} attempts; migration retry will still run.",
        maxAttempts);
}

static string BuildMasterDatabaseConnectionString(string? connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("Default database connection string is missing.");
    }

    var builder = new SqlConnectionStringBuilder(connectionString)
    {
        InitialCatalog = "master"
    };

    return builder.ConnectionString;
}

static async Task ApplyMigrationsWithRetryAsync(
    ApplicationDbContext context,
    ILogger logger,
    int maxAttempts,
    TimeSpan delay)
{
    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            await context.Database.MigrateAsync();
            return;
        }
        catch (Exception ex)
        {
            if (attempt == maxAttempts)
            {
                throw;
            }

            logger.LogWarning(
                ex,
                "Migration attempt {Attempt} failed. Retrying in {DelaySeconds}s.",
                attempt,
                delay.TotalSeconds);
            await Task.Delay(delay);
        }
    }
}

// Make Program class accessible to tests
public partial class Program { }