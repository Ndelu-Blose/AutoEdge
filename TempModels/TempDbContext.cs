using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AutoEdge.TempModels;

public partial class TempDbContext : DbContext
{
    public TempDbContext()
    {
    }

    public TempDbContext(DbContextOptions<TempDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Contract> Contracts { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Delivery> Deliveries { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<DocumentType> DocumentTypes { get; set; }

    public virtual DbSet<Inquiry> Inquiries { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Purchase> Purchases { get; set; }

    public virtual DbSet<PurchaseStatusHistory> PurchaseStatusHistories { get; set; }

    public virtual DbSet<Reservation> Reservations { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }

    public virtual DbSet<VehicleImage> VehicleImages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=AutoEdgeDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.Email, "IX_AspNetUsers_Email")
                .IsUnique()
                .HasFilter("([Email] IS NOT NULL)");

            entity.HasIndex(e => e.IsActive, "IX_AspNetUsers_IsActive");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.Country).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.ProfileImagePath).HasMaxLength(500);
            entity.Property(e => e.State).HasMaxLength(50);
            entity.Property(e => e.UserName).HasMaxLength(256);
            entity.Property(e => e.ZipCode).HasMaxLength(10);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.ProviderKey).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.Name).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasIndex(e => e.ContractNumber, "IX_Contracts_ContractNumber").IsUnique();

            entity.HasIndex(e => e.CreatedByUserId, "IX_Contracts_CreatedByUserId");

            entity.HasIndex(e => e.CreatedDate, "IX_Contracts_CreatedDate");

            entity.HasIndex(e => e.CustomerId, "IX_Contracts_CustomerId");

            entity.HasIndex(e => e.PurchaseId, "IX_Contracts_PurchaseId");

            entity.HasIndex(e => e.Status, "IX_Contracts_Status");

            entity.HasIndex(e => e.VehicleId, "IX_Contracts_VehicleId");

            entity.Property(e => e.CertificateUrl).HasMaxLength(1000);
            entity.Property(e => e.ContractNumber).HasMaxLength(50);
            entity.Property(e => e.ContractPath).HasMaxLength(500);
            entity.Property(e => e.DeliveryAddress).HasMaxLength(500);
            entity.Property(e => e.DeliveryMethod).HasMaxLength(20);
            entity.Property(e => e.DigitalSignatureData).HasMaxLength(500);
            entity.Property(e => e.DocumentationFee).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DownPayment).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ExtendedWarrantyFee).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.FinancedAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.InterestRate).HasColumnType("decimal(5, 4)");
            entity.Property(e => e.LenderName).HasMaxLength(100);
            entity.Property(e => e.LoanNumber).HasMaxLength(100);
            entity.Property(e => e.MonthlyPayment).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.OpenSignDocumentId).HasMaxLength(100);
            entity.Property(e => e.OtherFees).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PaymentType).HasMaxLength(20);
            entity.Property(e => e.RegistrationFee).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SignedContractPath).HasMaxLength(500);
            entity.Property(e => e.SignedDocumentUrl).HasMaxLength(1000);
            entity.Property(e => e.SigningRequestId).HasMaxLength(100);
            entity.Property(e => e.SigningStatus).HasMaxLength(50);
            entity.Property(e => e.SigningUrl).HasMaxLength(1000);
            entity.Property(e => e.SpecialTerms).HasMaxLength(1000);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TradeInValue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TradeInVehicle).HasMaxLength(100);

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Customer).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Purchase).WithMany(p => p.Contracts).HasForeignKey(d => d.PurchaseId);

            entity.HasOne(d => d.Vehicle).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasIndex(e => e.CustomerStatus, "IX_Customers_CustomerStatus");

            entity.HasIndex(e => e.IsActive, "IX_Customers_IsActive");

            entity.HasIndex(e => e.UserId, "IX_Customers_UserId").IsUnique();

            entity.Property(e => e.AnnualIncome).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BillingAddress).HasMaxLength(500);
            entity.Property(e => e.CustomerStatus).HasMaxLength(20);
            entity.Property(e => e.CustomerType).HasMaxLength(20);
            entity.Property(e => e.DeliveryAddress).HasMaxLength(500);
            entity.Property(e => e.EmergencyContactName).HasMaxLength(100);
            entity.Property(e => e.EmergencyContactPhone).HasMaxLength(20);
            entity.Property(e => e.EmergencyContactRelation).HasMaxLength(100);
            entity.Property(e => e.Employer).HasMaxLength(100);
            entity.Property(e => e.JobTitle).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.PreferredContact).HasMaxLength(20);
            entity.Property(e => e.TotalSpent).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.User).WithOne(p => p.Customer).HasForeignKey<Customer>(d => d.UserId);
        });

        modelBuilder.Entity<Delivery>(entity =>
        {
            entity.HasIndex(e => e.ContractId, "IX_Deliveries_ContractId");

            entity.HasIndex(e => e.DriverUserId, "IX_Deliveries_DriverUserId");

            entity.HasIndex(e => e.ScheduledDate, "IX_Deliveries_ScheduledDate");

            entity.HasIndex(e => e.Status, "IX_Deliveries_Status");

            entity.HasIndex(e => e.TrackingNumber, "IX_Deliveries_TrackingNumber");

            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.ContactPersonEmail).HasMaxLength(100);
            entity.Property(e => e.ContactPersonName).HasMaxLength(100);
            entity.Property(e => e.ContactPersonPhone).HasMaxLength(20);
            entity.Property(e => e.Country).HasMaxLength(50);
            entity.Property(e => e.CurrentLatitude).HasColumnType("decimal(10, 8)");
            entity.Property(e => e.CurrentLongitude).HasColumnType("decimal(11, 8)");
            entity.Property(e => e.CustomerFeedback).HasMaxLength(1000);
            entity.Property(e => e.DeliveryAddress).HasMaxLength(500);
            entity.Property(e => e.DeliveryCompany).HasMaxLength(100);
            entity.Property(e => e.DeliveryFee).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DeliveryInstructions).HasMaxLength(500);
            entity.Property(e => e.DeliveryPhotoPath).HasMaxLength(500);
            entity.Property(e => e.DeliveryType).HasMaxLength(50);
            entity.Property(e => e.DistanceRemaining).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.DriverName).HasMaxLength(100);
            entity.Property(e => e.DriverPhone).HasMaxLength(20);
            entity.Property(e => e.FailureReason).HasMaxLength(500);
            entity.Property(e => e.InsuranceAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.InsurancePolicyNumber).HasMaxLength(100);
            entity.Property(e => e.InsuranceProvider).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.RescheduleReason).HasMaxLength(500);
            entity.Property(e => e.SignaturePath).HasMaxLength(500);
            entity.Property(e => e.SignedByName).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.TrackingNumber).HasMaxLength(100);
            entity.Property(e => e.VehicleLicensePlate).HasMaxLength(100);
            entity.Property(e => e.ZipCode).HasMaxLength(10);

            entity.HasOne(d => d.Contract).WithMany(p => p.Deliveries)
                .HasForeignKey(d => d.ContractId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.DriverUser).WithMany(p => p.Deliveries)
                .HasForeignKey(d => d.DriverUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasIndex(e => e.CustomerId, "IX_Documents_CustomerId");

            entity.HasIndex(e => e.DocumentTypeId, "IX_Documents_DocumentTypeId");

            entity.HasIndex(e => e.IsActive, "IX_Documents_IsActive");

            entity.HasIndex(e => e.PurchaseId, "IX_Documents_PurchaseId");

            entity.HasIndex(e => e.ReviewedBy, "IX_Documents_ReviewedBy");

            entity.HasIndex(e => e.Status, "IX_Documents_Status");

            entity.HasIndex(e => e.UploadDate, "IX_Documents_UploadDate");

            entity.Property(e => e.ContentType).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DocumentNumber).HasMaxLength(100);
            entity.Property(e => e.EncryptionKey).HasMaxLength(100);
            entity.Property(e => e.ExtractedText).HasMaxLength(2000);
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.IssuingAuthority).HasMaxLength(100);
            entity.Property(e => e.OcrValidationResults).HasMaxLength(1000);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.ValidationNotes).HasMaxLength(1000);

            entity.HasOne(d => d.Customer).WithMany(p => p.Documents)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.DocumentType).WithMany(p => p.Documents)
                .HasForeignKey(d => d.DocumentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Purchase).WithMany(p => p.Documents).HasForeignKey(d => d.PurchaseId);

            entity.HasOne(d => d.ReviewedByNavigation).WithMany(p => p.Documents)
                .HasForeignKey(d => d.ReviewedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<DocumentType>(entity =>
        {
            entity.HasIndex(e => e.IsActive, "IX_DocumentTypes_IsActive");

            entity.HasIndex(e => e.IsRequired, "IX_DocumentTypes_IsRequired");

            entity.HasIndex(e => e.Name, "IX_DocumentTypes_Name").IsUnique();

            entity.Property(e => e.AllowedFileTypes).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.MaxFileSizeMb).HasColumnName("MaxFileSizeMB");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Requirements).HasMaxLength(1000);
        });

        modelBuilder.Entity<Inquiry>(entity =>
        {
            entity.HasIndex(e => e.AssignedToUserId, "IX_Inquiries_AssignedToUserId");

            entity.HasIndex(e => e.CreatedDate, "IX_Inquiries_CreatedDate");

            entity.HasIndex(e => e.CustomerId, "IX_Inquiries_CustomerId");

            entity.HasIndex(e => e.Status, "IX_Inquiries_Status");

            entity.HasIndex(e => e.VehicleId, "IX_Inquiries_VehicleId");

            entity.Property(e => e.CustomerEmail).HasMaxLength(100);
            entity.Property(e => e.CustomerName).HasMaxLength(100);
            entity.Property(e => e.CustomerPhone).HasMaxLength(20);
            entity.Property(e => e.InquiryType).HasMaxLength(50);
            entity.Property(e => e.Message).HasMaxLength(1000);
            entity.Property(e => e.OfferedPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PreferredContactMethod).HasMaxLength(20);
            entity.Property(e => e.Priority).HasMaxLength(20);
            entity.Property(e => e.Response).HasMaxLength(1000);
            entity.Property(e => e.SpecialRequests).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.AssignedToUser).WithMany(p => p.Inquiries)
                .HasForeignKey(d => d.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Customer).WithMany(p => p.Inquiries)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Vehicle).WithMany(p => p.Inquiries)
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasIndex(e => e.ContractId, "IX_Payments_ContractId");

            entity.HasIndex(e => e.PaymentType, "IX_Payments_PaymentType");

            entity.HasIndex(e => e.ProcessedDate, "IX_Payments_ProcessedDate");

            entity.HasIndex(e => e.Status, "IX_Payments_Status");

            entity.HasIndex(e => e.TransactionId, "IX_Payments_TransactionId");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BankName).HasMaxLength(100);
            entity.Property(e => e.CardBrand).HasMaxLength(50);
            entity.Property(e => e.CardLastFour).HasMaxLength(4);
            entity.Property(e => e.CheckNumber).HasMaxLength(50);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.FailureReason).HasMaxLength(500);
            entity.Property(e => e.LateFee).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NetAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.PaymentType).HasMaxLength(50);
            entity.Property(e => e.ProcessingFee).HasColumnType("decimal(5, 4)");
            entity.Property(e => e.ReceiptPath).HasMaxLength(500);
            entity.Property(e => e.RecurringScheduleId).HasMaxLength(100);
            entity.Property(e => e.RefundAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RefundReason).HasMaxLength(500);
            entity.Property(e => e.RefundTransactionId).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.StripePaymentIntentId).HasMaxLength(100);
            entity.Property(e => e.TransactionId).HasMaxLength(100);

            entity.HasOne(d => d.Contract).WithMany(p => p.Payments)
                .HasForeignKey(d => d.ContractId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Purchase>(entity =>
        {
            entity.HasIndex(e => e.CustomerId, "IX_Purchases_CustomerId");

            entity.HasIndex(e => e.InitiatedDate, "IX_Purchases_InitiatedDate");

            entity.HasIndex(e => e.IsActive, "IX_Purchases_IsActive");

            entity.HasIndex(e => e.Status, "IX_Purchases_Status");

            entity.HasIndex(e => e.VehicleId, "IX_Purchases_VehicleId");

            entity.Property(e => e.AssignedSalesRep).HasMaxLength(100);
            entity.Property(e => e.CancellationReason).HasMaxLength(500);
            entity.Property(e => e.DeliveryAddress).HasMaxLength(500);
            entity.Property(e => e.DeliveryMethod).HasMaxLength(100);
            entity.Property(e => e.DepositAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.FinancingProvider).HasMaxLength(100);
            entity.Property(e => e.InterestRate).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.MonthlyPayment).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.PurchasePrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RemainingAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Customer).WithMany(p => p.Purchases).HasForeignKey(d => d.CustomerId);

            entity.HasOne(d => d.Vehicle).WithMany(p => p.Purchases)
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<PurchaseStatusHistory>(entity =>
        {
            entity.HasIndex(e => e.ChangedDate, "IX_PurchaseStatusHistories_ChangedDate");

            entity.HasIndex(e => e.PurchaseId, "IX_PurchaseStatusHistories_PurchaseId");

            entity.HasIndex(e => new { e.PurchaseId, e.ChangedDate }, "IX_PurchaseStatusHistories_PurchaseId_ChangedDate");

            entity.Property(e => e.ChangeReason).HasMaxLength(50);
            entity.Property(e => e.ChangedBy).HasMaxLength(100);
            entity.Property(e => e.FromStatus).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.ToStatus).HasMaxLength(50);

            entity.HasOne(d => d.Purchase).WithMany(p => p.PurchaseStatusHistories).HasForeignKey(d => d.PurchaseId);
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasIndex(e => e.CancelledByUserId, "IX_Reservations_CancelledByUserId");

            entity.HasIndex(e => e.ContractId, "IX_Reservations_ContractId");

            entity.HasIndex(e => e.CustomerId, "IX_Reservations_CustomerId");

            entity.HasIndex(e => e.ExpiryDate, "IX_Reservations_ExpiryDate");

            entity.HasIndex(e => e.ReservationNumber, "IX_Reservations_ReservationNumber").IsUnique();

            entity.HasIndex(e => e.Status, "IX_Reservations_Status");

            entity.HasIndex(e => e.VehicleId, "IX_Reservations_VehicleId");

            entity.Property(e => e.CancellationReason).HasMaxLength(500);
            entity.Property(e => e.DepositAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.PaymentStatus).HasMaxLength(20);
            entity.Property(e => e.PaymentTransactionId).HasMaxLength(100);
            entity.Property(e => e.RefundAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RefundReason).HasMaxLength(500);
            entity.Property(e => e.ReservationNumber).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.CancelledByUser).WithMany(p => p.Reservations).HasForeignKey(d => d.CancelledByUserId);

            entity.HasOne(d => d.Contract).WithMany(p => p.Reservations).HasForeignKey(d => d.ContractId);

            entity.HasOne(d => d.Customer).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Vehicle).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasIndex(e => e.DateListed, "IX_Vehicles_DateListed");

            entity.HasIndex(e => e.IsActive, "IX_Vehicles_IsActive");

            entity.HasIndex(e => new { e.Make, e.Model, e.Year }, "IX_Vehicles_Make_Model_Year");

            entity.HasIndex(e => e.Status, "IX_Vehicles_Status");

            entity.HasIndex(e => e.Vin, "IX_Vehicles_VIN").IsUnique();

            entity.Property(e => e.Condition).HasMaxLength(20);
            entity.Property(e => e.CostPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.EngineType).HasMaxLength(50);
            entity.Property(e => e.ExteriorColor).HasMaxLength(30);
            entity.Property(e => e.InteriorColor).HasMaxLength(30);
            entity.Property(e => e.LicensePlate).HasMaxLength(20);
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.Make).HasMaxLength(50);
            entity.Property(e => e.Model).HasMaxLength(50);
            entity.Property(e => e.NegotiableRange).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SellingPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Source).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Transmission).HasMaxLength(50);
            entity.Property(e => e.Vin)
                .HasMaxLength(17)
                .HasColumnName("VIN");
        });

        modelBuilder.Entity<VehicleImage>(entity =>
        {
            entity.HasIndex(e => e.DisplayOrder, "IX_VehicleImages_DisplayOrder");

            entity.HasIndex(e => e.ImageType, "IX_VehicleImages_ImageType");

            entity.HasIndex(e => e.VehicleId, "IX_VehicleImages_VehicleId");

            entity.Property(e => e.AltText).HasMaxLength(500);
            entity.Property(e => e.ContentType).HasMaxLength(100);
            entity.Property(e => e.FileName).HasMaxLength(100);
            entity.Property(e => e.ImagePath).HasMaxLength(500);
            entity.Property(e => e.ImageType).HasMaxLength(50);

            entity.HasOne(d => d.Vehicle).WithMany(p => p.VehicleImages).HasForeignKey(d => d.VehicleId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
