using AutoEdge.Data;
using AutoEdge.Models.Entities;
using AutoEdge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoEdge.Controllers
{
    [Authorize]
    public class ServiceInvoiceController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IServiceNotificationService _notificationService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<ServiceInvoiceController> _logger;

        public ServiceInvoiceController(ApplicationDbContext db, IServiceNotificationService notificationService, 
            IPaymentService paymentService, ILogger<ServiceInvoiceController> logger)
        {
            _db = db;
            _notificationService = notificationService;
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Generate(int bookingId)
        {
            try
            {
                var booking = await _db.ServiceBookings
                    .Include(b => b.ServiceJob)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking == null)
                {
                    return NotFound();
                }

                // Check if user owns this booking
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (booking.CustomerId != userId)
                {
                    return Forbid();
                }

                var invoice = await _notificationService.GenerateServiceInvoiceAsync(bookingId);
                
                TempData["SuccessMessage"] = "Invoice generated successfully!";
                return RedirectToAction("CustomerView", new { id = invoice.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invoice for booking {BookingId}", bookingId);
                TempData["ErrorMessage"] = "An error occurred while generating the invoice.";
                return RedirectToAction("Index", "Customer");
            }
        }

        [HttpGet]
        public async Task<IActionResult> CustomerView(int id)
        {
            try
            {
                var invoice = await _db.ServiceInvoices
                    .Include(i => i.ServiceBooking)
                    .Include(i => i.ServicePayments)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (invoice == null)
                {
                    return NotFound();
                }

                // Check if user owns this invoice
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (invoice.ServiceBooking.CustomerId != userId)
                {
                    return Forbid();
                }

                return View(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading invoice {InvoiceId}", id);
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Pay(int id)
        {
            try
            {
                var invoice = await _db.ServiceInvoices
                    .Include(i => i.ServiceBooking)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (invoice == null)
                {
                    return NotFound();
                }

                // Check if user owns this invoice
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (invoice.ServiceBooking.CustomerId != userId)
                {
                    return Forbid();
                }

                // Check if already paid
                var totalPaid = await _db.ServicePayments
                    .Where(p => p.ServiceInvoiceId == id && p.Status == "Completed")
                    .SumAsync(p => p.Amount);

                if (totalPaid >= invoice.TotalAmount)
                {
                    TempData["InfoMessage"] = "This invoice has already been paid in full.";
                    return RedirectToAction("CustomerView", new { id });
                }

                ViewBag.RemainingAmount = invoice.TotalAmount - totalPaid;
                return View(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment page for invoice {InvoiceId}", id);
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int id, string paymentMethod, decimal amount, string? cardNumber = null, string? expiryDate = null, string? cvv = null)
        {
            try
            {
                _logger.LogInformation("Processing payment for invoice {InvoiceId}, amount: {Amount}, method: {Method}", id, amount, paymentMethod);
                
                var invoice = await _db.ServiceInvoices
                    .Include(i => i.ServiceBooking)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (invoice == null)
                {
                    return NotFound();
                }

                // Check if user owns this invoice
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (invoice.ServiceBooking.CustomerId != userId)
                {
                    return Forbid();
                }

                // Validate payment amount
                var totalPaid = await _db.ServicePayments
                    .Where(p => p.ServiceInvoiceId == id && p.Status == "Completed")
                    .SumAsync(p => p.Amount);

                var remainingAmount = invoice.TotalAmount - totalPaid;
                
                // If amount is 0 or not provided, default to full payment (remaining amount)
                if (amount <= 0)
                {
                    amount = remainingAmount;
                }
                
                // Ensure amount doesn't exceed remaining balance
                if (amount > remainingAmount)
                {
                    ModelState.AddModelError("Amount", "Payment amount cannot exceed the remaining balance.");
                    ViewBag.RemainingAmount = remainingAmount;
                    return View(invoice);
                }
                
                // Ensure amount matches remaining amount for full payment requirement
                if (amount != remainingAmount)
                {
                    ModelState.AddModelError("Amount", $"Full payment required: R {remainingAmount:F2}. Please pay the full remaining balance.");
                    ViewBag.RemainingAmount = remainingAmount;
                    return View(invoice);
                }

                // Process payment based on method
                PaymentResult result;
                switch (paymentMethod.ToLower())
                {
                    case "creditcard":
                    case "debitcard":
                        if (string.IsNullOrEmpty(cardNumber) || string.IsNullOrEmpty(expiryDate) || string.IsNullOrEmpty(cvv))
                        {
                            ModelState.AddModelError("", "Card details are required for card payments.");
                            return View(invoice);
                        }
                        result = await ProcessCardPaymentAsync(invoice, amount, cardNumber, expiryDate, cvv);
                        break;
                    case "cash":
                        result = await ProcessCashPaymentAsync(invoice, amount);
                        break;
                    case "eft":
                        result = await ProcessEFTPaymentAsync(invoice, amount);
                        break;
                    default:
                        ModelState.AddModelError("", "Invalid payment method selected.");
                        return View(invoice);
                }

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Payment successful! PaymentId: {PaymentId}, Amount: {Amount}", result.PaymentId, amount);
                    TempData["SuccessMessage"] = "Payment processed successfully!";
                    return RedirectToAction("PaymentSuccess", new { paymentId = result.PaymentId });
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                    return View(invoice);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for invoice {InvoiceId}", id);
                TempData["ErrorMessage"] = "An error occurred while processing the payment.";
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> PaymentSuccess(int paymentId)
        {
            try
            {
                var payment = await _db.ServicePayments
                    .Include(p => p.ServiceInvoice)
                    .ThenInclude(i => i.ServiceBooking)
                    .FirstOrDefaultAsync(p => p.Id == paymentId);

                if (payment == null)
                {
                    return NotFound();
                }

                // Check if user owns this payment
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (payment.ServiceInvoice.ServiceBooking.CustomerId != userId)
                {
                    return Forbid();
                }

                return View(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment success page for payment {PaymentId}", paymentId);
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadPDF(int id)
        {
            try
            {
                var invoice = await _db.ServiceInvoices
                    .Include(i => i.ServiceBooking)
                    .Include(i => i.ServicePayments)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (invoice == null)
                {
                    return NotFound();
                }

                // Check if user owns this invoice
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (invoice.ServiceBooking.CustomerId != userId)
                {
                    return Forbid();
                }

                // Generate PDF (implement PDF generation logic)
                var pdfBytes = await GenerateInvoicePDFAsync(invoice);
                
                return File(pdfBytes, "application/pdf", $"Invoice-{invoice.InvoiceNumber}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF for invoice {InvoiceId}", id);
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallback(string paymentIntentId)
        {
            try
            {
                // Handle Stripe webhook callback
                var payment = await _db.ServicePayments
                    .Include(p => p.ServiceInvoice)
                    .ThenInclude(i => i.ServiceBooking)
                    .FirstOrDefaultAsync(p => p.TransactionId == paymentIntentId);

                if (payment != null)
                {
                    payment.Status = "Completed";
                    payment.PaymentDate = DateTime.UtcNow;
                    await _db.SaveChangesAsync();

                    // Send confirmation notification
                    await _notificationService.SendPaymentConfirmationNotificationAsync(payment.Id);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment callback for {PaymentIntentId}", paymentIntentId);
                return StatusCode(500);
            }
        }

        private async Task<PaymentResult> ProcessCardPaymentAsync(ServiceInvoice invoice, decimal amount, string cardNumber, string expiryDate, string cvv)
        {
            try
            {
                _logger.LogInformation("Creating card payment for invoice {InvoiceId}, amount: {Amount}", invoice.Id, amount);
                
                var transactionId = $"SVC-{invoice.Id}-{DateTime.UtcNow:yyyyMMddHHmmss}";
                
                // Create service payment record
                var servicePayment = new ServicePayment
                {
                    ServiceInvoiceId = invoice.Id,
                    Amount = amount,
                    PaymentMethod = "CreditCard",
                    PaymentDate = DateTime.UtcNow,
                    Status = "Completed",
                    TransactionId = transactionId,
                    StripePaymentIntentId = $"pi_{transactionId}", // Simulated Stripe payment intent
                    Notes = $"Credit card payment processed for invoice {invoice.InvoiceNumber}",
                    CreatedAt = DateTime.UtcNow
                };

                _db.ServicePayments.Add(servicePayment);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Payment saved successfully! PaymentId: {PaymentId}, Amount: {Amount}", servicePayment.Id, servicePayment.Amount);

                // Send payment confirmation notification
                await _notificationService.SendPaymentConfirmationNotificationAsync(servicePayment.Id);

                return new PaymentResult
                {
                    IsSuccess = true,
                    Message = "Payment processed successfully!",
                    TransactionId = transactionId,
                    PaymentId = servicePayment.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing card payment for invoice {InvoiceId}", invoice.Id);
                return new PaymentResult
                {
                    IsSuccess = false,
                    Message = "An error occurred while processing the card payment."
                };
            }
        }

        private async Task<PaymentResult> ProcessCashPaymentAsync(ServiceInvoice invoice, decimal amount)
        {
            try
            {
                _logger.LogInformation("Creating cash payment for invoice {InvoiceId}, amount: {Amount}", invoice.Id, amount);
                
                var transactionId = $"CASH-{invoice.Id}-{DateTime.UtcNow:yyyyMMddHHmmss}";
                
                // Create service payment record for cash payment
                var servicePayment = new ServicePayment
                {
                    ServiceInvoiceId = invoice.Id,
                    Amount = amount,
                    PaymentMethod = "Cash",
                    PaymentDate = DateTime.UtcNow,
                    Status = "Pending", // Cash payments are pending until confirmed
                    TransactionId = transactionId,
                    Notes = $"Cash payment recorded for invoice {invoice.InvoiceNumber}",
                    CreatedAt = DateTime.UtcNow
                };

                _db.ServicePayments.Add(servicePayment);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Cash payment saved successfully! PaymentId: {PaymentId}, Amount: {Amount}", servicePayment.Id, servicePayment.Amount);

                return new PaymentResult
                {
                    IsSuccess = true,
                    Message = "Cash payment recorded. Please pay when collecting your vehicle.",
                    TransactionId = transactionId,
                    PaymentId = servicePayment.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing cash payment for invoice {InvoiceId}", invoice.Id);
                return new PaymentResult
                {
                    IsSuccess = false,
                    Message = "An error occurred while recording the cash payment."
                };
            }
        }

        private async Task<PaymentResult> ProcessEFTPaymentAsync(ServiceInvoice invoice, decimal amount)
        {
            try
            {
                _logger.LogInformation("Creating EFT payment for invoice {InvoiceId}, amount: {Amount}", invoice.Id, amount);
                
                var transactionId = $"EFT-{invoice.Id}-{DateTime.UtcNow:yyyyMMddHHmmss}";
                
                // Create service payment record for EFT payment
                var servicePayment = new ServicePayment
                {
                    ServiceInvoiceId = invoice.Id,
                    Amount = amount,
                    PaymentMethod = "EFT",
                    PaymentDate = DateTime.UtcNow,
                    Status = "Pending", // EFT payments are pending until bank confirmation
                    TransactionId = transactionId,
                    Notes = $"EFT payment recorded for invoice {invoice.InvoiceNumber}",
                    CreatedAt = DateTime.UtcNow
                };

                _db.ServicePayments.Add(servicePayment);
                await _db.SaveChangesAsync();

                _logger.LogInformation("EFT payment saved successfully! PaymentId: {PaymentId}, Amount: {Amount}", servicePayment.Id, servicePayment.Amount);

                return new PaymentResult
                {
                    IsSuccess = true,
                    Message = "EFT payment recorded. Please complete the transfer and provide proof of payment.",
                    TransactionId = transactionId,
                    PaymentId = servicePayment.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing EFT payment for invoice {InvoiceId}", invoice.Id);
                return new PaymentResult
                {
                    IsSuccess = false,
                    Message = "An error occurred while recording the EFT payment."
                };
            }
        }

        private async Task<byte[]> GenerateInvoicePDFAsync(ServiceInvoice invoice)
        {
            // Implement PDF generation using a library like iTextSharp or similar
            // This is a placeholder - implement actual PDF generation
            var html = $@"
                <html>
                <head><title>Invoice {invoice.InvoiceNumber}</title></head>
                <body>
                    <h1>Service Invoice</h1>
                    <p>Invoice Number: {invoice.InvoiceNumber}</p>
                    <p>Issue Date: {invoice.IssueDate:yyyy-MM-dd}</p>
                    <p>Total Amount: ${invoice.TotalAmount:F2}</p>
                </body>
                </html>
            ";

            // Convert HTML to PDF (implement actual PDF generation)
            return System.Text.Encoding.UTF8.GetBytes(html);
        }
    }
}
