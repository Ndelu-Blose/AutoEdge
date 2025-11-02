using AutoEdge.Data;
using AutoEdge.Models.Entities;
using AutoEdge.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using System.Text.Json;

namespace AutoEdge.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentService> _logger;
        private readonly PaymentIntentService _paymentIntentService;
        private readonly PaymentMethodService _paymentMethodService;
        private readonly IEmailService _emailService;

        public PaymentService(
            IUnitOfWork unitOfWork,
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<PaymentService> logger,
            IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _emailService = emailService;
            _paymentIntentService = new PaymentIntentService();
            _paymentMethodService = new PaymentMethodService();
        }

        public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Processing payment for Purchase {PurchaseId}, Method: {PaymentMethod}, Amount: {Amount}", 
                    request.PurchaseId, request.PaymentMethod, request.Amount);
                
                return request.PaymentMethod.ToLower() switch
                {
                    "creditcard" or "debitcard" => await ProcessCreditCardPaymentAsync((CreditCardPaymentRequest)request),
                    "cash" or "banktransfer" or "check" => await ProcessCashPaymentAsync((CashPaymentRequest)request),
                    "financing" => await ProcessFinancingPaymentAsync((FinancingPaymentRequest)request),
                    _ => new PaymentResult
                    {
                        IsSuccess = false,
                        Message = "Unsupported payment method",
                        ErrorCode = "UNSUPPORTED_PAYMENT_METHOD"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for purchase {PurchaseId}", request.PurchaseId);
                return new PaymentResult
                {
                    IsSuccess = false,
                    Message = "An error occurred while processing the payment",
                    ErrorCode = "PAYMENT_PROCESSING_ERROR",
                    FailureReason = ex.Message
                };
            }
        }

        public async Task<PaymentResult> ProcessCreditCardPaymentAsync(CreditCardPaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Processing credit card payment for Purchase {PurchaseId}, Amount: {Amount}", 
                    request.PurchaseId, request.Amount);
                
                // Use test payment method for development
                string paymentMethodId = GetTestPaymentMethodId(request.CardNumber);
                
                PaymentMethod paymentMethod;
                if (paymentMethodId != null)
                {
                    // Use predefined test payment method
                    var paymentMethodService = new PaymentMethodService();
                    paymentMethod = await paymentMethodService.GetAsync(paymentMethodId);
                }
                else
                {
                    // Create payment method from card details (for non-test cards)
                    var paymentMethodOptions = new PaymentMethodCreateOptions
                    {
                        Type = "card",
                        Card = new PaymentMethodCardOptions
                        {
                            Number = request.CardNumber,
                            ExpMonth = (long?)Convert.ToInt64(request.ExpiryMonth),
                            ExpYear = (long?)Convert.ToInt64(request.ExpiryYear),
                            Cvc = request.Cvv
                        },
                        BillingDetails = new PaymentMethodBillingDetailsOptions
                        {
                            Name = request.CardholderName,
                            Address = new AddressOptions
                            {
                                Line1 = request.BillingAddress,
                                City = request.BillingCity,
                                State = request.BillingState,
                                PostalCode = request.BillingZip
                            }
                        }
                    };

                    var paymentMethodService = new PaymentMethodService();
                    paymentMethod = await paymentMethodService.CreateAsync(paymentMethodOptions);
                }
                
                // Create payment intent with Stripe
                var paymentIntentOptions = new PaymentIntentCreateOptions
                {
                    Amount = (long)(request.Amount * 100), // Convert to cents
                    Currency = request.Currency.ToLower(),
                    PaymentMethodTypes = new List<string> { "card" },
                    Confirm = true,  // <-- auto-confirm the intent
                    PaymentMethod = paymentMethod.Id, // use the created payment method
                    Metadata = new Dictionary<string, string>
                    {
                        { "purchase_id", request.PurchaseId.ToString() },
                        { "payment_type", request.PaymentType }
                    }
                };

                var paymentIntent = await _paymentIntentService.CreateAsync(paymentIntentOptions);

                // Map Stripe status to application status
                var status = paymentIntent.Status switch
                {
                    "succeeded" => "Completed",
                    "processing" => "Processing",
                    "requires_action" => "RequiresAction",
                    "requires_payment_method" => "Failed",
                    _ => "Pending"
                };

                // Get the contract associated with this purchase
                var purchase = await _context.Purchases
                    .Include(p => p.Contracts)
                    .FirstOrDefaultAsync(p => p.Id == request.PurchaseId);
                
                if (purchase == null)
                {
                    throw new InvalidOperationException($"Purchase with ID {request.PurchaseId} not found");
                }
                
                var contract = purchase.Contracts.FirstOrDefault();
                if (contract == null)
                {
                    throw new InvalidOperationException($"No contract found for purchase {request.PurchaseId}. Contract must be generated before payment.");
                }

                // Create payment record
                var payment = new Payment
                {
                    ContractId = contract.Id, // Use the actual ContractId
                    Amount = request.Amount,
                    PaymentMethod = request.PaymentMethod,
                    PaymentType = request.PaymentType,
                    Status = status,
                    StripePaymentIntentId = paymentIntent.Id,
                    TransactionId = paymentIntent.Id,
                    Currency = request.Currency,
                    Notes = request.Notes,
                    ProcessedDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                };

                await _unitOfWork.Payments.AddAsync(payment);
                
                // Update purchase status if payment is completed
                if (status == "Completed")
                {
                    var purchaseToUpdate = await _unitOfWork.Purchases.GetByIdAsync(request.PurchaseId);
                    if (purchaseToUpdate != null)
                    {
                        purchaseToUpdate.Status = "Completed";
                        purchaseToUpdate.PaymentCompleted = true;
                        purchaseToUpdate.PaymentCompletedDate = DateTime.UtcNow;
                        purchaseToUpdate.ModifiedDate = DateTime.UtcNow;
                        _unitOfWork.Purchases.Update(purchaseToUpdate);
                    }
                }
                
                await _unitOfWork.SaveChangesAsync();

                // Send payment confirmation email if payment is completed
                if (status == "Completed")
                {
                    try
                    {
                        await SendPaymentProofEmailAsync(payment.Id);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogError(emailEx, "Failed to send payment confirmation email for payment {PaymentId}", payment.Id);
                        // Don't fail the payment process if email fails
                    }
                }

                return new PaymentResult
                {
                    IsSuccess = true,
                    Message = "Payment intent created successfully",
                    TransactionId = paymentIntent.Id,
                    PaymentIntentId = paymentIntent.Id,
                    Payment = payment,
                    RequiresAction = paymentIntent.Status == "requires_action",
                    ClientSecret = paymentIntent.ClientSecret
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error processing credit card payment for purchase {PurchaseId}. Error Code: {ErrorCode}, Message: {ErrorMessage}", 
                    request.PurchaseId, ex.StripeError?.Code, ex.StripeError?.Message);
                return new PaymentResult
                {
                    IsSuccess = false,
                    Message = "Credit card payment failed",
                    ErrorCode = ex.StripeError?.Code ?? "STRIPE_ERROR",
                    FailureReason = ex.StripeError?.Message ?? ex.Message
                };
            }
        }

        public async Task<PaymentResult> ProcessCashPaymentAsync(CashPaymentRequest request)
        {
            try
            {
                // Get the contract associated with this purchase
                var purchase = await _context.Purchases
                    .Include(p => p.Contracts)
                    .FirstOrDefaultAsync(p => p.Id == request.PurchaseId);
                
                if (purchase == null)
                {
                    throw new InvalidOperationException($"Purchase with ID {request.PurchaseId} not found");
                }
                
                var contract = purchase.Contracts.FirstOrDefault();
                if (contract == null)
                {
                    throw new InvalidOperationException($"No contract found for purchase {request.PurchaseId}. Contract must be generated before payment.");
                }

                // Generate reference number for cash payment
                var referenceNumber = $"CASH-{request.PurchaseId}-{DateTime.UtcNow:yyyyMMddHHmmss}";

                var payment = new Payment
                {
                    ContractId = contract.Id,
                    Amount = request.Amount,
                    PaymentMethod = request.PaymentMethod,
                    PaymentType = request.PaymentType,
                    Status = "Pending",
                    TransactionId = referenceNumber,
                    Currency = request.Currency,
                    Notes = $"{request.Notes}\nPayment Instructions: {request.PaymentInstructions}",
                    ProcessedDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow,
                    DueDate = request.PaymentDeadline
                };

                await _unitOfWork.Payments.AddAsync(payment);
                await _unitOfWork.SaveChangesAsync();

                return new PaymentResult
                {
                    IsSuccess = true,
                    Message = "Cash payment instructions generated",
                    TransactionId = referenceNumber,
                    Payment = payment
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing cash payment for purchase {PurchaseId}", request.PurchaseId);
                return new PaymentResult
                {
                    IsSuccess = false,
                    Message = "Failed to process cash payment",
                    ErrorCode = "CASH_PAYMENT_ERROR",
                    FailureReason = ex.Message
                };
            }
        }

        public async Task<PaymentResult> ProcessFinancingPaymentAsync(FinancingPaymentRequest request)
        {
            try
            {
                // Get the contract associated with this purchase
                var purchase = await _context.Purchases
                    .Include(p => p.Contracts)
                    .FirstOrDefaultAsync(p => p.Id == request.PurchaseId);
                
                if (purchase == null)
                {
                    throw new InvalidOperationException($"Purchase with ID {request.PurchaseId} not found");
                }
                
                var contract = purchase.Contracts.FirstOrDefault();
                if (contract == null)
                {
                    throw new InvalidOperationException($"No contract found for purchase {request.PurchaseId}. Contract must be generated before payment.");
                }

                var referenceNumber = $"FINANCE-{request.PurchaseId}-{DateTime.UtcNow:yyyyMMddHHmmss}";

                var payment = new Payment
                {
                    ContractId = contract.Id,
                    Amount = request.DownPayment, // Initial down payment
                    PaymentMethod = request.PaymentMethod,
                    PaymentType = "DownPayment",
                    Status = "Pending",
                    TransactionId = referenceNumber,
                    Currency = request.Currency,
                    Notes = $"{request.Notes}\nFinancing Provider: {request.FinancingProvider}\nLoan Term: {request.LoanTermMonths} months\nInterest Rate: {request.InterestRate}%\nMonthly Payment: ${request.MonthlyPayment}",
                    ProcessedDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow,
                    IsRecurring = true
                };

                await _unitOfWork.Payments.AddAsync(payment);
                await _unitOfWork.SaveChangesAsync();

                return new PaymentResult
                {
                    IsSuccess = true,
                    Message = "Financing payment setup completed",
                    TransactionId = referenceNumber,
                    Payment = payment
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing financing payment for purchase {PurchaseId}", request.PurchaseId);
                return new PaymentResult
                {
                    IsSuccess = false,
                    Message = "Failed to process financing payment",
                    ErrorCode = "FINANCING_PAYMENT_ERROR",
                    FailureReason = ex.Message
                };
            }
        }

        public async Task<PaymentResult> RefundPaymentAsync(int paymentId, decimal refundAmount, string reason)
        {
            try
            {
                var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
                if (payment == null)
                {
                    return new PaymentResult
                    {
                        IsSuccess = false,
                        Message = "Payment not found",
                        ErrorCode = "PAYMENT_NOT_FOUND"
                    };
                }

                if (!string.IsNullOrEmpty(payment.StripePaymentIntentId))
                {
                    // Process Stripe refund
                    var refundService = new RefundService();
                    var refundOptions = new RefundCreateOptions
                    {
                        PaymentIntent = payment.StripePaymentIntentId,
                        Amount = (long)(refundAmount * 100),
                        Reason = "requested_by_customer",
                        Metadata = new Dictionary<string, string>
                        {
                            { "refund_reason", reason }
                        }
                    };

                    var refund = await refundService.CreateAsync(refundOptions);
                    payment.RefundTransactionId = refund.Id;
                }

                payment.RefundAmount = refundAmount;
                payment.RefundDate = DateTime.UtcNow;
                payment.RefundReason = reason;
                payment.Status = "Refunded";
                payment.ModifiedDate = DateTime.UtcNow;

                _unitOfWork.Payments.Update(payment);
                await _unitOfWork.SaveChangesAsync();

                return new PaymentResult
                {
                    IsSuccess = true,
                    Message = "Refund processed successfully",
                    Payment = payment
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for payment {PaymentId}", paymentId);
                return new PaymentResult
                {
                    IsSuccess = false,
                    Message = "Failed to process refund",
                    ErrorCode = "REFUND_ERROR",
                    FailureReason = ex.Message
                };
            }
        }

        public async Task<Payment?> GetPaymentByIdAsync(int paymentId)
        {
            return await _unitOfWork.Payments.GetByIdAsync(paymentId);
        }

        public async Task<List<Payment>> GetPaymentsByPurchaseIdAsync(int purchaseId)
        {
            var payments = await _unitOfWork.Payments.GetAllAsync();
            return payments.Where(p => p.ContractId == purchaseId).ToList();
        }

        public async Task<bool> ValidatePaymentAsync(int paymentId)
        {
            try
            {
                var payment = await GetPaymentByIdAsync(paymentId);
                if (payment == null) return false;

                if (!string.IsNullOrEmpty(payment.StripePaymentIntentId))
                {
                    var paymentIntent = await _paymentIntentService.GetAsync(payment.StripePaymentIntentId);
                    return paymentIntent.Status == "succeeded";
                }

                return payment.Status == "Completed";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating payment {PaymentId}", paymentId);
                return false;
            }
        }

        public async Task<PaymentResult> CheckPaymentStatusAsync(string paymentIntentId)
        {
            try
            {
                var paymentIntent = await _paymentIntentService.GetAsync(paymentIntentId);
                
                // Find the payment record in our database
                var payments = await _unitOfWork.Payments.GetAllAsync();
                var payment = payments.FirstOrDefault(p => p.StripePaymentIntentId == paymentIntentId);
                
                if (payment != null)
                {
                    // Map Stripe status to application status
                    var newStatus = paymentIntent.Status switch
                    {
                        "succeeded" => "Completed",
                        "processing" => "Processing",
                        "requires_action" => "RequiresAction",
                        "requires_payment_method" => "Failed",
                        _ => "Pending"
                    };
                    
                    // Update payment status if it has changed
                    if (payment.Status != newStatus)
                    {
                        payment.Status = newStatus;
                        payment.ModifiedDate = DateTime.UtcNow;
                        
                        // If payment is completed, update the purchase status
                        if (newStatus == "Completed")
                        {
                            var purchase = await _unitOfWork.Purchases.GetByIdAsync(payment.ContractId);
                            if (purchase != null)
                            {
                                purchase.Status = "Completed";
                                purchase.PaymentCompleted = true;
                                purchase.PaymentCompletedDate = DateTime.UtcNow;
                                purchase.ModifiedDate = DateTime.UtcNow;
                                _unitOfWork.Purchases.Update(purchase);
                                
                                // Create status history entry
                                var statusHistory = new PurchaseStatusHistory
                                {
                                    PurchaseId = purchase.Id,
                                    FromStatus = payment.Status,
                                    ToStatus = "Completed",
                                    Status = "Completed",
                                    ChangedDate = DateTime.UtcNow,
                                    Notes = "Payment completed successfully",
                                    ChangedBy = "System",
                                    ChangeReason = "Payment processed",
                                    IsSystemGenerated = true
                                };
                                
                                // Add status history through DbContext since it's not in UnitOfWork yet
                                _context.PurchaseStatusHistories.Add(statusHistory);
                            }
                        }
                        
                        _unitOfWork.Payments.Update(payment);
                        await _unitOfWork.SaveChangesAsync();

                        // Send payment confirmation email if payment just completed
                        if (newStatus == "Completed" && payment.Status != "Completed")
                        {
                            try
                            {
                                await SendPaymentProofEmailAsync(payment.Id);
                            }
                            catch (Exception emailEx)
                            {
                                _logger.LogError(emailEx, "Failed to send payment confirmation email for payment {PaymentId}", payment.Id);
                                // Don't fail the payment status check if email fails
                            }
                        }
                    }
                }
                
                return new PaymentResult
                {
                    IsSuccess = paymentIntent.Status == "succeeded",
                    PaymentIntentId = paymentIntent.Id,
                    TransactionId = paymentIntent.Id,
                    RequiresAction = paymentIntent.Status == "requires_action",
                    ClientSecret = paymentIntent.ClientSecret,
                    Message = $"Payment status: {paymentIntent.Status}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking payment status for PaymentIntent {PaymentIntentId}", paymentIntentId);
                return new PaymentResult
                {
                    IsSuccess = false,
                    Message = "Failed to check payment status",
                    ErrorCode = "STATUS_CHECK_ERROR",
                    FailureReason = ex.Message
                };
            }
        }

        public async Task<bool> SendPaymentProofEmailAsync(int paymentId)
        {
            try
            {
                // Get payment with related data
                var payment = await _context.Payments
                    .Include(p => p.Contract)
                        .ThenInclude(c => c.Purchase)
                            .ThenInclude(p => p.Customer)
                                .ThenInclude(c => c.User)
                    .Include(p => p.Contract)
                        .ThenInclude(c => c.Purchase)
                            .ThenInclude(p => p.Vehicle)
                    .FirstOrDefaultAsync(p => p.Id == paymentId);

                if (payment == null)
                {
                    _logger.LogError("Payment with ID {PaymentId} not found", paymentId);
                    return false;
                }

                if (payment.Contract?.Purchase?.Customer == null)
                {
                    _logger.LogError("Customer information not found for payment {PaymentId}", paymentId);
                    return false;
                }

                var customer = payment.Contract.Purchase.Customer;
                var vehicle = payment.Contract.Purchase.Vehicle;
                
                // Check if User navigation property is loaded
                if (customer.User == null)
                {
                    _logger.LogError("User information not found for customer {CustomerId} in payment {PaymentId}", customer.Id, paymentId);
                    return false;
                }
                
                // Build vehicle info string
                var vehicleInfo = vehicle != null 
                    ? $"{vehicle.Year} {vehicle.Make} {vehicle.Model}"
                    : "Vehicle information not available";

                // Send payment confirmation email
                await _emailService.SendPaymentConfirmationEmailAsync(
                    customer.User.Email,
                    $"{customer.User.FirstName} {customer.User.LastName}",
                    vehicleInfo,
                    payment.Amount,
                    payment.PaymentMethod,
                    payment.TransactionId
                );

                _logger.LogInformation("Payment proof email sent successfully for payment {PaymentId} to {CustomerEmail}", 
                    paymentId, customer.User.Email);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment proof email for payment {PaymentId}", paymentId);
                return false;
            }
        }

        private string GetTestPaymentMethodId(string cardNumber)
        {
            // Map common test card numbers to test tokens
            // Using Stripe's test card tokens instead of raw card data
            return cardNumber switch
            {
                "4242424242424242" => "pm_card_visa", // Visa test card
                "4000056655665556" => "pm_card_visa_debit", // Visa debit
                "5555555555554444" => "pm_card_mastercard", // Mastercard
                "2223003122003222" => "pm_card_mastercard", // Mastercard (2-series)
                "5200828282828210" => "pm_card_mastercard_debit", // Mastercard debit
                "5105105105105100" => "pm_card_mastercard_prepaid", // Mastercard prepaid
                "378282246310005" => "pm_card_amex", // American Express
                "371449635398431" => "pm_card_amex", // American Express
                "6011111111111117" => "pm_card_discover", // Discover
                "6011000990139424" => "pm_card_discover", // Discover
                "30569309025904" => "pm_card_diners", // Diners Club
                "38520000023237" => "pm_card_diners", // Diners Club
                "3566002020360505" => "pm_card_jcb", // JCB
                "6200000000000005" => "pm_card_unionpay", // UnionPay
                _ => null // Return null for non-test cards
            };
        }
    }
}