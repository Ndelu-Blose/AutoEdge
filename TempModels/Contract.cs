using System;
using System.Collections.Generic;

namespace AutoEdge.TempModels;

public partial class Contract
{
    public int Id { get; set; }

    public int VehicleId { get; set; }

    public int CustomerId { get; set; }

    public string ContractNumber { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public decimal DownPayment { get; set; }

    public decimal FinancedAmount { get; set; }

    public string PaymentType { get; set; } = null!;

    public string LoanDetails { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? SignedDate { get; set; }

    public string ContractPath { get; set; } = null!;

    public string SignedContractPath { get; set; } = null!;

    public string CreatedByUserId { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public bool IsActive { get; set; }

    public decimal? InterestRate { get; set; }

    public int? LoanTermMonths { get; set; }

    public decimal? MonthlyPayment { get; set; }

    public string LenderName { get; set; } = null!;

    public string LoanNumber { get; set; } = null!;

    public decimal? TradeInValue { get; set; }

    public string TradeInVehicle { get; set; } = null!;

    public decimal TaxAmount { get; set; }

    public decimal RegistrationFee { get; set; }

    public decimal DocumentationFee { get; set; }

    public decimal ExtendedWarrantyFee { get; set; }

    public decimal OtherFees { get; set; }

    public string SpecialTerms { get; set; } = null!;

    public string Notes { get; set; } = null!;

    public int? PurchaseId { get; set; }

    public DateTime? DeliveryDate { get; set; }

    public string DeliveryAddress { get; set; } = null!;

    public string DeliveryMethod { get; set; } = null!;

    public bool IsDigitallySigned { get; set; }

    public string DigitalSignatureData { get; set; } = null!;

    public string? OpenSignDocumentId { get; set; }

    public string? SigningRequestId { get; set; }

    public string? SigningUrl { get; set; }

    public string? SignedDocumentUrl { get; set; }

    public string? CertificateUrl { get; set; }

    public DateTime? SigningRequestSentDate { get; set; }

    public DateTime? SigningCompletedDate { get; set; }

    public string? SigningStatus { get; set; }

    public DateTime? CompletionDate { get; set; }

    public virtual AspNetUser CreatedByUser { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Purchase? Purchase { get; set; }

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    public virtual Vehicle Vehicle { get; set; } = null!;
}
