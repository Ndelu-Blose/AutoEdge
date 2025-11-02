using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoEdge.Data;
using AutoEdge.Models.Entities;
using AutoEdge.Models;
using AutoEdge.Services;
using Newtonsoft.Json;
using System.Text;
using Stripe;

namespace AutoEdge.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WebhookController> _logger;
        private readonly IESignatureService _eSignatureService;
        private readonly IConfiguration _configuration;
        private readonly IPaymentService _paymentService;

        public WebhookController(
            ApplicationDbContext context,
            ILogger<WebhookController> logger,
            IESignatureService eSignatureService,
            IConfiguration configuration,
            IPaymentService paymentService)
        {
            _context = context;
            _logger = logger;
            _eSignatureService = eSignatureService;
            _configuration = configuration;
            _paymentService = paymentService;
        }

        [HttpPost("opensign")]
        public async Task<IActionResult> OpenSignWebhook([FromBody] object payload)
        {
            try
            {
                // Log the incoming webhook payload
                var payloadJson = JsonConvert.SerializeObject(payload);
                _logger.LogInformation("Received OpenSign webhook: {Payload}", payloadJson);

                // Parse the webhook payload
                dynamic webhookData = JsonConvert.DeserializeObject(payloadJson) ?? new { };
                
                // Extract relevant information from the webhook
                string? eventType = webhookData.eventType?.ToString();
                string? documentId = webhookData.documentId?.ToString();
                string? status = webhookData.status?.ToString();
                string? signerEmail = webhookData.signerEmail?.ToString();
                DateTime? signedAt = null;
                
                if (webhookData.signedAt != null)
                {
                    if (DateTime.TryParse(webhookData.signedAt.ToString(), out DateTime parsedDate))
                    {
                        signedAt = parsedDate;
                    }
                }

                // Validate required fields
                if (string.IsNullOrEmpty(documentId))
                {
                    _logger.LogWarning("OpenSign webhook missing documentId");
                    return BadRequest("Missing documentId");
                }

                // Find the contract by OpenSign document ID
                var contract = await _context.Contracts
                    .Include(c => c.Purchase)
                    .FirstOrDefaultAsync(c => c.OpenSignDocumentId == documentId);

                if (contract == null)
                {
                    _logger.LogWarning("Contract not found for OpenSign document ID: {DocumentId}", documentId);
                    return NotFound($"Contract not found for document ID: {documentId}");
                }

                // Process different event types
                switch (eventType?.ToLower())
                {
                    case "document_signed":
                    case "signing_completed":
                        await HandleDocumentSigned(contract, status, signerEmail, signedAt, webhookData);
                        break;
                        
                    case "document_declined":
                    case "signing_declined":
                        await HandleDocumentDeclined(contract, signerEmail, webhookData);
                        break;
                        
                    case "document_viewed":
                        await HandleDocumentViewed(contract, signerEmail);
                        break;
                        
                    default:
                        _logger.LogInformation("Unhandled OpenSign event type: {EventType}", eventType);
                        break;
                }

                return Ok(new { message = "Webhook processed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OpenSign webhook");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        private async Task HandleDocumentSigned(Contract contract, string? status, string? signerEmail, DateTime? signedAt, dynamic webhookData)
        {
            try
            {
                // Update contract status
                contract.SigningStatus = "Signed";
                contract.SignedDate = signedAt ?? DateTime.UtcNow;
                
                // Extract signed document URL if available
                if (webhookData.signedDocumentUrl != null)
                {
                    contract.SignedDocumentUrl = webhookData.signedDocumentUrl.ToString();
                }
                
                // Extract certificate URL if available
                if (webhookData.certificateUrl != null)
                {
                    contract.CertificateUrl = webhookData.certificateUrl.ToString();
                }

                // Update purchase status
                if (contract.Purchase != null)
                {
                    contract.Purchase.Status = "ContractSigned";
                    contract.Purchase.ContractSignedDate = signedAt ?? DateTime.UtcNow;
                    contract.Purchase.ModifiedDate = DateTime.UtcNow;
                    
                    // Add status history entry
                    var statusHistory = new PurchaseStatusHistory
                    {
                        PurchaseId = contract.Purchase.Id,
                        FromStatus = "ContractSentForSigning",
                        ToStatus = "ContractSigned",
                        Status = "ContractSigned",
                        Notes = $"Contract signed electronically by {signerEmail}",
                        ChangedBy = "System",
                        ChangeReason = "OpenSign Webhook",
                        ChangedDate = DateTime.UtcNow,
                        IsSystemGenerated = true
                    };
                    _context.PurchaseStatusHistories.Add(statusHistory);
                }

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Contract {ContractId} marked as signed via OpenSign webhook", contract.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling document signed event for contract {ContractId}", contract.Id);
                throw;
            }
        }

        private async Task HandleDocumentDeclined(Contract contract, string? signerEmail, dynamic webhookData)
        {
            try
            {
                // Update contract status
                contract.SigningStatus = "Declined";
                
                // Extract decline reason if available
                string declineReason = webhookData.declineReason?.ToString() ?? "No reason provided";

                // Update purchase status
                if (contract.Purchase != null)
                {
                    contract.Purchase.Status = "ContractGenerated"; // Reset to allow resending
                    contract.Purchase.ModifiedDate = DateTime.UtcNow;
                    
                    // Add status history entry
                    var statusHistory = new PurchaseStatusHistory
                    {
                        PurchaseId = contract.Purchase.Id,
                        FromStatus = "ContractSentForSigning",
                        ToStatus = "ContractGenerated",
                        Status = "ContractGenerated",
                        Notes = $"Contract declined by {signerEmail}. Reason: {declineReason}",
                        ChangedBy = "System",
                        ChangeReason = "OpenSign Webhook",
                        ChangedDate = DateTime.UtcNow,
                        IsSystemGenerated = true
                    };
                    _context.PurchaseStatusHistories.Add(statusHistory);
                }

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Contract {ContractId} marked as declined via OpenSign webhook. Reason: {Reason}", contract.Id, declineReason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling document declined event for contract {ContractId}", contract.Id);
                throw;
            }
        }

        private async Task HandleDocumentViewed(Contract contract, string? signerEmail)
        {
            try
            {
                // Log document view event
                _logger.LogInformation("Contract {ContractId} viewed by {SignerEmail}", contract.Id, signerEmail);
                
                // Optionally add to status history for audit trail
                if (contract.Purchase != null)
                {
                    var statusHistory = new PurchaseStatusHistory
                    {
                        PurchaseId = contract.Purchase.Id,
                        FromStatus = contract.Purchase.Status,
                        ToStatus = contract.Purchase.Status,
                        Status = contract.Purchase.Status,
                        Notes = $"Contract viewed by {signerEmail}",
                        ChangedBy = "System",
                        ChangeReason = "OpenSign Webhook",
                        ChangedDate = DateTime.UtcNow,
                        IsSystemGenerated = true
                    };
                    _context.PurchaseStatusHistories.Add(statusHistory);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling document viewed event for contract {ContractId}", contract.Id);
                // Don't throw for view events as they're not critical
            }
        }



        [HttpPost("stripe")]
        public async Task<IActionResult> StripeWebhook()
        {
            try
            {
                var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                var stripeSignature = Request.Headers["Stripe-Signature"];
                var webhookSecret = _configuration["Stripe:WebhookSecret"];

                Event stripeEvent;
                try
                {
                    if (!string.IsNullOrEmpty(webhookSecret))
                    {
                        stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);
                    }
                    else
                    {
                        // For development without webhook secret
                        stripeEvent = EventUtility.ParseEvent(json);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse Stripe webhook");
                    return BadRequest("Invalid webhook signature");
                }

                _logger.LogInformation("Received Stripe webhook: {EventType}", 
                    stripeEvent.Type);

                // Handle different event types
                switch (stripeEvent.Type)
                {
                    case "payment_intent.succeeded":
                        await HandlePaymentIntentSucceeded(stripeEvent);
                        break;
                    case "payment_intent.payment_failed":
                        await HandlePaymentIntentFailed(stripeEvent);
                        break;
                    case "payment_intent.processing":
                        await HandlePaymentIntentProcessing(stripeEvent);
                        break;
                    default:
                        _logger.LogInformation("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                        break;
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Stripe webhook");
                return StatusCode(500, "Internal server error");
            }
        }

        private async Task HandlePaymentIntentSucceeded(Event stripeEvent)
        {
            try
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                if (paymentIntent == null) return;

                _logger.LogInformation("Payment succeeded for PaymentIntent: {PaymentIntentId}", paymentIntent.Id);

                // Find the payment record in our database
                var payment = await _context.Payments
                    .Include(p => p.Contract)
                        .ThenInclude(c => c.Purchase)
                            .ThenInclude(p => p.Customer)
                                .ThenInclude(c => c.User)
                    .Include(p => p.Contract.Purchase.Vehicle)
                    .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntent.Id);

                if (payment == null)
                {
                    _logger.LogWarning("Payment not found for PaymentIntent: {PaymentIntentId}", paymentIntent.Id);
                    return;
                }

                // Update payment status
                payment.Status = "Completed";
                payment.ModifiedDate = DateTime.UtcNow;
                _context.Payments.Update(payment);

                // Update purchase status
                var purchase = payment.Contract.Purchase;
                purchase.Status = "Completed";
                purchase.PaymentCompleted = true;
                purchase.PaymentCompletedDate = DateTime.UtcNow;
                purchase.ModifiedDate = DateTime.UtcNow;
                _context.Purchases.Update(purchase);

                // Add status history
                var statusHistory = new PurchaseStatusHistory
                {
                    PurchaseId = purchase.Id,
                    FromStatus = "PaymentProcessing",
                    ToStatus = "Completed",
                    Status = "Completed",
                    Notes = $"Payment completed via Stripe webhook. Transaction ID: {paymentIntent.Id}",
                    ChangedBy = "System",
                    ChangeReason = "Payment Confirmation",
                    ChangedDate = DateTime.UtcNow,
                    IsSystemGenerated = true
                };
                _context.PurchaseStatusHistories.Add(statusHistory);

                await _context.SaveChangesAsync();

                // Send payment confirmation email
                try
                {
                    await _paymentService.SendPaymentProofEmailAsync(payment.Id);
                    _logger.LogInformation("Payment confirmation email sent for payment {PaymentId}", payment.Id);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Failed to send payment confirmation email for payment {PaymentId}", payment.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling payment intent succeeded webhook");
            }
        }

        private async Task HandlePaymentIntentFailed(Event stripeEvent)
        {
            try
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                if (paymentIntent == null) return;

                _logger.LogInformation("Payment failed for PaymentIntent: {PaymentIntentId}", paymentIntent.Id);

                // Find the payment record in our database
                var payment = await _context.Payments
                    .Include(p => p.Contract.Purchase)
                    .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntent.Id);

                if (payment == null)
                {
                    _logger.LogWarning("Payment not found for failed PaymentIntent: {PaymentIntentId}", paymentIntent.Id);
                    return;
                }

                // Update payment status
                payment.Status = "Failed";
                payment.ModifiedDate = DateTime.UtcNow;
                _context.Payments.Update(payment);

                // Update purchase status
                var purchase = payment.Contract.Purchase;
                purchase.Status = "PaymentFailed";
                purchase.ModifiedDate = DateTime.UtcNow;
                _context.Purchases.Update(purchase);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling payment intent failed webhook");
            }
        }

        private async Task HandlePaymentIntentProcessing(Event stripeEvent)
        {
            try
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                if (paymentIntent == null) return;

                _logger.LogInformation("Payment processing for PaymentIntent: {PaymentIntentId}", paymentIntent.Id);

                // Find the payment record in our database
                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntent.Id);

                if (payment == null)
                {
                    _logger.LogWarning("Payment not found for processing PaymentIntent: {PaymentIntentId}", paymentIntent.Id);
                    return;
                }

                // Update payment status
                payment.Status = "Processing";
                payment.ModifiedDate = DateTime.UtcNow;
                _context.Payments.Update(payment);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling payment intent processing webhook");
            }
        }

        [HttpGet("test")]
        public IActionResult TestWebhook()
        {
            return Ok(new { message = "Webhook endpoint is working", timestamp = DateTime.UtcNow });
        }
    }
}