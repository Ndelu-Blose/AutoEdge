using AutoEdge.Models.Entities;

namespace AutoEdge.Services
{
    public interface IPaymentService
    {
        Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
        Task<PaymentResult> ProcessCreditCardPaymentAsync(CreditCardPaymentRequest request);
        Task<PaymentResult> ProcessCashPaymentAsync(CashPaymentRequest request);
        Task<PaymentResult> ProcessFinancingPaymentAsync(FinancingPaymentRequest request);
        Task<PaymentResult> RefundPaymentAsync(int paymentId, decimal refundAmount, string reason);
        Task<Payment?> GetPaymentByIdAsync(int paymentId);
        Task<List<Payment>> GetPaymentsByPurchaseIdAsync(int purchaseId);
        Task<bool> ValidatePaymentAsync(int paymentId);
        Task<PaymentResult> CheckPaymentStatusAsync(string paymentIntentId);
        Task<bool> SendPaymentProofEmailAsync(int paymentId);
    }

    public class PaymentRequest
    {
        public int PurchaseId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentType { get; set; } = "FullPayment"; // DownPayment, MonthlyPayment, FullPayment, Deposit
        public string Currency { get; set; } = "ZAR";
        public string Notes { get; set; } = string.Empty;
    }

    public class CreditCardPaymentRequest : PaymentRequest
    {
        public string CardNumber { get; set; } = string.Empty;
        public string ExpiryMonth { get; set; } = string.Empty;
        public string ExpiryYear { get; set; } = string.Empty;
        public string Cvv { get; set; } = string.Empty;
        public string CardholderName { get; set; } = string.Empty;
        public string BillingAddress { get; set; } = string.Empty;
        public string BillingCity { get; set; } = string.Empty;
        public string BillingState { get; set; } = string.Empty;
        public string BillingZip { get; set; } = string.Empty;
    }

    public class CashPaymentRequest : PaymentRequest
    {
        public string PaymentInstructions { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public DateTime PaymentDeadline { get; set; }
    }

    public class FinancingPaymentRequest : PaymentRequest
    {
        public string FinancingProvider { get; set; } = string.Empty;
        public decimal InterestRate { get; set; }
        public int LoanTermMonths { get; set; }
        public decimal MonthlyPayment { get; set; }
        public decimal DownPayment { get; set; }
        public string LoanApplicationId { get; set; } = string.Empty;
    }

    public class PaymentResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string PaymentIntentId { get; set; } = string.Empty;
        public Payment? Payment { get; set; }
        public int? PaymentId { get; set; } // Added for service payments
        public string ErrorCode { get; set; } = string.Empty;
        public string FailureReason { get; set; } = string.Empty;
        public bool RequiresAction { get; set; }
        public string ClientSecret { get; set; } = string.Empty;
    }
}