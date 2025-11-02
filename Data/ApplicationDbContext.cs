using AutoEdge.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AutoEdge.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for all entities
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleImage> VehicleImages { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Inquiry> Inquiries { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseStatusHistory> PurchaseStatusHistories { get; set; }
        public DbSet<CustomerFeedback> CustomerFeedbacks { get; set; }
        public DbSet<Warranty> Warranties { get; set; }
        public DbSet<ServiceReminder> ServiceReminders { get; set; }
        public DbSet<DigitalSignature> DigitalSignatures { get; set; }
        
        // Recruitment entities
        public DbSet<JobPosting> JobPostings { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Interview> Interviews { get; set; }
        public DbSet<InterviewSlot> InterviewSlots { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
        
        // Assessment entities
        public DbSet<Question> Questions { get; set; }
        public DbSet<AssessmentQuestion> AssessmentQuestions { get; set; }
        public DbSet<AssessmentAnswer> AssessmentAnswers { get; set; }
        public DbSet<RecruiterAssignment> RecruiterAssignments { get; set; }
        
        // Employment onboarding entities
        public DbSet<EmploymentOffer> EmploymentOffers { get; set; }
        public DbSet<PreEmploymentDocumentation> PreEmploymentDocumentations { get; set; }

        // Service management entities
        // public DbSet<ServiceBooking> ServiceBookings { get; set; }
        // public DbSet<ServiceSchedule> ServiceSchedules { get; set; }
        // public DbSet<VehiclePickup> VehiclePickups { get; set; }
        // public DbSet<VehicleInspection> VehicleInspections { get; set; }
        // public DbSet<ServiceExecution> ServiceExecutions { get; set; }
        // public DbSet<ServiceInvoice> ServiceInvoices { get; set; }
        // public DbSet<ServicePayment> ServicePayments { get; set; }
        
        public DbSet<ServiceBooking> ServiceBookings { get; set; }
        public DbSet<Mechanic> Mechanics { get; set; }
        public DbSet<ServiceJob> ServiceJobs { get; set; }
        public DbSet<MechanicUser> MechanicUsers { get; set; }
        public DbSet<ServiceChecklist> ServiceChecklists { get; set; }
        public DbSet<ServiceChecklistItem> ServiceChecklistItems { get; set; }
        public DbSet<ServicePhoto> ServicePhotos { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<ServiceInvoice> ServiceInvoices { get; set; }
        public DbSet<ServicePayment> ServicePayments { get; set; }
        public DbSet<VehiclePickup> VehiclePickups { get; set; }
        public DbSet<ServiceExecution> ServiceExecutions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Vehicle entity
            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.HasIndex(e => e.VIN).IsUnique();
                entity.HasIndex(e => new { e.Make, e.Model, e.Year });
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.DateListed);
                
                entity.Property(e => e.VIN).IsRequired().HasMaxLength(17);
                entity.Property(e => e.Make).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Model).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CostPrice).HasPrecision(18, 2);
                entity.Property(e => e.SellingPrice).HasPrecision(18, 2);
                entity.Property(e => e.NegotiableRange).HasPrecision(18, 2);
            });

            // Configure VehicleImage entity
            modelBuilder.Entity<VehicleImage>(entity =>
            {
                entity.HasIndex(e => e.VehicleId);
                entity.HasIndex(e => e.ImageType);
                entity.HasIndex(e => e.DisplayOrder);
                
                entity.HasOne(d => d.Vehicle)
                    .WithMany(p => p.VehicleImages)
                    .HasForeignKey(d => d.VehicleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ApplicationUser entity
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.IsActive);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            });

            // Configure Customer entity
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasIndex(e => e.CustomerStatus);
                entity.HasIndex(e => e.IsActive);
                
                entity.HasOne(d => d.User)
                    .WithOne(p => p.Customer)
                    .HasForeignKey<Customer>(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.Property(e => e.AnnualIncome).HasPrecision(18, 2);
                entity.Property(e => e.TotalSpent).HasPrecision(18, 2);
            });

            // Configure Inquiry entity
            modelBuilder.Entity<Inquiry>(entity =>
            {
                entity.HasIndex(e => e.VehicleId);
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedDate);
                entity.HasIndex(e => e.AssignedToUserId);
                
                entity.HasOne(d => d.Vehicle)
                    .WithMany(p => p.Inquiries)
                    .HasForeignKey(d => d.VehicleId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Inquiries)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(d => d.AssignedTo)
                    .WithMany(p => p.Inquiries)
                    .HasForeignKey(d => d.AssignedToUserId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.Property(e => e.OfferedPrice).HasPrecision(18, 2);
            });

            // Configure Reservation entity
            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasIndex(e => e.VehicleId);
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.ReservationNumber).IsUnique();
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.ExpiryDate);
                
                entity.HasOne(d => d.Vehicle)
                    .WithMany(p => p.Reservations)
                    .HasForeignKey(d => d.VehicleId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Reservations)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.Property(e => e.DepositAmount).HasPrecision(18, 2);
                entity.Property(e => e.RefundAmount).HasPrecision(18, 2);
            });

            // Configure DocumentType entity
            modelBuilder.Entity<DocumentType>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.IsRequired);
                
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.AllowedFileTypes).HasMaxLength(200);
            });

            // Configure Document entity
            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.DocumentTypeId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.UploadDate);
                entity.HasIndex(e => e.IsActive);
                

                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ContentType).HasMaxLength(100);
                entity.Property(e => e.ValidationNotes).HasMaxLength(1000);
                entity.Property(e => e.ExtractedText).HasMaxLength(2000);
                entity.Property(e => e.OcrValidationResults).HasMaxLength(1000);
                
                entity.HasOne(d => d.DocumentType)
                    .WithMany(p => p.Documents)
                    .HasForeignKey(d => d.DocumentTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Documents)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(d => d.Reviewer)
                    .WithMany()
                    .HasForeignKey(d => d.ReviewedBy)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Contract entity
            modelBuilder.Entity<Contract>(entity =>
            {
                entity.HasIndex(e => e.VehicleId);
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.ContractNumber).IsUnique();
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedDate);
                
                entity.HasOne(d => d.Vehicle)
                    .WithMany(p => p.Contracts)
                    .HasForeignKey(d => d.VehicleId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Contracts)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(d => d.CreatedBy)
                    .WithMany(p => p.CreatedContracts)
                    .HasForeignKey(d => d.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.Property(e => e.DownPayment).HasPrecision(18, 2);
                entity.Property(e => e.FinancedAmount).HasPrecision(18, 2);
                entity.Property(e => e.InterestRate).HasPrecision(5, 4);
                entity.Property(e => e.MonthlyPayment).HasPrecision(18, 2);
                entity.Property(e => e.TradeInValue).HasPrecision(18, 2);
                entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
                entity.Property(e => e.RegistrationFee).HasPrecision(18, 2);
                entity.Property(e => e.DocumentationFee).HasPrecision(18, 2);
                entity.Property(e => e.ExtendedWarrantyFee).HasPrecision(18, 2);
                entity.Property(e => e.OtherFees).HasPrecision(18, 2);
            });

            // Configure Payment entity
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasIndex(e => e.ContractId);
                entity.HasIndex(e => e.TransactionId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.ProcessedDate);
                entity.HasIndex(e => e.PaymentType);
                
                entity.HasOne(d => d.Contract)
                    .WithMany(p => p.Payments)
                    .HasForeignKey(d => d.ContractId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.RefundAmount).HasPrecision(18, 2);
                entity.Property(e => e.ProcessingFee).HasPrecision(5, 4);
                entity.Property(e => e.NetAmount).HasPrecision(18, 2);
                entity.Property(e => e.LateFee).HasPrecision(18, 2);
            });

            // Configure Delivery entity
            modelBuilder.Entity<Delivery>(entity =>
            {
                entity.HasIndex(e => e.ContractId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.ScheduledDate);
                entity.HasIndex(e => e.TrackingNumber);
                
                entity.HasOne(d => d.Contract)
                    .WithMany(p => p.Deliveries)
                    .HasForeignKey(d => d.ContractId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(d => d.Driver)
                    .WithMany()
                    .HasForeignKey(d => d.DriverUserId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.Property(e => e.DeliveryFee).HasPrecision(18, 2);
                entity.Property(e => e.InsuranceAmount).HasPrecision(18, 2);
                entity.Property(e => e.CurrentLatitude).HasPrecision(10, 8);
                entity.Property(e => e.CurrentLongitude).HasPrecision(11, 8);
                entity.Property(e => e.DistanceRemaining).HasPrecision(5, 2);
            });

            // Configure Purchase entity
            modelBuilder.Entity<Purchase>(entity =>
            {
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.VehicleId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.InitiatedDate);
                entity.HasIndex(e => e.IsActive);
                
                entity.Property(e => e.PurchasePrice).HasPrecision(18, 2);
                entity.Property(e => e.DepositAmount).HasPrecision(18, 2);
                entity.Property(e => e.RemainingAmount).HasPrecision(18, 2);
                entity.Property(e => e.InterestRate).HasPrecision(5, 2);
                entity.Property(e => e.MonthlyPayment).HasPrecision(18, 2);
                
                entity.HasOne(p => p.Vehicle)
                    .WithMany(v => v.Purchases)
                    .HasForeignKey(p => p.VehicleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure PurchaseStatusHistory entity
            modelBuilder.Entity<PurchaseStatusHistory>(entity =>
            {
                entity.HasIndex(e => e.PurchaseId);
                entity.HasIndex(e => e.ChangedDate);
                entity.HasIndex(e => new { e.PurchaseId, e.ChangedDate });
                
                entity.HasOne(psh => psh.Purchase)
                    .WithMany(p => p.StatusHistory)
                    .HasForeignKey(psh => psh.PurchaseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure CustomerFeedback entity
            modelBuilder.Entity<CustomerFeedback>(entity =>
            {
                entity.HasIndex(e => e.ContractId);
                entity.HasIndex(e => e.SubmittedDate);
                entity.HasIndex(e => e.IsActive);
                
                entity.HasOne(d => d.Contract)
                    .WithMany()
                    .HasForeignKey(d => d.ContractId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Warranty entity
            modelBuilder.Entity<Warranty>(entity =>
            {
                entity.HasIndex(e => e.ContractId);
                entity.HasIndex(e => e.WarrantyType);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.StartDate);
                entity.HasIndex(e => e.EndDate);
                
                entity.HasOne(d => d.Contract)
                    .WithMany()
                    .HasForeignKey(d => d.ContractId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure ServiceReminder entity
            modelBuilder.Entity<ServiceReminder>(entity =>
            {
                entity.HasIndex(e => e.ContractId);
                entity.HasIndex(e => e.ServiceType);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.ScheduledDate);
                entity.HasIndex(e => e.CreatedDate);
                
                entity.HasOne(d => d.Contract)
                    .WithMany(c => c.ServiceReminders)
                    .HasForeignKey(d => d.ContractId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure DigitalSignature entity
            modelBuilder.Entity<DigitalSignature>(entity =>
            {
                entity.HasIndex(e => e.ContractId);
                entity.HasIndex(e => e.SignerEmail);
                entity.HasIndex(e => e.SignedDate);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.CreatedDate);
                
                entity.HasOne(d => d.Contract)
                    .WithMany(c => c.DigitalSignatures)
                    .HasForeignKey(d => d.ContractId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.Property(e => e.SignerName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SignerEmail).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SignatureData).IsRequired();
                entity.Property(e => e.SignatureType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.DocumentHash).HasMaxLength(100);
            });

            // Configure JobPosting entity
            modelBuilder.Entity<JobPosting>(entity =>
            {
                entity.HasIndex(e => e.Department);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.PostedDate);
                entity.HasIndex(e => e.ClosingDate);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.CreatedByUserId);
                
                entity.Property(e => e.JobTitle).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Department).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                
                entity.HasOne(d => d.CreatedBy)
                    .WithMany()
                    .HasForeignKey(d => d.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Application entity
            modelBuilder.Entity<Application>(entity =>
            {
                entity.HasIndex(e => e.JobId);
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.SubmittedDate);
                entity.HasIndex(e => e.MatchScore);
                entity.HasIndex(e => e.IsActive);
                
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.MatchScore).HasPrecision(5, 2);
                
                entity.HasOne(d => d.JobPosting)
                    .WithMany(p => p.Applications)
                    .HasForeignKey(d => d.JobId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Interview entity
            modelBuilder.Entity<Interview>(entity =>
            {
                entity.HasIndex(e => e.ApplicationId);
                entity.HasIndex(e => e.JobId);
                entity.HasIndex(e => e.ScheduledDateTime);
                entity.HasIndex(e => e.IsCompleted);
                entity.HasIndex(e => e.RecruiterEmail);
                entity.HasIndex(e => e.IsActive);
                
                entity.Property(e => e.RecruiterName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.RecruiterEmail).IsRequired().HasMaxLength(100);
                entity.Property(e => e.MeetingPlatform).HasMaxLength(50);
                
                entity.HasOne(d => d.Application)
                    .WithMany(p => p.Interviews)
                    .HasForeignKey(d => d.ApplicationId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(d => d.JobPosting)
                    .WithMany()
                    .HasForeignKey(d => d.JobId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure InterviewSlot entity
            modelBuilder.Entity<InterviewSlot>(entity =>
            {
                entity.HasIndex(e => e.StartTime);
                entity.HasIndex(e => e.EndTime);
                entity.HasIndex(e => e.IsBooked);
                entity.HasIndex(e => e.RecruiterName);
                entity.HasIndex(e => e.IsActive);
                
                entity.Property(e => e.RecruiterName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.RecruiterEmail).HasMaxLength(100);
                
                entity.HasOne(d => d.Interview)
                    .WithMany()
                    .HasForeignKey(d => d.InterviewId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Assessment entity
            modelBuilder.Entity<Assessment>(entity =>
            {
                entity.HasIndex(e => e.ApplicationId);
                entity.HasIndex(e => e.AccessToken).IsUnique();
                entity.HasIndex(e => e.IsCompleted);
                entity.HasIndex(e => e.DueDate);
                entity.HasIndex(e => e.AssessmentType);
                entity.HasIndex(e => e.IsActive);
                
                entity.Property(e => e.AssessmentTitle).IsRequired().HasMaxLength(200);
                entity.Property(e => e.AccessToken).IsRequired().HasMaxLength(100);
                entity.Property(e => e.AssessmentType).HasMaxLength(50);
                entity.Property(e => e.Score).HasPrecision(5, 2);
                
                entity.HasOne(d => d.Application)
                    .WithMany(p => p.Assessments)
                    .HasForeignKey(d => d.ApplicationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Question entity
            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasIndex(e => e.QuestionCode).IsUnique();
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.Department);
                entity.HasIndex(e => e.IsActive);
                
                entity.Property(e => e.QuestionCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.QuestionText).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Category).HasMaxLength(50);
                entity.Property(e => e.Department).HasMaxLength(50);
            });

            // Configure AssessmentQuestion entity
            modelBuilder.Entity<AssessmentQuestion>(entity =>
            {
                entity.HasIndex(e => new { e.AssessmentId, e.Order });
                entity.HasIndex(e => e.QuestionId);
                
                entity.HasOne(d => d.Assessment)
                    .WithMany(p => p.AssessmentQuestions)
                    .HasForeignKey(d => d.AssessmentId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(d => d.Question)
                    .WithMany(p => p.AssessmentQuestions)
                    .HasForeignKey(d => d.QuestionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure AssessmentAnswer entity
            modelBuilder.Entity<AssessmentAnswer>(entity =>
            {
                entity.HasIndex(e => e.AssessmentQuestionId);
                entity.HasIndex(e => e.ApplicationId);
                entity.HasIndex(e => e.SubmittedDate);
                
                entity.Property(e => e.AnswerText).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.Score).HasPrecision(5, 2);
                
                entity.HasOne(d => d.AssessmentQuestion)
                    .WithMany(p => p.AssessmentAnswers)
                    .HasForeignKey(d => d.AssessmentQuestionId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(d => d.Application)
                    .WithMany(p => p.AssessmentAnswers)
                    .HasForeignKey(d => d.ApplicationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure RecruiterAssignment entity
            modelBuilder.Entity<RecruiterAssignment>(entity =>
            {
                entity.HasIndex(e => e.AssessmentId);
                entity.HasIndex(e => e.RecruiterId);
                entity.HasIndex(e => e.ApplicationId);
                entity.HasIndex(e => e.AssignedDate);
                entity.HasIndex(e => e.IsActive);
                
                entity.Property(e => e.RecruiterId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.RecruiterName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.RecruiterEmail).IsRequired().HasMaxLength(100);
                
                entity.HasOne(d => d.Assessment)
                    .WithMany(p => p.RecruiterAssignments)
                    .HasForeignKey(d => d.AssessmentId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(d => d.Application)
                    .WithMany(p => p.RecruiterAssignments)
                    .HasForeignKey(d => d.ApplicationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure ServiceBooking entity
            // modelBuilder.Entity<ServiceBooking>(entity =>
            // {
            //     entity.HasIndex(e => e.CustomerId);
            //     entity.HasIndex(e => e.BookingDate);
            //     entity.HasIndex(e => e.BookingStatus);
            //     entity.HasIndex(e => e.ReferenceNumber).IsUnique();
                
            //     entity.Property(e => e.CustomerId).IsRequired().HasMaxLength(450);
            //     entity.Property(e => e.VehicleMake).IsRequired().HasMaxLength(50);
            //     entity.Property(e => e.VehicleModel).IsRequired().HasMaxLength(50);
            //     entity.Property(e => e.ServiceType).IsRequired().HasMaxLength(100);
            //     entity.Property(e => e.BookingStatus).IsRequired().HasMaxLength(50);
            //     entity.Property(e => e.ReferenceNumber).IsRequired().HasMaxLength(20);
            //     entity.Property(e => e.EstimatedCostMin).HasPrecision(10, 2);
            //     entity.Property(e => e.EstimatedCostMax).HasPrecision(10, 2);
                
            //     entity.HasOne(d => d.Customer)
            //         .WithMany()
            //         .HasForeignKey(d => d.CustomerId)
            //         .OnDelete(DeleteBehavior.Restrict);
            // });

            // // Configure ServiceSchedule entity
            // modelBuilder.Entity<ServiceSchedule>(entity =>
            // {
            //     entity.HasIndex(e => e.ServiceBookingId);
            //     entity.HasIndex(e => e.MechanicId);
            //     entity.HasIndex(e => e.ScheduledDate);
            //     entity.HasIndex(e => e.ScheduleStatus);
                
            //     entity.Property(e => e.MechanicId).IsRequired().HasMaxLength(450);
            //     entity.Property(e => e.ServiceBay).HasMaxLength(20);
            //     entity.Property(e => e.ScheduleStatus).IsRequired().HasMaxLength(50);
                
            //     entity.HasOne(d => d.ServiceBooking)
            //         .WithMany(p => p.ServiceSchedules)
            //         .HasForeignKey(d => d.ServiceBookingId)
            //         .OnDelete(DeleteBehavior.Cascade);
                    
            //     entity.HasOne(d => d.Mechanic)
            //         .WithMany()
            //         .HasForeignKey(d => d.MechanicId)
            //         .OnDelete(DeleteBehavior.Restrict);
            // });

            // // Configure VehiclePickup entity
            // modelBuilder.Entity<VehiclePickup>(entity =>
            // {
            //     entity.HasIndex(e => e.ServiceBookingId);
            //     entity.HasIndex(e => e.DriverId);
            //     entity.HasIndex(e => e.PickupDate);
            //     entity.HasIndex(e => e.PickupStatus);
                
            //     entity.Property(e => e.DriverId).IsRequired().HasMaxLength(450);
            //     entity.Property(e => e.PickupLocation).IsRequired().HasMaxLength(500);
            //     entity.Property(e => e.PickupStatus).IsRequired().HasMaxLength(50);
                
            //     entity.HasOne(d => d.ServiceBooking)
            //         .WithMany(p => p.VehiclePickups)
            //         .HasForeignKey(d => d.ServiceBookingId)
            //         .OnDelete(DeleteBehavior.Cascade);
                    
            //     entity.HasOne(d => d.Driver)
            //         .WithMany()
            //         .HasForeignKey(d => d.DriverId)
            //         .OnDelete(DeleteBehavior.Restrict);
            // });

            // // Configure VehicleInspection entity
            // modelBuilder.Entity<VehicleInspection>(entity =>
            // {
            //     entity.HasIndex(e => e.ServiceBookingId);
            //     entity.HasIndex(e => e.InspectorId);
            //     entity.HasIndex(e => e.CheckInTime);
            //     entity.HasIndex(e => e.InspectionStatus);
                
            //     entity.Property(e => e.InspectorId).IsRequired().HasMaxLength(450);
            //     entity.Property(e => e.InspectionStatus).IsRequired().HasMaxLength(50);
            //     entity.Property(e => e.InspectionReportUrl).HasMaxLength(500);
                
            //     entity.HasOne(d => d.ServiceBooking)
            //         .WithMany(p => p.VehicleInspections)
            //         .HasForeignKey(d => d.ServiceBookingId)
            //         .OnDelete(DeleteBehavior.Cascade);
                    
            //     entity.HasOne(d => d.Inspector)
            //         .WithMany()
            //         .HasForeignKey(d => d.InspectorId)
            //         .OnDelete(DeleteBehavior.Restrict);
            // });

            // // Configure ServiceExecution entity
            // modelBuilder.Entity<ServiceExecution>(entity =>
            // {
            //     entity.HasIndex(e => e.ServiceBookingId);
            //     entity.HasIndex(e => e.TechnicianId);
            //     entity.HasIndex(e => e.StartTime);
            //     entity.HasIndex(e => e.ExecutionStatus);
                
            //     entity.Property(e => e.TechnicianId).IsRequired().HasMaxLength(450);
            //     entity.Property(e => e.ApprovedBy).HasMaxLength(450);
            //     entity.Property(e => e.ExecutionStatus).IsRequired().HasMaxLength(50);
            //     entity.Property(e => e.LaborHours).HasPrecision(5, 2);
            //     entity.Property(e => e.LaborRate).HasPrecision(10, 2);
            //     entity.Property(e => e.TotalCost).HasPrecision(10, 2);
                
            //     entity.HasOne(d => d.ServiceBooking)
            //         .WithMany(p => p.ServiceExecutions)
            //         .HasForeignKey(d => d.ServiceBookingId)
            //         .OnDelete(DeleteBehavior.Cascade);
                    
            //     entity.HasOne(d => d.Technician)
            //         .WithMany()
            //         .HasForeignKey(d => d.TechnicianId)
            //         .OnDelete(DeleteBehavior.Restrict);
                    
            //     entity.HasOne(d => d.ApprovedByUser)
            //         .WithMany()
            //         .HasForeignKey(d => d.ApprovedBy)
            //         .OnDelete(DeleteBehavior.Restrict);
            // });

            // // Configure ServiceInvoice entity
            // modelBuilder.Entity<ServiceInvoice>(entity =>
            // {
            //     entity.HasIndex(e => e.ServiceBookingId);
            //     entity.HasIndex(e => e.ServiceExecutionId);
            //     entity.HasIndex(e => e.InvoiceNumber).IsUnique();
            //     entity.HasIndex(e => e.InvoiceStatus);
                
            //     entity.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(20);
            //     entity.Property(e => e.InvoiceStatus).IsRequired().HasMaxLength(50);
            //     entity.Property(e => e.PartsCost).HasPrecision(10, 2);
            //     entity.Property(e => e.LaborCost).HasPrecision(10, 2);
            //     entity.Property(e => e.AdditionalFees).HasPrecision(10, 2);
            //     entity.Property(e => e.Subtotal).HasPrecision(10, 2);
            //     entity.Property(e => e.TaxAmount).HasPrecision(10, 2);
            //     entity.Property(e => e.TotalAmount).HasPrecision(10, 2);
                
            //     entity.HasOne(d => d.ServiceBooking)
            //         .WithMany(p => p.ServiceInvoices)
            //         .HasForeignKey(d => d.ServiceBookingId)
            //         .OnDelete(DeleteBehavior.Cascade);
                    
            //     entity.HasOne(d => d.ServiceExecution)
            //         .WithMany()
            //         .HasForeignKey(d => d.ServiceExecutionId)
            //         .OnDelete(DeleteBehavior.Restrict);
            // });

            // // Configure ServicePayment entity
            // modelBuilder.Entity<ServicePayment>(entity =>
            // {
            //     entity.HasIndex(e => e.ServiceBookingId);
            //     entity.HasIndex(e => e.ServiceInvoiceId);
            //     entity.HasIndex(e => e.CustomerId);
            //     entity.HasIndex(e => e.PaymentStatus);
            //     entity.HasIndex(e => e.TransactionId);
                
            //     entity.Property(e => e.CustomerId).IsRequired().HasMaxLength(450);
            //     entity.Property(e => e.PaymentMethod).IsRequired().HasMaxLength(50);
            //     entity.Property(e => e.PaymentStatus).IsRequired().HasMaxLength(50);
            //     entity.Property(e => e.TransactionId).HasMaxLength(100);
            //     entity.Property(e => e.ProofOfPaymentUrl).HasMaxLength(500);
            //     entity.Property(e => e.ReceiptUrl).HasMaxLength(500);
            //     entity.Property(e => e.PaymentAmount).HasPrecision(10, 2);
                
            //     entity.HasOne(d => d.ServiceBooking)
            //         .WithMany(p => p.ServicePayments)
            //         .HasForeignKey(d => d.ServiceBookingId)
            //         .OnDelete(DeleteBehavior.Cascade);
                    
            //     entity.HasOne(d => d.ServiceInvoice)
            //         .WithMany(p => p.ServicePayments)
            //         .HasForeignKey(d => d.ServiceInvoiceId)
            //         .OnDelete(DeleteBehavior.Restrict);
                    
            //     entity.HasOne(d => d.Customer)
            //         .WithMany()
            //         .HasForeignKey(d => d.CustomerId)
            //         .OnDelete(DeleteBehavior.Restrict);
            // });
            

            // Configure EmploymentOffer entity
            modelBuilder.Entity<EmploymentOffer>(entity =>
            {
                entity.HasIndex(e => e.ApplicationId);
                entity.HasIndex(e => e.AccessToken).IsUnique();
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.OfferSentDate);
                entity.HasIndex(e => e.OfferExpiryDate);
                entity.HasIndex(e => e.ContractAccepted);
                entity.HasIndex(e => e.IsActive);
                
                entity.Property(e => e.JobTitle).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Department).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SalaryOffered).HasPrecision(18, 2);
                entity.Property(e => e.EmploymentType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.WorkLocation).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ContractFilePath).HasMaxLength(500);
                entity.Property(e => e.AccessToken).IsRequired().HasMaxLength(40);
                entity.Property(e => e.RejectionReason).HasMaxLength(1000);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                
                entity.HasOne(d => d.Application)
                    .WithMany()
                    .HasForeignKey(d => d.ApplicationId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(d => d.PreEmploymentDocumentation)
                    .WithOne(p => p.EmploymentOffer)
                    .HasForeignKey<PreEmploymentDocumentation>(d => d.OfferId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure PreEmploymentDocumentation entity
            modelBuilder.Entity<PreEmploymentDocumentation>(entity =>
            {
                entity.HasIndex(e => e.OfferId);
                entity.HasIndex(e => e.IdNumber);
                entity.HasIndex(e => e.IsCompleted);
                entity.HasIndex(e => e.AdminReviewed);
                entity.HasIndex(e => e.Approved);
                entity.HasIndex(e => e.CompletedDate);
                entity.HasIndex(e => e.IsActive);
                
                entity.Property(e => e.IdNumber).IsRequired().HasMaxLength(13);
                entity.Property(e => e.Gender).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Nationality).IsRequired().HasMaxLength(50);
                entity.Property(e => e.MaritalStatus).IsRequired().HasMaxLength(20);
                entity.Property(e => e.ResidentialAddress).IsRequired().HasMaxLength(500);
                entity.Property(e => e.City).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Province).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PostalCode).IsRequired().HasMaxLength(10);
                entity.Property(e => e.AlternativePhoneNumber).HasMaxLength(20);
                entity.Property(e => e.EmergencyContactName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EmergencyContactRelationship).IsRequired().HasMaxLength(50);
                entity.Property(e => e.EmergencyContactPhone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.EmergencyContactAddress).HasMaxLength(500);
                entity.Property(e => e.SecondaryEmergencyContactPhone).HasMaxLength(20);
                entity.Property(e => e.SecondaryEmergencyContactName).HasMaxLength(100);
                entity.Property(e => e.SecondaryEmergencyContactRelationship).HasMaxLength(50);
                entity.Property(e => e.SecondaryEmergencyContactAddress).HasMaxLength(500);
                entity.Property(e => e.BankName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.AccountType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.AccountNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.BranchCode).IsRequired().HasMaxLength(6);
                entity.Property(e => e.AccountHolderName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.BankStatementPath).HasMaxLength(500);
                entity.Property(e => e.TaxNumber).HasMaxLength(20);
                entity.Property(e => e.TaxClearancePath).HasMaxLength(500);
                entity.Property(e => e.TaxDirectiveNumber).HasMaxLength(50);
                entity.Property(e => e.MedicalAidProvider).HasMaxLength(100);
                entity.Property(e => e.MedicalAidNumber).HasMaxLength(50);
                entity.Property(e => e.MedicalAidMemberType).HasMaxLength(20);
                entity.Property(e => e.ChronicConditionsDetails).HasMaxLength(500);
                entity.Property(e => e.DisabilitiesDetails).HasMaxLength(500);
                entity.Property(e => e.CertifiedIdPath).HasMaxLength(500);
                entity.Property(e => e.ProofOfAddressPath).HasMaxLength(500);
                entity.Property(e => e.DriversLicensePath).HasMaxLength(500);
                entity.Property(e => e.CriminalRecordDetails).HasMaxLength(1000);
                entity.Property(e => e.ReviewedBy).HasMaxLength(450);
                entity.Property(e => e.AdminNotes).HasMaxLength(1000);
                entity.Property(e => e.CorrectionRequests).HasMaxLength(1000);
                
                entity.HasOne(d => d.EmploymentOffer)
                    .WithOne(p => p.PreEmploymentDocumentation)
                    .HasForeignKey<PreEmploymentDocumentation>(d => d.OfferId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(d => d.Reviewer)
                    .WithMany()
                    .HasForeignKey(d => d.ReviewedBy)
                    .OnDelete(DeleteBehavior.SetNull);
            });
                // Configure ServiceBooking entity
            modelBuilder.Entity<ServiceBooking>(entity =>
            {
                entity.HasIndex(e => e.Reference).IsUnique();
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.PreferredDate);
                entity.HasIndex(e => e.CreatedAtUtc);

                entity.Property(e => e.EstimatedCost).HasPrecision(18, 2);
                entity.Property(e => e.CustomerEmail).HasMaxLength(120);
                entity.Property(e => e.CustomerName).HasMaxLength(80);
                entity.Property(e => e.Reference).HasMaxLength(30);
                entity.Property(e => e.Notes).HasMaxLength(500);
            });

            // Configure Mechanic entity
            modelBuilder.Entity<Mechanic>(entity =>
            {
                    entity.HasIndex(e => e.Name);
                    entity.HasIndex(e => e.IsAvailable);

                    var stringListConverter = new ValueConverter<List<string>, string>(
                        v => v == null ? string.Empty : string.Join('|', v),
                        v => string.IsNullOrEmpty(v) ? new List<string>() : v.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList()
                    );
                    var stringListComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                        (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                        c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c == null ? new List<string>() : c.ToList()
                    );

                    entity.Property(e => e.Skills)
                        .HasConversion(stringListConverter)
                        .Metadata.SetValueComparer(stringListComparer);
            });

            // Configure ServiceJob entity
            modelBuilder.Entity<ServiceJob>(entity =>
            {
                    entity.HasIndex(e => e.MechanicId);
                    entity.HasIndex(e => e.ScheduledDate);
                    entity.HasIndex(e => new { e.ScheduledDate, e.MechanicId });

                    entity.HasOne(j => j.ServiceBooking)
                        .WithOne(b => b.ServiceJob)
                        .HasForeignKey<ServiceJob>(j => j.ServiceBookingId)
                        .OnDelete(DeleteBehavior.Cascade);

                    entity.HasOne(j => j.Mechanic)
                        .WithMany(m => m.ServiceJobs)
                        .HasForeignKey(j => j.MechanicId)
                        .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure MechanicUser link entity
            modelBuilder.Entity<MechanicUser>(entity =>
            {
                entity.HasIndex(e => e.UserId).IsUnique();

                entity.HasOne(mu => mu.Mechanic)
                    .WithMany()
                    .HasForeignKey(mu => mu.MechanicId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            

            // Seed data for roles
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed roles
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>()
                .HasData(
                    new Microsoft.AspNetCore.Identity.IdentityRole
                    {
                        Id = "1",
                        Name = "Administrator",
                        NormalizedName = "ADMINISTRATOR"
                    },
                    new Microsoft.AspNetCore.Identity.IdentityRole
                    {
                        Id = "2",
                        Name = "Customer",
                        NormalizedName = "CUSTOMER"
                    },
                    new Microsoft.AspNetCore.Identity.IdentityRole
                    {
                        Id = "3",
                        Name = "SalesRepresentative",
                        NormalizedName = "SALESREPRESENTATIVE"
                    },
                    new Microsoft.AspNetCore.Identity.IdentityRole
                    {
                        Id = "4",
                        Name = "SupportStaff",
                        NormalizedName = "SUPPORTSTAFF"
                    },
                    new Microsoft.AspNetCore.Identity.IdentityRole
                    {
                        Id = "5",
                        Name = "Driver",
                        NormalizedName = "DRIVER"
                    },
                    new Microsoft.AspNetCore.Identity.IdentityRole
                    {
                        Id = "6",
                        Name = "Recruiter",
                        NormalizedName = "RECRUITER"
                    },
                    new Microsoft.AspNetCore.Identity.IdentityRole
                    {
                        Id = "7",
                        Name = "Applicant",
                        NormalizedName = "APPLICANT"
                    }
                );

                // Seed initial mechanics
                modelBuilder.Entity<Mechanic>()
                    .HasData(
                        new Mechanic
                        {
                            Id = 1,
                            Name = "Alice Khumalo",
                            Skills = new List<string> { "Engine", "Diagnostics", "Maintenance" },
                            Rating = 4.7,
                            IsAvailable = true
                        },
                        new Mechanic
                        {
                            Id = 2,
                            Name = "Brian Moyo",
                            Skills = new List<string> { "Electrical", "Brakes" },
                            Rating = 4.3,
                            IsAvailable = true
                        },
                        new Mechanic
                        {
                            Id = 3,
                            Name = "Cindy Dlamini",
                            Skills = new List<string> { "Suspension", "Transmission" },
                            Rating = 4.5,
                            IsAvailable = false
                        }
                    );

                // Configure ServiceChecklist entity
                modelBuilder.Entity<ServiceChecklist>(entity =>
                {
                    entity.HasKey(e => e.Id);
                    entity.HasIndex(e => e.ServiceJobId);
                    entity.HasIndex(e => e.MechanicId);
                    entity.HasIndex(e => e.IsCompleted);
                    entity.HasIndex(e => e.StartedAt);
                    
                    entity.HasOne(e => e.ServiceJob)
                        .WithMany()
                        .HasForeignKey(e => e.ServiceJobId)
                        .OnDelete(DeleteBehavior.Cascade);
                        
                    entity.HasOne(e => e.Mechanic)
                        .WithMany()
                        .HasForeignKey(e => e.MechanicId)
                        .OnDelete(DeleteBehavior.Restrict);
                        
                    entity.HasMany(e => e.Items)
                        .WithOne(i => i.ServiceChecklist)
                        .HasForeignKey(i => i.ServiceChecklistId)
                        .OnDelete(DeleteBehavior.Cascade);
                        
                    entity.HasMany(e => e.Photos)
                        .WithOne(p => p.ServiceChecklist)
                        .HasForeignKey(p => p.ServiceChecklistId)
                        .OnDelete(DeleteBehavior.Cascade);
                });

                // Configure ServiceChecklistItem entity
                modelBuilder.Entity<ServiceChecklistItem>(entity =>
                {
                    entity.HasKey(e => e.Id);
                    entity.HasIndex(e => e.ServiceChecklistId);
                    entity.HasIndex(e => e.IsCompleted);
                });

                // Configure ServicePhoto entity
                modelBuilder.Entity<ServicePhoto>(entity =>
                {
                    entity.HasKey(e => e.Id);
                    entity.HasIndex(e => e.ServiceChecklistId);
                    entity.HasIndex(e => e.PhotoType);
                    entity.HasIndex(e => e.TakenAt);
                });
        }
    }
}