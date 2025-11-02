using System;
using System.Collections.Generic;

namespace AutoEdge.TempModels;

public partial class Reservation
{
    public int Id { get; set; }

    public int VehicleId { get; set; }

    public int CustomerId { get; set; }

    public string ReservationNumber { get; set; } = null!;

    public DateTime ReservationDate { get; set; }

    public DateTime ExpiryDate { get; set; }

    public decimal DepositAmount { get; set; }

    public string Status { get; set; } = null!;

    public string Notes { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public bool IsActive { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string PaymentTransactionId { get; set; } = null!;

    public DateTime? PaymentDate { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public decimal? RefundAmount { get; set; }

    public DateTime? RefundDate { get; set; }

    public string RefundReason { get; set; } = null!;

    public DateTime? ConvertedToContractDate { get; set; }

    public int? ContractId { get; set; }

    public string CancellationReason { get; set; } = null!;

    public DateTime? CancellationDate { get; set; }

    public string? CancelledByUserId { get; set; }

    public virtual AspNetUser? CancelledByUser { get; set; }

    public virtual Contract? Contract { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Vehicle Vehicle { get; set; } = null!;
}
