CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [FirstName] nvarchar(50) NOT NULL,
    [LastName] nvarchar(50) NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [LastLoginDate] datetime2 NULL,
    [IsActive] bit NOT NULL,
    [ProfileImagePath] nvarchar(500) NOT NULL,
    [Address] nvarchar(200) NOT NULL,
    [City] nvarchar(50) NOT NULL,
    [State] nvarchar(50) NOT NULL,
    [ZipCode] nvarchar(10) NOT NULL,
    [Country] nvarchar(50) NOT NULL,
    [DateOfBirth] datetime2 NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [DocumentTypes] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [IsRequired] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [AllowedFileTypes] nvarchar(200) NULL,
    [MaxFileSizeMB] int NOT NULL,
    [Requirements] nvarchar(1000) NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    CONSTRAINT [PK_DocumentTypes] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Vehicles] (
    [Id] int NOT NULL IDENTITY,
    [VIN] nvarchar(17) NOT NULL,
    [LicensePlate] nvarchar(20) NOT NULL,
    [Make] nvarchar(50) NOT NULL,
    [Model] nvarchar(50) NOT NULL,
    [Year] int NOT NULL,
    [Mileage] int NOT NULL,
    [EngineType] nvarchar(50) NOT NULL,
    [Transmission] nvarchar(50) NOT NULL,
    [InteriorColor] nvarchar(30) NOT NULL,
    [ExteriorColor] nvarchar(30) NOT NULL,
    [Condition] nvarchar(20) NOT NULL,
    [Features] nvarchar(max) NOT NULL,
    [CostPrice] decimal(18,2) NOT NULL,
    [SellingPrice] decimal(18,2) NOT NULL,
    [NegotiableRange] decimal(18,2) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [Location] nvarchar(100) NOT NULL,
    [DateListed] datetime2 NOT NULL,
    [Source] nvarchar(100) NOT NULL,
    [PurchaseDate] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Vehicles] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(128) NOT NULL,
    [ProviderKey] nvarchar(128) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(128) NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Customers] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [CustomerType] nvarchar(20) NOT NULL,
    [CreditScore] int NULL,
    [PreferredContact] nvarchar(20) NOT NULL,
    [DeliveryAddress] nvarchar(500) NOT NULL,
    [BillingAddress] nvarchar(500) NOT NULL,
    [EmergencyContactName] nvarchar(100) NOT NULL,
    [EmergencyContactPhone] nvarchar(20) NOT NULL,
    [EmergencyContactRelation] nvarchar(100) NOT NULL,
    [Employer] nvarchar(100) NOT NULL,
    [JobTitle] nvarchar(100) NOT NULL,
    [AnnualIncome] decimal(18,2) NULL,
    [Notes] nvarchar(1000) NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [CustomerStatus] nvarchar(20) NOT NULL,
    [LastPurchaseDate] datetime2 NULL,
    [TotalPurchases] int NOT NULL,
    [TotalSpent] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Customers_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [VehicleImages] (
    [Id] int NOT NULL IDENTITY,
    [VehicleId] int NOT NULL,
    [ImagePath] nvarchar(500) NOT NULL,
    [ImageType] nvarchar(50) NOT NULL,
    [UploadDate] datetime2 NOT NULL,
    [DisplayOrder] int NOT NULL,
    [IsActive] bit NOT NULL,
    [FileName] nvarchar(100) NOT NULL,
    [FileSize] bigint NOT NULL,
    [ContentType] nvarchar(100) NOT NULL,
    [AltText] nvarchar(500) NOT NULL,
    CONSTRAINT [PK_VehicleImages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_VehicleImages_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Inquiries] (
    [Id] int NOT NULL IDENTITY,
    [VehicleId] int NOT NULL,
    [CustomerId] int NOT NULL,
    [InquiryType] nvarchar(50) NOT NULL,
    [Message] nvarchar(1000) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [Priority] nvarchar(20) NOT NULL,
    [AssignedToUserId] nvarchar(450) NULL,
    [Response] nvarchar(1000) NOT NULL,
    [ResponseDate] datetime2 NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [CustomerName] nvarchar(100) NOT NULL,
    [CustomerEmail] nvarchar(100) NOT NULL,
    [CustomerPhone] nvarchar(20) NOT NULL,
    [PreferredContactMethod] nvarchar(20) NOT NULL,
    [PreferredContactTime] datetime2 NULL,
    [SpecialRequests] nvarchar(500) NOT NULL,
    [IsTestDriveRequested] bit NOT NULL,
    [PreferredTestDriveDate] datetime2 NULL,
    [IsFinancingInquiry] bit NOT NULL,
    [OfferedPrice] decimal(18,2) NULL,
    CONSTRAINT [PK_Inquiries] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Inquiries_AspNetUsers_AssignedToUserId] FOREIGN KEY ([AssignedToUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Inquiries_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Inquiries_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Purchases] (
    [Id] int NOT NULL IDENTITY,
    [CustomerId] int NOT NULL,
    [VehicleId] int NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [PurchasePrice] decimal(18,2) NOT NULL,
    [DepositAmount] decimal(18,2) NOT NULL,
    [RemainingAmount] decimal(18,2) NOT NULL,
    [PaymentMethod] nvarchar(50) NOT NULL,
    [FinancingProvider] nvarchar(100) NOT NULL,
    [InterestRate] decimal(5,2) NULL,
    [LoanTermMonths] int NULL,
    [MonthlyPayment] decimal(18,2) NULL,
    [InitiatedDate] datetime2 NOT NULL,
    [DocumentsSubmittedDate] datetime2 NULL,
    [DocumentsVerifiedDate] datetime2 NULL,
    [ContractGeneratedDate] datetime2 NULL,
    [ContractSignedDate] datetime2 NULL,
    [PaymentCompletedDate] datetime2 NULL,
    [CompletedDate] datetime2 NULL,
    [CancelledDate] datetime2 NULL,
    [Notes] nvarchar(500) NOT NULL,
    [CancellationReason] nvarchar(500) NOT NULL,
    [AssignedSalesRep] nvarchar(100) NOT NULL,
    [RequiresFinancing] bit NOT NULL,
    [DocumentsVerified] bit NOT NULL,
    [ContractSigned] bit NOT NULL,
    [PaymentCompleted] bit NOT NULL,
    [DeliveryMethod] nvarchar(100) NOT NULL,
    [DeliveryAddress] nvarchar(500) NOT NULL,
    [ScheduledDeliveryDate] datetime2 NULL,
    [ActualDeliveryDate] datetime2 NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Purchases] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Purchases_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Purchases_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Contracts] (
    [Id] int NOT NULL IDENTITY,
    [VehicleId] int NOT NULL,
    [CustomerId] int NOT NULL,
    [ContractNumber] nvarchar(50) NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [DownPayment] decimal(18,2) NOT NULL,
    [FinancedAmount] decimal(18,2) NOT NULL,
    [PaymentType] nvarchar(20) NOT NULL,
    [LoanDetails] nvarchar(max) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [SignedDate] datetime2 NULL,
    [ContractPath] nvarchar(500) NOT NULL,
    [SignedContractPath] nvarchar(500) NOT NULL,
    [CreatedByUserId] nvarchar(450) NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [InterestRate] decimal(5,4) NULL,
    [LoanTermMonths] int NULL,
    [MonthlyPayment] decimal(18,2) NULL,
    [LenderName] nvarchar(100) NOT NULL,
    [LoanNumber] nvarchar(100) NOT NULL,
    [TradeInValue] decimal(18,2) NULL,
    [TradeInVehicle] nvarchar(100) NOT NULL,
    [TaxAmount] decimal(18,2) NOT NULL,
    [RegistrationFee] decimal(18,2) NOT NULL,
    [DocumentationFee] decimal(18,2) NOT NULL,
    [ExtendedWarrantyFee] decimal(18,2) NOT NULL,
    [OtherFees] decimal(18,2) NOT NULL,
    [SpecialTerms] nvarchar(1000) NOT NULL,
    [Notes] nvarchar(1000) NOT NULL,
    [PurchaseId] int NULL,
    [DeliveryDate] datetime2 NULL,
    [DeliveryAddress] nvarchar(500) NOT NULL,
    [DeliveryMethod] nvarchar(20) NOT NULL,
    [IsDigitallySigned] bit NOT NULL,
    [DigitalSignatureData] nvarchar(500) NOT NULL,
    [OpenSignDocumentId] nvarchar(100) NULL,
    [SigningRequestId] nvarchar(100) NULL,
    [SigningUrl] nvarchar(1000) NULL,
    [SignedDocumentUrl] nvarchar(1000) NULL,
    [CertificateUrl] nvarchar(1000) NULL,
    [SigningRequestSentDate] datetime2 NULL,
    [SigningCompletedDate] datetime2 NULL,
    [SigningStatus] nvarchar(50) NULL,
    [CompletionDate] datetime2 NULL,
    CONSTRAINT [PK_Contracts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Contracts_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Contracts_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Contracts_Purchases_PurchaseId] FOREIGN KEY ([PurchaseId]) REFERENCES [Purchases] ([Id]),
    CONSTRAINT [FK_Contracts_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Documents] (
    [Id] int NOT NULL IDENTITY,
    [CustomerId] int NOT NULL,
    [DocumentTypeId] int NOT NULL,
    [FileName] nvarchar(255) NOT NULL,
    [FilePath] nvarchar(500) NOT NULL,
    [FileSize] bigint NOT NULL,
    [ContentType] nvarchar(100) NULL,
    [UploadDate] datetime2 NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [ValidationNotes] nvarchar(1000) NULL,
    [ReviewedBy] nvarchar(450) NULL,
    [ReviewDate] datetime2 NULL,
    [ExpiryDate] datetime2 NULL,
    [IsActive] bit NOT NULL,
    [ExtractedText] nvarchar(2000) NULL,
    [IsOcrProcessed] bit NOT NULL,
    [OcrProcessedDate] datetime2 NULL,
    [OcrValidationResults] nvarchar(1000) NULL,
    [Metadata] nvarchar(max) NOT NULL,
    [DocumentNumber] nvarchar(100) NOT NULL,
    [DocumentIssueDate] datetime2 NULL,
    [DocumentExpiryDate] datetime2 NULL,
    [IssuingAuthority] nvarchar(100) NOT NULL,
    [IsRequired] bit NOT NULL,
    [DisplayOrder] int NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [IsConfidential] bit NOT NULL,
    [EncryptionKey] nvarchar(100) NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    [PurchaseId] int NULL,
    CONSTRAINT [PK_Documents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Documents_AspNetUsers_ReviewedBy] FOREIGN KEY ([ReviewedBy]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Documents_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Documents_DocumentTypes_DocumentTypeId] FOREIGN KEY ([DocumentTypeId]) REFERENCES [DocumentTypes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Documents_Purchases_PurchaseId] FOREIGN KEY ([PurchaseId]) REFERENCES [Purchases] ([Id])
);
GO


CREATE TABLE [PurchaseStatusHistories] (
    [Id] int NOT NULL IDENTITY,
    [PurchaseId] int NOT NULL,
    [FromStatus] nvarchar(50) NOT NULL,
    [ToStatus] nvarchar(50) NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [Notes] nvarchar(500) NOT NULL,
    [ChangedBy] nvarchar(100) NOT NULL,
    [ChangeReason] nvarchar(50) NOT NULL,
    [ChangedDate] datetime2 NOT NULL,
    [IsSystemGenerated] bit NOT NULL,
    CONSTRAINT [PK_PurchaseStatusHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PurchaseStatusHistories_Purchases_PurchaseId] FOREIGN KEY ([PurchaseId]) REFERENCES [Purchases] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [CustomerFeedbacks] (
    [Id] int NOT NULL IDENTITY,
    [ContractId] int NOT NULL,
    [Rating] int NOT NULL,
    [ServiceRating] int NOT NULL,
    [VehicleRating] int NOT NULL,
    [Comments] nvarchar(2000) NULL,
    [RecommendToOthers] bit NOT NULL,
    [ImprovementSuggestions] nvarchar(500) NULL,
    [AllowFollowUp] bit NOT NULL,
    [SubmittedDate] datetime2 NOT NULL,
    [LastUpdatedDate] datetime2 NULL,
    [IsActive] bit NOT NULL,
    [ContractId1] int NULL,
    CONSTRAINT [PK_CustomerFeedbacks] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CustomerFeedbacks_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CustomerFeedbacks_Contracts_ContractId1] FOREIGN KEY ([ContractId1]) REFERENCES [Contracts] ([Id])
);
GO


CREATE TABLE [Deliveries] (
    [Id] int NOT NULL IDENTITY,
    [ContractId] int NOT NULL,
    [DeliveryType] nvarchar(50) NOT NULL,
    [ScheduledDate] datetime2 NOT NULL,
    [CompletedDate] datetime2 NULL,
    [DeliveryAddress] nvarchar(500) NOT NULL,
    [City] nvarchar(50) NOT NULL,
    [State] nvarchar(50) NOT NULL,
    [ZipCode] nvarchar(10) NOT NULL,
    [Country] nvarchar(50) NOT NULL,
    [DriverUserId] nvarchar(450) NULL,
    [DriverName] nvarchar(100) NOT NULL,
    [DriverPhone] nvarchar(20) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [TrackingNumber] nvarchar(100) NOT NULL,
    [Notes] nvarchar(1000) NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [DeliveryFee] decimal(18,2) NOT NULL,
    [DeliveryCompany] nvarchar(100) NOT NULL,
    [VehicleLicensePlate] nvarchar(100) NOT NULL,
    [PickupTime] datetime2 NULL,
    [EstimatedArrival] datetime2 NULL,
    [ActualArrival] datetime2 NULL,
    [DeliveryInstructions] nvarchar(500) NOT NULL,
    [ContactPersonName] nvarchar(100) NOT NULL,
    [ContactPersonPhone] nvarchar(20) NOT NULL,
    [ContactPersonEmail] nvarchar(100) NOT NULL,
    [RequiresSignature] bit NOT NULL,
    [SignaturePath] nvarchar(500) NOT NULL,
    [SignedByName] nvarchar(100) NOT NULL,
    [SignedDate] datetime2 NULL,
    [DeliveryPhotoPath] nvarchar(500) NOT NULL,
    [CustomerFeedback] nvarchar(1000) NOT NULL,
    [CustomerRating] int NOT NULL,
    [FailureReason] nvarchar(500) NOT NULL,
    [RescheduleCount] int NOT NULL,
    [LastRescheduleDate] datetime2 NULL,
    [RescheduleReason] nvarchar(500) NOT NULL,
    [IsInsured] bit NOT NULL,
    [InsuranceProvider] nvarchar(100) NOT NULL,
    [InsurancePolicyNumber] nvarchar(100) NOT NULL,
    [InsuranceAmount] decimal(18,2) NOT NULL,
    [CurrentLatitude] decimal(10,8) NULL,
    [CurrentLongitude] decimal(11,8) NULL,
    [LastLocationUpdate] datetime2 NULL,
    [DistanceRemaining] decimal(5,2) NULL,
    [EstimatedMinutesRemaining] int NULL,
    CONSTRAINT [PK_Deliveries] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Deliveries_AspNetUsers_DriverUserId] FOREIGN KEY ([DriverUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Deliveries_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Payments] (
    [Id] int NOT NULL IDENTITY,
    [ContractId] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [PaymentMethod] nvarchar(50) NOT NULL,
    [TransactionId] nvarchar(100) NOT NULL,
    [StripePaymentIntentId] nvarchar(100) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [ProcessedDate] datetime2 NOT NULL,
    [GatewayResponse] nvarchar(max) NOT NULL,
    [RefundAmount] decimal(18,2) NOT NULL,
    [RefundDate] datetime2 NULL,
    [RefundReason] nvarchar(500) NOT NULL,
    [RefundTransactionId] nvarchar(100) NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [PaymentType] nvarchar(50) NOT NULL,
    [CardLastFour] nvarchar(4) NOT NULL,
    [CardBrand] nvarchar(50) NOT NULL,
    [BankName] nvarchar(100) NOT NULL,
    [CheckNumber] nvarchar(50) NOT NULL,
    [ProcessingFee] decimal(5,4) NOT NULL,
    [NetAmount] decimal(18,2) NOT NULL,
    [Currency] nvarchar(3) NOT NULL,
    [Notes] nvarchar(1000) NOT NULL,
    [FailureReason] nvarchar(500) NOT NULL,
    [RetryCount] int NOT NULL,
    [NextRetryDate] datetime2 NULL,
    [IsRecurring] bit NOT NULL,
    [RecurringScheduleId] nvarchar(100) NOT NULL,
    [DueDate] datetime2 NULL,
    [IsOverdue] bit NOT NULL,
    [LateFee] decimal(18,2) NOT NULL,
    [ReceiptPath] nvarchar(500) NOT NULL,
    [ReceiptSent] bit NOT NULL,
    [ReceiptSentDate] datetime2 NULL,
    CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Payments_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Reservations] (
    [Id] int NOT NULL IDENTITY,
    [VehicleId] int NOT NULL,
    [CustomerId] int NOT NULL,
    [ReservationNumber] nvarchar(50) NOT NULL,
    [ReservationDate] datetime2 NOT NULL,
    [ExpiryDate] datetime2 NOT NULL,
    [DepositAmount] decimal(18,2) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [Notes] nvarchar(1000) NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [PaymentMethod] nvarchar(50) NOT NULL,
    [PaymentTransactionId] nvarchar(100) NOT NULL,
    [PaymentDate] datetime2 NULL,
    [PaymentStatus] nvarchar(20) NOT NULL,
    [RefundAmount] decimal(18,2) NULL,
    [RefundDate] datetime2 NULL,
    [RefundReason] nvarchar(500) NOT NULL,
    [ConvertedToContractDate] datetime2 NULL,
    [ContractId] int NULL,
    [CancellationReason] nvarchar(500) NOT NULL,
    [CancellationDate] datetime2 NULL,
    [CancelledByUserId] nvarchar(450) NULL,
    CONSTRAINT [PK_Reservations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Reservations_AspNetUsers_CancelledByUserId] FOREIGN KEY ([CancelledByUserId]) REFERENCES [AspNetUsers] ([Id]),
    CONSTRAINT [FK_Reservations_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([Id]),
    CONSTRAINT [FK_Reservations_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Reservations_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [ServiceReminders] (
    [Id] int NOT NULL IDENTITY,
    [ContractId] int NOT NULL,
    [ServiceType] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [ScheduledDate] datetime2 NOT NULL,
    [CompletedDate] datetime2 NULL,
    [Status] nvarchar(20) NOT NULL,
    [Mileage] int NULL,
    [Notes] nvarchar(1000) NOT NULL,
    [ServiceProvider] nvarchar(100) NOT NULL,
    [ServiceLocation] nvarchar(200) NOT NULL,
    [EstimatedCost] decimal(18,2) NULL,
    [ActualCost] decimal(18,2) NULL,
    [IsActive] bit NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [LastReminderSent] datetime2 NULL,
    [RemindersSent] int NOT NULL,
    CONSTRAINT [PK_ServiceReminders] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ServiceReminders_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Warranties] (
    [Id] int NOT NULL IDENTITY,
    [ContractId] int NOT NULL,
    [WarrantyType] nvarchar(100) NOT NULL,
    [DurationMonths] int NOT NULL,
    [DurationKilometers] int NOT NULL,
    [CoverageDetails] nvarchar(1000) NOT NULL,
    [Terms] nvarchar(2000) NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [LastUpdatedDate] datetime2 NULL,
    [Status] nvarchar(50) NOT NULL,
    [Notes] nvarchar(500) NULL,
    [ContractId1] int NULL,
    CONSTRAINT [PK_Warranties] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Warranties_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Warranties_Contracts_ContractId1] FOREIGN KEY ([ContractId1]) REFERENCES [Contracts] ([Id])
);
GO


IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConcurrencyStamp', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[AspNetRoles]'))
    SET IDENTITY_INSERT [AspNetRoles] ON;
INSERT INTO [AspNetRoles] ([Id], [ConcurrencyStamp], [Name], [NormalizedName])
VALUES (N'1', NULL, N'Administrator', N'ADMINISTRATOR'),
(N'2', NULL, N'Customer', N'CUSTOMER'),
(N'3', NULL, N'SalesRepresentative', N'SALESREPRESENTATIVE'),
(N'4', NULL, N'SupportStaff', N'SUPPORTSTAFF');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConcurrencyStamp', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[AspNetRoles]'))
    SET IDENTITY_INSERT [AspNetRoles] OFF;
GO


CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO


CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO


CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO


CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO


CREATE UNIQUE INDEX [IX_AspNetUsers_Email] ON [AspNetUsers] ([Email]) WHERE [Email] IS NOT NULL;
GO


CREATE INDEX [IX_AspNetUsers_IsActive] ON [AspNetUsers] ([IsActive]);
GO


CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO


CREATE UNIQUE INDEX [IX_Contracts_ContractNumber] ON [Contracts] ([ContractNumber]);
GO


CREATE INDEX [IX_Contracts_CreatedByUserId] ON [Contracts] ([CreatedByUserId]);
GO


CREATE INDEX [IX_Contracts_CreatedDate] ON [Contracts] ([CreatedDate]);
GO


CREATE INDEX [IX_Contracts_CustomerId] ON [Contracts] ([CustomerId]);
GO


CREATE INDEX [IX_Contracts_PurchaseId] ON [Contracts] ([PurchaseId]);
GO


CREATE INDEX [IX_Contracts_Status] ON [Contracts] ([Status]);
GO


CREATE INDEX [IX_Contracts_VehicleId] ON [Contracts] ([VehicleId]);
GO


CREATE INDEX [IX_CustomerFeedbacks_ContractId] ON [CustomerFeedbacks] ([ContractId]);
GO


CREATE INDEX [IX_CustomerFeedbacks_ContractId1] ON [CustomerFeedbacks] ([ContractId1]);
GO


CREATE INDEX [IX_CustomerFeedbacks_IsActive] ON [CustomerFeedbacks] ([IsActive]);
GO


CREATE INDEX [IX_CustomerFeedbacks_SubmittedDate] ON [CustomerFeedbacks] ([SubmittedDate]);
GO


CREATE INDEX [IX_Customers_CustomerStatus] ON [Customers] ([CustomerStatus]);
GO


CREATE INDEX [IX_Customers_IsActive] ON [Customers] ([IsActive]);
GO


CREATE UNIQUE INDEX [IX_Customers_UserId] ON [Customers] ([UserId]);
GO


CREATE INDEX [IX_Deliveries_ContractId] ON [Deliveries] ([ContractId]);
GO


CREATE INDEX [IX_Deliveries_DriverUserId] ON [Deliveries] ([DriverUserId]);
GO


CREATE INDEX [IX_Deliveries_ScheduledDate] ON [Deliveries] ([ScheduledDate]);
GO


CREATE INDEX [IX_Deliveries_Status] ON [Deliveries] ([Status]);
GO


CREATE INDEX [IX_Deliveries_TrackingNumber] ON [Deliveries] ([TrackingNumber]);
GO


CREATE INDEX [IX_Documents_CustomerId] ON [Documents] ([CustomerId]);
GO


CREATE INDEX [IX_Documents_DocumentTypeId] ON [Documents] ([DocumentTypeId]);
GO


CREATE INDEX [IX_Documents_IsActive] ON [Documents] ([IsActive]);
GO


CREATE INDEX [IX_Documents_PurchaseId] ON [Documents] ([PurchaseId]);
GO


CREATE INDEX [IX_Documents_ReviewedBy] ON [Documents] ([ReviewedBy]);
GO


CREATE INDEX [IX_Documents_Status] ON [Documents] ([Status]);
GO


CREATE INDEX [IX_Documents_UploadDate] ON [Documents] ([UploadDate]);
GO


CREATE INDEX [IX_DocumentTypes_IsActive] ON [DocumentTypes] ([IsActive]);
GO


CREATE INDEX [IX_DocumentTypes_IsRequired] ON [DocumentTypes] ([IsRequired]);
GO


CREATE UNIQUE INDEX [IX_DocumentTypes_Name] ON [DocumentTypes] ([Name]);
GO


CREATE INDEX [IX_Inquiries_AssignedToUserId] ON [Inquiries] ([AssignedToUserId]);
GO


CREATE INDEX [IX_Inquiries_CreatedDate] ON [Inquiries] ([CreatedDate]);
GO


CREATE INDEX [IX_Inquiries_CustomerId] ON [Inquiries] ([CustomerId]);
GO


CREATE INDEX [IX_Inquiries_Status] ON [Inquiries] ([Status]);
GO


CREATE INDEX [IX_Inquiries_VehicleId] ON [Inquiries] ([VehicleId]);
GO


CREATE INDEX [IX_Payments_ContractId] ON [Payments] ([ContractId]);
GO


CREATE INDEX [IX_Payments_PaymentType] ON [Payments] ([PaymentType]);
GO


CREATE INDEX [IX_Payments_ProcessedDate] ON [Payments] ([ProcessedDate]);
GO


CREATE INDEX [IX_Payments_Status] ON [Payments] ([Status]);
GO


CREATE INDEX [IX_Payments_TransactionId] ON [Payments] ([TransactionId]);
GO


CREATE INDEX [IX_Purchases_CustomerId] ON [Purchases] ([CustomerId]);
GO


CREATE INDEX [IX_Purchases_InitiatedDate] ON [Purchases] ([InitiatedDate]);
GO


CREATE INDEX [IX_Purchases_IsActive] ON [Purchases] ([IsActive]);
GO


CREATE INDEX [IX_Purchases_Status] ON [Purchases] ([Status]);
GO


CREATE INDEX [IX_Purchases_VehicleId] ON [Purchases] ([VehicleId]);
GO


CREATE INDEX [IX_PurchaseStatusHistories_ChangedDate] ON [PurchaseStatusHistories] ([ChangedDate]);
GO


CREATE INDEX [IX_PurchaseStatusHistories_PurchaseId] ON [PurchaseStatusHistories] ([PurchaseId]);
GO


CREATE INDEX [IX_PurchaseStatusHistories_PurchaseId_ChangedDate] ON [PurchaseStatusHistories] ([PurchaseId], [ChangedDate]);
GO


CREATE INDEX [IX_Reservations_CancelledByUserId] ON [Reservations] ([CancelledByUserId]);
GO


CREATE INDEX [IX_Reservations_ContractId] ON [Reservations] ([ContractId]);
GO


CREATE INDEX [IX_Reservations_CustomerId] ON [Reservations] ([CustomerId]);
GO


CREATE INDEX [IX_Reservations_ExpiryDate] ON [Reservations] ([ExpiryDate]);
GO


CREATE UNIQUE INDEX [IX_Reservations_ReservationNumber] ON [Reservations] ([ReservationNumber]);
GO


CREATE INDEX [IX_Reservations_Status] ON [Reservations] ([Status]);
GO


CREATE INDEX [IX_Reservations_VehicleId] ON [Reservations] ([VehicleId]);
GO


CREATE INDEX [IX_ServiceReminders_ContractId] ON [ServiceReminders] ([ContractId]);
GO


CREATE INDEX [IX_ServiceReminders_CreatedDate] ON [ServiceReminders] ([CreatedDate]);
GO


CREATE INDEX [IX_ServiceReminders_IsActive] ON [ServiceReminders] ([IsActive]);
GO


CREATE INDEX [IX_ServiceReminders_ScheduledDate] ON [ServiceReminders] ([ScheduledDate]);
GO


CREATE INDEX [IX_ServiceReminders_ServiceType] ON [ServiceReminders] ([ServiceType]);
GO


CREATE INDEX [IX_ServiceReminders_Status] ON [ServiceReminders] ([Status]);
GO


CREATE INDEX [IX_VehicleImages_DisplayOrder] ON [VehicleImages] ([DisplayOrder]);
GO


CREATE INDEX [IX_VehicleImages_ImageType] ON [VehicleImages] ([ImageType]);
GO


CREATE INDEX [IX_VehicleImages_VehicleId] ON [VehicleImages] ([VehicleId]);
GO


CREATE INDEX [IX_Vehicles_DateListed] ON [Vehicles] ([DateListed]);
GO


CREATE INDEX [IX_Vehicles_IsActive] ON [Vehicles] ([IsActive]);
GO


CREATE INDEX [IX_Vehicles_Make_Model_Year] ON [Vehicles] ([Make], [Model], [Year]);
GO


CREATE INDEX [IX_Vehicles_Status] ON [Vehicles] ([Status]);
GO


CREATE UNIQUE INDEX [IX_Vehicles_VIN] ON [Vehicles] ([VIN]);
GO


CREATE INDEX [IX_Warranties_ContractId] ON [Warranties] ([ContractId]);
GO


CREATE INDEX [IX_Warranties_ContractId1] ON [Warranties] ([ContractId1]);
GO


CREATE INDEX [IX_Warranties_EndDate] ON [Warranties] ([EndDate]);
GO


CREATE INDEX [IX_Warranties_IsActive] ON [Warranties] ([IsActive]);
GO


CREATE INDEX [IX_Warranties_StartDate] ON [Warranties] ([StartDate]);
GO


CREATE INDEX [IX_Warranties_Status] ON [Warranties] ([Status]);
GO


CREATE INDEX [IX_Warranties_WarrantyType] ON [Warranties] ([WarrantyType]);
GO


