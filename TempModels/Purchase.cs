using System;
using System.Collections.Generic;

namespace AutoEdge.TempModels;

public partial class Purchase
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int VehicleId { get; set; }

    public string Status { get; set; } = null!;

    public decimal PurchasePrice { get; set; }

    public decimal DepositAmount { get; set; }

    public decimal RemainingAmount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string FinancingProvider { get; set; } = null!;

    public decimal? InterestRate { get; set; }

    public int? LoanTermMonths { get; set; }

    public decimal? MonthlyPayment { get; set; }

    public DateTime InitiatedDate { get; set; }

    public DateTime? DocumentsSubmittedDate { get; set; }

    public DateTime? DocumentsVerifiedDate { get; set; }

    public DateTime? ContractGeneratedDate { get; set; }

    public DateTime? ContractSignedDate { get; set; }

    public DateTime? PaymentCompletedDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public DateTime? CancelledDate { get; set; }

    public string Notes { get; set; } = null!;

    public string CancellationReason { get; set; } = null!;

    public string AssignedSalesRep { get; set; } = null!;

    public bool RequiresFinancing { get; set; }

    public bool DocumentsVerified { get; set; }

    public bool ContractSigned { get; set; }

    public bool PaymentCompleted { get; set; }

    public string DeliveryMethod { get; set; } = null!;

    public string DeliveryAddress { get; set; } = null!;

    public DateTime? ScheduledDeliveryDate { get; set; }

    public DateTime? ActualDeliveryDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual ICollection<PurchaseStatusHistory> PurchaseStatusHistories { get; set; } = new List<PurchaseStatusHistory>();

    public virtual Vehicle Vehicle { get; set; } = null!;
}
