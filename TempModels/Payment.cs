using System;
using System.Collections.Generic;

namespace AutoEdge.TempModels;

public partial class Payment
{
    public int Id { get; set; }

    public int ContractId { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string TransactionId { get; set; } = null!;

    public string StripePaymentIntentId { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime ProcessedDate { get; set; }

    public string GatewayResponse { get; set; } = null!;

    public decimal RefundAmount { get; set; }

    public DateTime? RefundDate { get; set; }

    public string RefundReason { get; set; } = null!;

    public string RefundTransactionId { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public bool IsActive { get; set; }

    public string PaymentType { get; set; } = null!;

    public string CardLastFour { get; set; } = null!;

    public string CardBrand { get; set; } = null!;

    public string BankName { get; set; } = null!;

    public string CheckNumber { get; set; } = null!;

    public decimal ProcessingFee { get; set; }

    public decimal NetAmount { get; set; }

    public string Currency { get; set; } = null!;

    public string Notes { get; set; } = null!;

    public string FailureReason { get; set; } = null!;

    public int RetryCount { get; set; }

    public DateTime? NextRetryDate { get; set; }

    public bool IsRecurring { get; set; }

    public string RecurringScheduleId { get; set; } = null!;

    public DateTime? DueDate { get; set; }

    public bool IsOverdue { get; set; }

    public decimal LateFee { get; set; }

    public string ReceiptPath { get; set; } = null!;

    public bool ReceiptSent { get; set; }

    public DateTime? ReceiptSentDate { get; set; }

    public virtual Contract Contract { get; set; } = null!;
}
