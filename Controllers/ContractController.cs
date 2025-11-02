using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoEdge.Data;
using AutoEdge.Models.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace AutoEdge.Controllers
{
    public class ContractController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ContractController> _logger;

        public ContractController(ApplicationDbContext context, ILogger<ContractController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Contract/Sign - Canvas-based signature page
        public async Task<IActionResult> Sign(int contractId, string email, string name)
        {
            try
            {
                _logger.LogInformation("Canvas signature page requested for contractId: {ContractId}, email: {Email}, name: {Name}", contractId, email, name);

                // Get contract details
                var contract = await _context.Contracts
                    .Include(c => c.Vehicle)
                    .Include(c => c.Customer)
                        .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == contractId);

                if (contract == null)
                {
                    _logger.LogWarning("Contract not found: {ContractId}", contractId);
                    return NotFound("Contract not found");
                }

                // Verify email matches (basic validation)
                if (contract.Customer?.User?.Email != email)
                {
                    _logger.LogWarning("Email mismatch for contract {ContractId}. Expected: {ExpectedEmail}, Provided: {ProvidedEmail}", 
                        contractId, contract.Customer?.User?.Email, email);
                    return Unauthorized("Invalid access");
                }

                // Check if already signed
                if (contract.IsDigitallySigned)
                {
                    return View("AlreadySigned", new { ContractNumber = contract.ContractNumber });
                }

                var model = new
                {
                    ContractId = contractId,
                    ContractNumber = contract.ContractNumber,
                    SignerEmail = email,
                    SignerName = name,
                    VehicleInfo = $"{contract.Vehicle?.Year} {contract.Vehicle?.Make} {contract.Vehicle?.Model}",
                    CustomerName = $"{contract.Customer?.User?.FirstName} {contract.Customer?.User?.LastName}"
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading signature page for contract {ContractId}", contractId);
                return StatusCode(500, "An error occurred while loading the signature page");
            }
        }

        // POST: Contract/SaveSignature - Save canvas signature
        [HttpPost]
        public async Task<IActionResult> SaveSignature([FromBody] SaveSignatureRequest request)
        {
            try
            {
                _logger.LogInformation("SaveSignature called for ContractId: {ContractId}", request.ContractId);

                // Validate request
                if (request.ContractId <= 0 || string.IsNullOrEmpty(request.SignatureData))
                {
                    return Json(new { success = false, message = "Invalid signature data" });
                }

                // Get contract
                var contract = await _context.Contracts
                    .Include(c => c.Customer)
                        .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == request.ContractId);

                if (contract == null)
                {
                    return Json(new { success = false, message = "Contract not found" });
                }

                // Verify email matches
                if (contract.Customer?.User?.Email != request.SignerEmail)
                {
                    return Json(new { success = false, message = "Invalid access" });
                }

                // Check if already signed
                if (contract.IsDigitallySigned)
                {
                    return Json(new { success = false, message = "Contract is already signed" });
                }

                // Create digital signature record
                var digitalSignature = new DigitalSignature
                {
                    ContractId = contract.Id,
                    SignerName = request.SignerName,
                    SignerEmail = request.SignerEmail,
                    SignatureData = request.SignatureData, // Base64 encoded canvas data
                    SignedDate = DateTime.UtcNow,
                    IpAddress = GetClientIpAddress(),
                    UserAgent = Request.Headers.UserAgent.ToString(),
                    SignatureType = "Canvas",
                    DocumentHash = GenerateDocumentHash(contract),
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                };

                _context.DigitalSignatures.Add(digitalSignature);

                // Update contract
                contract.IsDigitallySigned = true;
                contract.DigitalSignatureData = request.SignatureData;
                contract.SignedDate = DateTime.UtcNow;
                contract.SigningStatus = "Completed";
                contract.SigningCompletedDate = DateTime.UtcNow;
                contract.Status = "Signed";
                contract.ModifiedDate = DateTime.UtcNow;

                // Update related Purchase status if exists
                if (contract.PurchaseId.HasValue)
                {
                    var purchase = await _context.Purchases.FindAsync(contract.PurchaseId.Value);
                    if (purchase != null)
                    {
                        purchase.Status = "ContractSigned";
                        purchase.ModifiedDate = DateTime.UtcNow;
                        _logger.LogInformation("Updated Purchase {PurchaseId} status to ContractSigned", purchase.Id);
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Contract {ContractId} signed successfully by {SignerEmail}", request.ContractId, request.SignerEmail);

                return Json(new { 
                    success = true, 
                    message = "Contract signed successfully!",
                    redirectUrl = Url.Action("SigningComplete", "Purchase", new { documentId = contract.OpenSignDocumentId })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving signature for contract {ContractId}", request?.ContractId);
                return Json(new { success = false, message = "An error occurred while saving the signature" });
            }
        }

        private string GetClientIpAddress()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                ipAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
            }
            return ipAddress ?? "Unknown";
        }

        private string GenerateDocumentHash(Contract contract)
        {
            // Generate a simple hash based on contract details
            var content = $"{contract.Id}_{contract.ContractNumber}_{contract.CreatedDate:yyyyMMddHHmmss}";
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(content));
            return Convert.ToBase64String(hashBytes);
        }
    }

    public class SaveSignatureRequest
    {
        public int ContractId { get; set; }
        public string SignerName { get; set; } = string.Empty;
        public string SignerEmail { get; set; } = string.Empty;
        public string SignatureData { get; set; } = string.Empty; // Base64 encoded canvas data
    }
}