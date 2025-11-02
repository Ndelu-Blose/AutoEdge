using AutoEdge.Models.Entities;
using AutoEdge.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace AutoEdge.Services
{
    public class ESignatureService : IESignatureService
    {
        // OpenSign integration commented out - replaced with canvas-based digital signature
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ESignatureService> _logger;
        private readonly IEmailService _emailService;

        // Simplified constructor for canvas-based digital signature
        public ESignatureService(
            ApplicationDbContext context,
            ILogger<ESignatureService> logger,
            IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        // Canvas-based signature implementation
        public async Task<(string DocumentId, string SigningUrl)> SendForSignatureAsync(
            int contractId, 
            string signerEmail, 
            string signerName, 
            string? signerPhone = null)
        {
            try
            {
                // Get contract from database
                var contract = await _context.Contracts
                    .Include(c => c.Vehicle)
                    .Include(c => c.Customer)
                    .FirstOrDefaultAsync(c => c.Id == contractId);

                if (contract == null)
                    throw new ArgumentException($"Contract with ID {contractId} not found");

                // Generate canvas-based signature URL
                var documentId = $"canvas_{Guid.NewGuid():N}";
                var signingUrl = $"https://autoedgedealsershipsystemgroup23-agdsbvbbcebtabhj.southafricanorth-01.azurewebsites.net/Contract/Sign?contractId={contractId}&email={Uri.EscapeDataString(signerEmail)}&name={Uri.EscapeDataString(signerName)}";

                // Update contract with canvas signature information
                contract.OpenSignDocumentId = documentId;
                contract.SigningUrl = signingUrl;
                contract.SigningStatus = "Sent";
                contract.SigningRequestSentDate = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();

                // Send email notification to signer
                try
                {
                    var vehicleInfo = $"{contract.Vehicle?.Year} {contract.Vehicle?.Make} {contract.Vehicle?.Model}";
                    await _emailService.SendContractSigningEmailAsync(signerEmail, signerName, signingUrl, vehicleInfo);
                    _logger.LogInformation("Signing notification email sent to {Email} for contract {ContractId}", signerEmail, contractId);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Failed to send signing notification email to {Email} for contract {ContractId}", signerEmail, contractId);
                    // Don't fail the entire operation if email fails
                }

                _logger.LogInformation("Contract {ContractId} prepared for canvas signature with document ID {DocumentId}", 
                    contractId, documentId);

                return (documentId, signingUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending contract {ContractId} for signature", contractId);
                throw;
            }
        }

        // Canvas-based signature status check
        public async Task<ESignatureStatus> GetDocumentStatusAsync(string documentId)
        {
            try
            {
                // Find contract by document ID
                var contract = await _context.Contracts
                    .FirstOrDefaultAsync(c => c.OpenSignDocumentId == documentId);

                if (contract == null)
                {
                    return new ESignatureStatus
                    {
                        Status = "Not Found",
                        IsCompleted = false,
                        IsDeclined = false
                    };
                }

                var status = new ESignatureStatus
                {
                    Status = contract.SigningStatus ?? "Unknown",
                    IsCompleted = contract.IsDigitallySigned,
                    IsDeclined = contract.SigningStatus == "Declined",
                    SignedAt = contract.SignedDate
                };

                return status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document status for {DocumentId}", documentId);
                throw;
            }
        }

        // Canvas-based signatures don't need document download - signatures are stored in database
        public async Task<byte[]> DownloadSignedDocumentAsync(string documentId)
        {
            throw new NotImplementedException("Document download not implemented for canvas-based signatures. Signature data is stored in the database.");
        }

        public async Task<bool> ResendSigningNotificationAsync(string documentId)
        {
            try
            {
                // Find contract by document ID
                var contract = await _context.Contracts
                    .Include(c => c.Customer)
                    .ThenInclude(c => c.User)
                    .Include(c => c.Vehicle)
                    .FirstOrDefaultAsync(c => c.OpenSignDocumentId == documentId);

                if (contract == null)
                {
                    _logger.LogWarning("Contract not found for document ID {DocumentId}", documentId);
                    return false;
                }

                // Resend email notification
                var signerEmail = contract.Customer?.User?.Email ?? "";
                var signerName = contract.Customer?.User?.UserName ?? "";
                var vehicleInfo = $"{contract.Vehicle?.Year} {contract.Vehicle?.Make} {contract.Vehicle?.Model}";
                
                await _emailService.SendContractSigningEmailAsync(signerEmail, signerName, contract.SigningUrl ?? "", vehicleInfo);
                _logger.LogInformation("Resent signing notification for document {DocumentId}", documentId);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending signing notification for document {DocumentId}", documentId);
                return false;
            }
        }

        public async Task<bool> ProcessWebhookAsync(OpenSignWebhookData webhookData)
        {
            // Canvas-based signatures don't use webhooks
            _logger.LogInformation("Webhook processing not implemented for canvas-based signatures");
            return false;
        }
    }
}