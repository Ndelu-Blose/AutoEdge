using AutoEdge.Data;
using AutoEdge.Models.Entities;
using AutoEdge.Services;
using Stripe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutoEdge.Controllers
{
    public class MockSigningRequest
    {
        public int contractId { get; set; }
    }
    [Authorize]
    public class PurchaseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PurchaseController> _logger;
        private readonly IContractService _contractService;
        private readonly IPaymentService _paymentService;
        private readonly IESignatureService _eSignatureService;

        public PurchaseController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<PurchaseController> logger,
            IContractService contractService,
            IPaymentService paymentService,
            IESignatureService eSignatureService)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _contractService = contractService;
            _paymentService = paymentService;
            _eSignatureService = eSignatureService;
        }

        // GET: Purchase/InitiatePurchase
        public async Task<IActionResult> InitiatePurchase(int vehicleId)
        {
            try
            {
                // Get the vehicle
                var vehicle = await _context.Vehicles
                    .FirstOrDefaultAsync(v => v.Id == vehicleId && v.IsActive && !v.IsDeleted);

                if (vehicle == null)
                {
                    TempData["ErrorMessage"] = "Vehicle not found or no longer available.";
                    return RedirectToAction("Index", "VehicleBrowse");
                }

                if (vehicle.Status != "Available")
                {
                    TempData["ErrorMessage"] = "This vehicle is no longer available for purchase.";
                    return RedirectToAction("Details", "VehicleBrowse", new { id = vehicleId });
                }

                // Get current user
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return Redirect("/Identity/Account/Login");
                }

                // Get or create customer record
                var customer = await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (customer == null)
                {
                    customer = new AutoEdge.Models.Entities.Customer
                    {
                        UserId = userId,
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow,
                        IsActive = true
                    };
                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                }

                // Check if there's already an active purchase for this vehicle by this customer
                var existingPurchase = await _context.Purchases
                    .FirstOrDefaultAsync(p => p.VehicleId == vehicleId && 
                                            p.CustomerId == customer.Id && 
                                            p.IsActive &&
                                            p.Status != "Cancelled" &&
                                            p.Status != "Completed");

                if (existingPurchase != null)
                {
                    TempData["InfoMessage"] = "You already have an active purchase process for this vehicle.";
                    return RedirectToAction("PurchaseStatus", new { id = existingPurchase.Id });
                }

                // Create new purchase record
                var purchase = new Purchase
                {
                    CustomerId = customer.Id,
                    VehicleId = vehicleId,
                    Status = "Initiated",
                    PurchasePrice = vehicle.SellingPrice,
                    InitiatedDate = DateTime.UtcNow,
                    IsActive = true,
                    Notes = "Purchase initiated by customer"
                };

                _context.Purchases.Add(purchase);
                await _context.SaveChangesAsync();

                // Create status history entry
                var statusHistory = new PurchaseStatusHistory
                {
                    PurchaseId = purchase.Id,
                    FromStatus = "",
                    ToStatus = "Initiated",
                    ChangedDate = DateTime.UtcNow,
                    ChangedBy = user.Email ?? "System",
                    ChangeReason = "Purchase process started by customer",
                    Notes = "Customer initiated purchase workflow"
                };

                _context.PurchaseStatusHistories.Add(statusHistory);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Purchase process initiated successfully! Please complete the required documents.";
                return RedirectToAction("PurchaseWorkflow", new { id = purchase.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating purchase for vehicle {VehicleId}", vehicleId);
                TempData["ErrorMessage"] = "An error occurred while starting the purchase process. Please try again.";
                return RedirectToAction("Details", "VehicleBrowse", new { id = vehicleId });
            }
        }

        // POST: Purchase/SubmitForReview
        [HttpGet]
        public async Task<IActionResult> SubmitForReview(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var customer = await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (customer == null)
                {
                    return Redirect("/Identity/Account/Login");
                }

                var purchase = await _context.Purchases
                    .Include(p => p.Documents)
                    .Include(p => p.StatusHistory)
                    .FirstOrDefaultAsync(p => p.Id == id && p.CustomerId == customer.Id);

                if (purchase == null)
                {
                    TempData["ErrorMessage"] = "Purchase not found.";
                    return RedirectToAction("Index", "VehicleBrowse");
                }

                if (purchase.Status == "DocumentsUploaded")
                {
                    // Update purchase status to DocumentsReview
                    purchase.Status = "DocumentsReview";
                    purchase.ModifiedDate = DateTime.UtcNow;

                    // Add status history
                    var statusHistory = new PurchaseStatusHistory
                    {
                        PurchaseId = id,
                        FromStatus = "DocumentsUploaded",
                        ToStatus = "DocumentsReview",
                        ChangedDate = DateTime.UtcNow,
                        Notes = "Documents submitted for admin review",
                        IsSystemGenerated = false
                    };

                    _context.PurchaseStatusHistories.Add(statusHistory);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Your documents have been submitted for review. You will be notified once the review is complete.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Cannot submit documents for review at this time.";
                }

                return RedirectToAction("PurchaseWorkflow", new { id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting documents for review for purchase {PurchaseId}", id);
                TempData["ErrorMessage"] = "An error occurred while submitting documents for review.";
                return RedirectToAction("PurchaseWorkflow", new { id = id });
            }
        }

        // GET: Purchase/PurchaseWorkflow
        public async Task<IActionResult> PurchaseWorkflow(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var customer = await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (customer == null)
                {
                    return Redirect("/Identity/Account/Login");
                }

                var purchase = await _context.Purchases
                    .Include(p => p.Vehicle)
                    .Include(p => p.StatusHistory)
                    .FirstOrDefaultAsync(p => p.Id == id && p.CustomerId == customer.Id);

                if (purchase == null)
                {
                    TempData["ErrorMessage"] = "Purchase not found.";
                    return RedirectToAction("Index", "VehicleBrowse");
                }

                return View(purchase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading purchase workflow for purchase {PurchaseId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the purchase workflow.";
                return RedirectToAction("Index", "VehicleBrowse");
            }
        }

        // GET: Purchase/PurchaseStatus
        public async Task<IActionResult> PurchaseStatus(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (customer == null)
                {
                    return Redirect("/Identity/Account/Login");
                }

                var purchase = await _context.Purchases
                    .Include(p => p.Vehicle)
                    .Include(p => p.StatusHistory.OrderByDescending(sh => sh.ChangedDate))
                    .FirstOrDefaultAsync(p => p.Id == id && p.CustomerId == customer.Id);

                if (purchase == null)
                {
                    TempData["ErrorMessage"] = "Purchase not found.";
                    return RedirectToAction("Index", "VehicleBrowse");
                }

                return View(purchase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading purchase status for purchase {PurchaseId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the purchase status.";
                return RedirectToAction("Index", "VehicleBrowse");
            }
        }

        // GET: Purchase/MyPurchases
        public async Task<IActionResult> MyPurchases()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (customer == null)
                {
                    return View(new List<Purchase>());
                }

                var purchases = await _context.Purchases
                    .Include(p => p.Vehicle)
                    .Where(p => p.CustomerId == customer.Id)
                    .OrderByDescending(p => p.InitiatedDate)
                    .ToListAsync();

                return View(purchases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customer purchases");
                TempData["ErrorMessage"] = "An error occurred while loading your purchases.";
                return View(new List<Purchase>());
            }
        }

        // GET: Purchase/GenerateContract
        public async Task<IActionResult> GenerateContract(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (customer == null)
                {
                    TempData["ErrorMessage"] = "Customer profile not found.";
                    return RedirectToAction("MyPurchases");
                }

                var purchase = await _context.Purchases
                    .Include(p => p.Vehicle)
                    .Include(p => p.Customer)
                    .FirstOrDefaultAsync(p => p.Id == id && p.CustomerId == customer.Id);

                if (purchase == null)
                {
                    TempData["ErrorMessage"] = "Purchase not found.";
                    return RedirectToAction("MyPurchases");
                }

                // Check if purchase is in a state where contract can be generated
                if (purchase.Status != "DocumentsApproved" && purchase.Status != "ContractGenerated")
                {
                    TempData["ErrorMessage"] = "Contract can only be generated after documents are approved.";
                    return RedirectToAction("PurchaseStatus", new { id });
                }

                // Generate contract
                var contractResult = await _contractService.GenerateContractAsync(purchase);

                if (!contractResult.Success)
                {
                    TempData["ErrorMessage"] = $"Failed to generate contract: {contractResult.ErrorMessage}";
                    return RedirectToAction("PurchaseStatus", new { id });
                }

                // Update purchase status
                purchase.Status = "ContractGenerated";
                purchase.ModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Store contract HTML in ViewBag for preview
                ViewBag.ContractHtml = contractResult.ContractHtml;
                ViewBag.ContractNumber = contractResult.ContractNumber;

                return View(purchase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating contract for purchase {PurchaseId}", id);
                TempData["ErrorMessage"] = "An error occurred while generating the contract.";
                return RedirectToAction("MyPurchases");
            }
        }

        // GET: Purchase/DownloadContract
        public async Task<IActionResult> DownloadContract(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (customer == null)
                {
                    TempData["ErrorMessage"] = "Customer profile not found.";
                    return RedirectToAction("MyPurchases");
                }

                var purchase = await _context.Purchases
                    .Include(p => p.Vehicle)
                    .Include(p => p.Customer)
                    .FirstOrDefaultAsync(p => p.Id == id && p.CustomerId == customer.Id);

                if (purchase == null)
                {
                    TempData["ErrorMessage"] = "Purchase not found.";
                    return RedirectToAction("MyPurchases");
                }

                // Generate contract
                var contractResult = await _contractService.GenerateContractAsync(purchase);

                if (!contractResult.Success)
                {
                    TempData["ErrorMessage"] = $"Failed to generate contract: {contractResult.ErrorMessage}";
                    return RedirectToAction("PurchaseStatus", new { id });
                }

                // Return PDF file (for now, we'll return HTML as text file)
                var fileName = $"Contract_{contractResult.ContractNumber}.html";
                return File(contractResult.PdfBytes, "text/html", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading contract for purchase {PurchaseId}", id);
                TempData["ErrorMessage"] = "An error occurred while downloading the contract.";
                return RedirectToAction("MyPurchases");
            }
        }

        // POST: Purchase/SendForSignature/{id} - JSON API endpoint
        [HttpPost]
        [Route("Purchase/SendForSignature/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendForSignature(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var customer = await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (customer == null)
                {
                    return Json(new { success = false, message = "Customer profile not found." });
                }

                var purchase = await _context.Purchases
                    .Include(p => p.Vehicle)
                    .Include(p => p.Customer)
                        .ThenInclude(c => c.User)
                    .Include(p => p.Contracts)
                    .FirstOrDefaultAsync(p => p.Id == id && p.CustomerId == customer.Id);

                if (purchase == null)
                {
                    return Json(new { success = false, message = "Purchase not found." });
                }

                // Check if purchase has a contract generated
                if (purchase.Status != "ContractGenerated")
                {
                    return Json(new { success = false, message = "Contract must be generated before sending for signing." });
                }

                var contract = purchase.Contracts.FirstOrDefault();
                if (contract == null)
                {
                    return Json(new { success = false, message = "Contract not found for this purchase." });
                }

                // Check if already sent for signing
                if (!string.IsNullOrEmpty(contract.OpenSignDocumentId))
                {
                    return Json(new { success = false, message = "Contract has already been sent for signing." });
                }

                // Send contract for e-signature
                try
                {
                    var (documentId, signingUrl) = await _eSignatureService.SendForSignatureAsync(
                        contract.Id,
                        customer.User.Email,
                        $"{customer.User.FirstName} {customer.User.LastName}");

                    // Update purchase status
                    purchase.Status = "ContractSentForSigning";
                    purchase.ModifiedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = "Contract has been sent for electronic signature. Please check your email for signing instructions." });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send contract for signing for purchase {PurchaseId}", id);
                    return Json(new { success = false, message = $"Failed to send contract for signing: {ex.Message}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending contract for signing for purchase {PurchaseId}", id);
                return Json(new { success = false, message = "An error occurred while sending the contract for signing." });
            }
        }

        // POST: Purchase/SendContractForSigning
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendContractForSigning(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var customer = await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (customer == null)
                {
                    return Json(new { success = false, message = "Customer profile not found." });
                }

                var purchase = await _context.Purchases
                    .Include(p => p.Vehicle)
                    .Include(p => p.Customer)
                        .ThenInclude(c => c.User)
                    .Include(p => p.Contracts)
                    .FirstOrDefaultAsync(p => p.Id == id && p.CustomerId == customer.Id);

                if (purchase == null)
                {
                    return Json(new { success = false, message = "Purchase not found." });
                }

                // Check if purchase has a contract generated
                if (purchase.Status != "ContractGenerated")
                {
                    return Json(new { success = false, message = "Contract must be generated before sending for signing." });
                }

                // Get or create contract
                var contract = purchase.Contracts.FirstOrDefault();
                if (contract == null)
                {
                    // Generate contract first
                    var contractResult = await _contractService.GenerateContractAsync(purchase);
                    if (!contractResult.Success)
                    {
                        return Json(new { success = false, message = $"Failed to generate contract: {contractResult.ErrorMessage}" });
                    }

                    // Create contract entity
                    contract = new Contract
                    {
                        ContractNumber = contractResult.ContractNumber,
                        CustomerId = purchase.CustomerId,
                        VehicleId = purchase.VehicleId,
                        PurchaseId = purchase.Id,
                        // Save the contract HTML or PDF to a file path if needed
                        // ContractPath = "path/to/saved/contract.pdf",
                        Status = "Generated",
                        CreatedDate = DateTime.UtcNow,
                        CreatedByUserId = userId,
                        DeliveryAddress = purchase.DeliveryAddress,
                        TotalAmount = purchase.PurchasePrice
                    };
                    _context.Contracts.Add(contract);
                    await _context.SaveChangesAsync();
                }

                // Send contract for e-signature
                try
                {
                    var (documentId, signingUrl) = await _eSignatureService.SendForSignatureAsync(
                        contract.Id,
                        customer.User.Email,
                        $"{customer.User.FirstName} {customer.User.LastName}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send contract for signing for purchase {PurchaseId}", id);
                    return Json(new { success = false, message = $"Failed to send contract for signing: {ex.Message}" });
                }

                // Update purchase status
                purchase.Status = "ContractSentForSigning";
                purchase.ModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Contract has been sent for electronic signature. Please check your email for signing instructions." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending contract for signing for purchase {PurchaseId}", id);
                return Json(new { success = false, message = "An error occurred while sending the contract for signing." });
            }
        }

        // GET: Purchase/SignContract
        public async Task<IActionResult> SignContract(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var customer = await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (customer == null)
                {
                    TempData["ErrorMessage"] = "Customer profile not found.";
                    return RedirectToAction("MyPurchases");
                }

                var purchase = await _context.Purchases
                    .Include(p => p.Contracts)
                    .FirstOrDefaultAsync(p => p.Id == id && p.CustomerId == customer.Id);

                if (purchase == null)
                {
                    TempData["ErrorMessage"] = "Purchase not found.";
                    return RedirectToAction("MyPurchases");
                }

                var contract = purchase.Contracts.FirstOrDefault();
                if (contract == null)
                {
                    TempData["ErrorMessage"] = "Contract not found.";
                    return RedirectToAction("PurchaseStatus", new { id });
                }

                // Redirect to canvas-based signature page
                return RedirectToAction("Sign", "Contract", new { 
                    contractId = contract.Id, 
                    email = customer.User.Email, 
                    name = $"{customer.User.FirstName} {customer.User.LastName}" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing contract signing for purchase {PurchaseId}", id);
                TempData["ErrorMessage"] = "An error occurred while accessing the contract signing.";
                return RedirectToAction("PurchaseStatus", new { id });
            }
        }

        // GET: Purchase/GetSigningUrl/{id} - JSON API endpoint for getting signing URL
        [HttpGet]
        [Route("Purchase/GetSigningUrl/{id}")]
        public async Task<IActionResult> GetSigningUrl(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var customer = await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (customer == null)
                {
                    return Json(new { success = false, message = "Customer profile not found." });
                }

                var purchase = await _context.Purchases
                    .Include(p => p.Contracts)
                    .FirstOrDefaultAsync(p => p.Id == id && p.CustomerId == customer.Id);

                if (purchase == null)
                {
                    return Json(new { success = false, message = "Purchase not found." });
                }

                var contract = purchase.Contracts.FirstOrDefault();
                if (contract == null)
                {
                    return Json(new { success = false, message = "Contract not found." });
                }

                // Generate canvas-based signature URL
                var signingUrl = Url.Action("Sign", "Contract", new { 
                    contractId = contract.Id, 
                    email = customer.User.Email, 
                    name = $"{customer.User.FirstName} {customer.User.LastName}" 
                }, Request.Scheme);

                return Json(new { success = true, signingUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting signing URL for purchase {PurchaseId}", id);
                return Json(new { success = false, message = "An error occurred while getting the signing URL." });
            }
        }

        // GET: Purchase/CheckSigningStatus
        public async Task<IActionResult> CheckSigningStatus(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (customer == null)
                {
                    return Json(new { success = false, message = "Customer not found." });
                }

                var purchase = await _context.Purchases
                    .Include(p => p.Contracts)
                        .ThenInclude(c => c.DigitalSignatures)
                    .FirstOrDefaultAsync(p => p.Id == id && p.CustomerId == customer.Id);

                if (purchase == null)
                {
                    return Json(new { success = false, message = "Purchase not found." });
                }

                var contract = purchase.Contracts.FirstOrDefault();
                if (contract == null)
                {
                    return Json(new { success = false, message = "Contract not found." });
                }

                // Check if contract has digital signatures
                var hasSignature = contract.DigitalSignatures.Any(ds => ds.IsActive);
                var latestSignature = contract.DigitalSignatures
                    .Where(ds => ds.IsActive)
                    .OrderByDescending(ds => ds.SignedDate)
                    .FirstOrDefault();

                var status = hasSignature ? "signed" : "pending";
                var signedDate = latestSignature?.SignedDate;

                // Update contract status if signed
                if (hasSignature && contract.SigningStatus != "signed")
                {
                    contract.SigningStatus = "signed";
                    contract.SignedDate = signedDate;
                    await _context.SaveChangesAsync();
                }

                return Json(new
                {
                    success = true,
                    status = status,
                    isCompleted = hasSignature,
                    isDeclined = false,
                    signedDate = signedDate?.ToString("yyyy-MM-dd HH:mm:ss"),
                    certificateUrl = "", // Not applicable for canvas signatures
                    contractStatus = contract.SigningStatus,
                    signedDocumentUrl = contract.SignedDocumentUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking signing status for purchase {PurchaseId}", id);
                return Json(new { success = false, message = "An error occurred while checking signing status." });
            }
        }

        // GET: Purchase/SigningComplete - Redirect endpoint from OpenSign
        public async Task<IActionResult> SigningComplete(string documentId = "", string status = "")
        {
            try
            {
                if (string.IsNullOrEmpty(documentId))
                {
                    TempData["ErrorMessage"] = "Invalid signing completion request.";
                    return RedirectToAction("MyPurchases");
                }

                // Find contract by OpenSign document ID
                var contract = await _context.Contracts
                    .Include(c => c.Vehicle)
                    .Include(c => c.Customer)
                        .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(c => c.OpenSignDocumentId == documentId);

                if (contract == null)
                {
                    TempData["ErrorMessage"] = "Contract not found.";
                    return RedirectToAction("MyPurchases");
                }

                // Verify user has access to this contract
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (contract.Customer?.UserId != userId)
                {
                    TempData["ErrorMessage"] = "Access denied.";
                    return RedirectToAction("MyPurchases");
                }

                // Get purchase ID to redirect to status page
                var purchase = await _context.Purchases
                    .FirstOrDefaultAsync(p => p.Contracts.Any(c => c.Id == contract.Id));

                if (purchase != null)
                {
                    if (status?.ToLower() == "completed")
                    {
                        TempData["SuccessMessage"] = "Contract has been successfully signed! Thank you for completing the signing process.";
                    }
                    else
                    {
                        TempData["InfoMessage"] = "Signing process completed. Please check the contract status below.";
                    }
                    
                    return RedirectToAction("PurchaseStatus", new { id = purchase.Id });
                }

                TempData["InfoMessage"] = "Signing process completed.";
                return RedirectToAction("MyPurchases");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling signing completion for document {DocumentId}", documentId);
                TempData["ErrorMessage"] = "An error occurred while processing the signing completion.";
                return RedirectToAction("MyPurchases");
            }
        }



        // POST: Purchase/ResendSigningRequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendSigningRequest(int purchaseId)
        {
            try
            {
                var purchase = await _context.Purchases
                    .Include(p => p.Contracts)
                    .Include(p => p.Customer)
                        .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(p => p.Id == purchaseId);

                if (purchase == null)
                {
                    TempData["Error"] = "Purchase not found.";
                    return RedirectToAction("Purchases", "Admin");
                }

                var contract = purchase.Contracts.FirstOrDefault();
                if (contract == null || string.IsNullOrEmpty(contract.OpenSignDocumentId))
                {
                    TempData["Error"] = "Contract not found or not sent for signing yet.";
                    return RedirectToAction("ViewContract", "Admin", new { purchaseId });
                }

                // Resend signing notification
                var success = await _eSignatureService.ResendSigningNotificationAsync(contract.OpenSignDocumentId);
                
                if (success)
                {
                    TempData["Success"] = "Signing request has been resent successfully.";
                    _logger.LogInformation("Resent signing request for purchase {PurchaseId}, contract {ContractId}", 
                        purchaseId, contract.Id);
                }
                else
                {
                    TempData["Error"] = "Failed to resend signing request. Please try again.";
                    _logger.LogWarning("Failed to resend signing request for purchase {PurchaseId}, contract {ContractId}", 
                        purchaseId, contract.Id);
                }

                return RedirectToAction("ViewContract", "Admin", new { purchaseId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending signing request for purchase {PurchaseId}", purchaseId);
                TempData["Error"] = "An error occurred while resending the signing request.";
                return RedirectToAction("ViewContract", "Admin", new { purchaseId });
            }
        }

        [HttpGet]
        public async Task<IActionResult> SelectPayment(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (customer == null)
                {
                    return Redirect("/Identity/Account/Login");
                }

                var purchase = await _context.Purchases
                    .Include(p => p.Vehicle)
                    .FirstOrDefaultAsync(p => p.Id == id && p.CustomerId == customer.Id);

                if (purchase == null)
                {
                    TempData["ErrorMessage"] = "Purchase not found.";
                    return RedirectToAction("MyPurchases");
                }

                if (purchase.Status != "ContractGenerated" && purchase.Status != "ContractSigned" && purchase.Status != "PaymentPending")
                {
                    TempData["ErrorMessage"] = "Payment is not available for this purchase at this time.";
                    return RedirectToAction("PurchaseStatus", new { id });
                }

                return View(purchase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment selection for purchase {PurchaseId}", id);
                TempData["ErrorMessage"] = "Unable to load payment options. Please try again.";
                return RedirectToAction("MyPurchases");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(int purchaseId, string paymentMethod, decimal amount, string paymentType = "FullPayment")
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (customer == null)
                {
                    return Json(new { success = false, message = "Customer not found." });
                }

                var purchase = await _context.Purchases
                    .FirstOrDefaultAsync(p => p.Id == purchaseId && p.CustomerId == customer.Id);

                if (purchase == null)
                {
                    return Json(new { success = false, message = "Purchase not found or access denied." });
                }

                PaymentRequest paymentRequest = paymentMethod.ToLower() switch
                {
                    "creditcard" or "debitcard" => new CreditCardPaymentRequest
                    {
                        PurchaseId = purchaseId,
                        Amount = amount,
                        PaymentMethod = paymentMethod,
                        PaymentType = paymentType,
                        CardNumber = Request.Form["cardNumber"],
                        ExpiryMonth = Request.Form["expiryMonth"],
                        ExpiryYear = Request.Form["expiryYear"],
                        Cvv = Request.Form["cvv"],
                        CardholderName = Request.Form["cardholderName"],
                        BillingAddress = Request.Form["billingAddress"],
                        BillingCity = Request.Form["billingCity"],
                        BillingState = Request.Form["billingState"],
                        BillingZip = Request.Form["billingZip"]
                    },
                    "cash" or "banktransfer" or "check" => new CashPaymentRequest
                    {
                        PurchaseId = purchaseId,
                        Amount = amount,
                        PaymentMethod = paymentMethod,
                        PaymentType = paymentType,
                        PaymentDeadline = DateTime.UtcNow.AddDays(7),
                        PaymentInstructions = "Please follow the payment instructions provided."
                    },
                    "financing" => new FinancingPaymentRequest
                    {
                        PurchaseId = purchaseId,
                        Amount = amount,
                        PaymentMethod = paymentMethod,
                        PaymentType = paymentType,
                        FinancingProvider = Request.Form["financingProvider"],
                        InterestRate = decimal.Parse(Request.Form["interestRate"]),
                        LoanTermMonths = int.Parse(Request.Form["loanTermMonths"]),
                        MonthlyPayment = decimal.Parse(Request.Form["monthlyPayment"]),
                        DownPayment = decimal.Parse(Request.Form["downPayment"])
                    },
                    _ => throw new ArgumentException("Invalid payment method")
                };

                // Log payment request details for debugging
                _logger.LogInformation("Processing payment request for Purchase {PurchaseId}: Method={PaymentMethod}, Amount={Amount}, Type={PaymentType}", 
                    purchaseId, paymentMethod, amount, paymentType);
                
                var result = await _paymentService.ProcessPaymentAsync(paymentRequest);
                
                // Log payment result for debugging
                _logger.LogInformation("Payment processing result for Purchase {PurchaseId}: Success={IsSuccess}, Message={Message}, ErrorCode={ErrorCode}", 
                    purchaseId, result.IsSuccess, result.Message, result.ErrorCode);

                if (result.IsSuccess)
                {
                    // Update purchase status
                    purchase.Status = paymentMethod.ToLower() == "cash" || paymentMethod.ToLower() == "financing" ? "PaymentPending" : "PaymentProcessing";
                    purchase.ModifiedDate = DateTime.UtcNow;
                    _context.Purchases.Update(purchase);
                    await _context.SaveChangesAsync();

                    return Json(new
                    {
                        success = true,
                        message = result.Message,
                        transactionId = result.TransactionId,
                        requiresAction = result.RequiresAction,
                        clientSecret = result.ClientSecret,
                        redirectUrl = Url.Action("PaymentConfirmation", new { purchaseId, paymentId = result.Payment?.Id })
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = result.Message,
                        errorCode = result.ErrorCode
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for purchase {PurchaseId}", purchaseId);
                return Json(new
                {
                    success = false,
                    message = "An error occurred while processing the payment. Please try again."
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckPaymentStatus(string paymentIntentId)
        {
            try
            {
                var result = await _paymentService.CheckPaymentStatusAsync(paymentIntentId);
                
                return Json(new
                {
                    success = result.IsSuccess,
                    status = result.Message,
                    requiresAction = result.RequiresAction,
                    clientSecret = result.ClientSecret,
                    paymentIntentId = result.PaymentIntentId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking payment status for PaymentIntent {PaymentIntentId}", paymentIntentId);
                return Json(new
                {
                    success = false,
                    message = "Failed to check payment status"
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckPaymentStatusByPurchase(int purchaseId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (customer == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var purchase = await _context.Purchases
                    .FirstOrDefaultAsync(p => p.Id == purchaseId && p.CustomerId == customer.Id);

                if (purchase == null)
                {
                    return Json(new { success = false, message = "Purchase not found" });
                }

                // Get the latest payment for this purchase
                var payments = await _paymentService.GetPaymentsByPurchaseIdAsync(purchaseId);
                var latestPayment = payments.OrderByDescending(p => p.CreatedDate).FirstOrDefault();

                if (latestPayment == null || string.IsNullOrEmpty(latestPayment.StripePaymentIntentId))
                {
                    return Json(new { success = false, message = "No payment found for this purchase" });
                }

                var result = await _paymentService.CheckPaymentStatusAsync(latestPayment.StripePaymentIntentId);
                
                return Json(new
                {
                    success = result.IsSuccess,
                    status = result.Message,
                    requiresAction = result.RequiresAction,
                    clientSecret = result.ClientSecret,
                    paymentIntentId = result.PaymentIntentId,
                    purchaseStatus = purchase.Status,
                    paymentCompleted = purchase.PaymentCompleted
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking payment status for Purchase {PurchaseId}", purchaseId);
                return Json(new
                {
                    success = false,
                    message = "Failed to check payment status"
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> PaymentConfirmation(int purchaseId, int? paymentId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (customer == null)
                {
                    return Redirect("/Identity/Account/Login");
                }

                var purchase = await _context.Purchases
                    .Include(p => p.Vehicle)
                    .FirstOrDefaultAsync(p => p.Id == purchaseId && p.CustomerId == customer.Id);

                if (purchase == null)
                {
                    TempData["ErrorMessage"] = "Purchase not found or access denied.";
                    return RedirectToAction("MyPurchases");
                }

                Payment? payment = null;
                if (paymentId.HasValue)
                {
                    payment = await _paymentService.GetPaymentByIdAsync(paymentId.Value);
                }

                ViewBag.Payment = payment;
                return View(purchase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment confirmation for purchase {PurchaseId}", purchaseId);
                TempData["ErrorMessage"] = "Unable to load payment confirmation. Please try again.";
                return RedirectToAction("MyPurchases");
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadReceipt(int purchaseId, int paymentId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (customer == null)
                {
                    return Redirect("/Identity/Account/Login");
                }

                var purchase = await _context.Purchases
                    .Include(p => p.Vehicle)
                    .FirstOrDefaultAsync(p => p.Id == purchaseId && p.CustomerId == customer.Id);

                if (purchase == null)
                {
                    TempData["ErrorMessage"] = "Purchase not found or access denied.";
                    return RedirectToAction("MyPurchases");
                }

                var payment = await _paymentService.GetPaymentByIdAsync(paymentId);
                if (payment == null || payment.ContractId != purchaseId)
                {
                    TempData["ErrorMessage"] = "Payment receipt not found.";
                    return RedirectToAction("PaymentConfirmation", new { purchaseId });
                }

                // Generate receipt content
                var receiptHtml = GenerateReceiptHtml(purchase, payment, customer);
                var receiptBytes = System.Text.Encoding.UTF8.GetBytes(receiptHtml);

                return File(receiptBytes, "text/html", $"Receipt-{purchaseId}-{paymentId}.html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading receipt for purchase {PurchaseId}, payment {PaymentId}", purchaseId, paymentId);
                TempData["ErrorMessage"] = "Unable to download receipt. Please try again.";
                return RedirectToAction("PaymentConfirmation", new { purchaseId });
            }
        }

        private string GenerateReceiptHtml(Purchase purchase, Payment payment, AutoEdge.Models.Entities.Customer customer)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <title>Payment Receipt - AutoEdge</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .header {{ text-align: center; border-bottom: 2px solid #333; padding-bottom: 20px; margin-bottom: 20px; }}
        .company-info {{ text-align: center; margin-bottom: 30px; }}
        .receipt-info {{ margin-bottom: 30px; }}
        .table {{ width: 100%; border-collapse: collapse; margin-bottom: 20px; }}
        .table th, .table td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
        .table th {{ background-color: #f2f2f2; }}
        .total {{ font-weight: bold; font-size: 1.2em; }}
        .footer {{ margin-top: 30px; text-align: center; font-size: 0.9em; color: #666; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>AutoEdge</h1>
        <h2>Payment Receipt</h2>
    </div>
    
    <div class='company-info'>
        <p><strong>AutoEdge Dealership</strong></p>
        <p>123 Auto Street, City, State 12345</p>
        <p>Phone: (555) 123-4567 | Email: info@autoedge.com</p>
    </div>
    
    <div class='receipt-info'>
        <p><strong>Receipt Date:</strong> {DateTime.Now:MMM dd, yyyy HH:mm}</p>
        <p><strong>Transaction ID:</strong> {payment.TransactionId}</p>
        <p><strong>Purchase ID:</strong> #{purchase.Id}</p>
    </div>
    
    <h3>Customer Information</h3>
    <table class='table'>
        <tr><td><strong>Name:</strong></td><td>{customer.User.FirstName} {customer.User.LastName}</td></tr>
        <tr><td><strong>Email:</strong></td><td>{customer.User.Email}</td></tr>
        <tr><td><strong>Phone:</strong></td><td>{customer.User.PhoneNumber}</td></tr>
    </table>
    
    <h3>Vehicle Information</h3>
    <table class='table'>
        <tr><td><strong>Vehicle:</strong></td><td>{purchase.Vehicle.Make} {purchase.Vehicle.Model}</td></tr>
        <tr><td><strong>Year:</strong></td><td>{purchase.Vehicle.Year}</td></tr>
        <tr><td><strong>VIN:</strong></td><td>{purchase.Vehicle.VIN}</td></tr>
        <tr><td><strong>Mileage:</strong></td><td>{purchase.Vehicle.Mileage:N0} miles</td></tr>
    </table>
    
    <h3>Payment Details</h3>
    <table class='table'>
        <tr><td><strong>Payment Method:</strong></td><td>{payment.PaymentMethod}</td></tr>
        <tr><td><strong>Payment Type:</strong></td><td>{payment.PaymentType}</td></tr>
        <tr><td><strong>Payment Date:</strong></td><td>{payment.ProcessedDate:MMM dd, yyyy HH:mm}</td></tr>
        <tr><td><strong>Status:</strong></td><td>{payment.Status}</td></tr>
        <tr class='total'><td><strong>Amount Paid:</strong></td><td>{payment.Amount:C}</td></tr>
    </table>
    
    {(payment.PaymentType == "DepositPayment" ? $"<p><strong>Remaining Balance:</strong> {(purchase.PurchasePrice - payment.Amount):C}</p>" : "")}
    
    <div class='footer'>
        <p>Thank you for your business!</p>
        <p>This receipt serves as proof of payment for the above transaction.</p>
        <p>For questions or concerns, please contact us at (555) 123-4567 or support@autoedge.com</p>
    </div>
</body>
</html>";
        }

        // GET: Purchase/MockSigning - Development mock signing endpoint
        [AllowAnonymous]
        public async Task<IActionResult> MockSigning(int contractId, string email, string name)
        {
            try
            {
                _logger.LogInformation("MockSigning called with contractId: {ContractId}, email: {Email}, name: {Name}", contractId, email, name);
                
                // First check if contract exists at all
                var contractExists = await _context.Contracts.AnyAsync(c => c.Id == contractId);
                _logger.LogInformation("Contract {ContractId} exists: {Exists}", contractId, contractExists);
                
                var contract = await _context.Contracts
                    .Include(c => c.Vehicle)
                    .Include(c => c.Customer)
                    .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == contractId);

                if (contract == null)
                {
                    _logger.LogWarning("Contract {ContractId} not found in query with includes", contractId);
                    TempData["ErrorMessage"] = "Contract not found.";
                    return RedirectToAction("Index", "Home");
                }
                
                _logger.LogInformation("Contract {ContractId} found successfully", contractId);

                var model = new
                {
                    ContractId = contractId,
                    SignerEmail = email,
                    SignerName = name,
                    VehicleInfo = $"{contract.Vehicle?.Year} {contract.Vehicle?.Make} {contract.Vehicle?.Model}",
                    ContractNumber = contract.ContractNumber
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in mock signing for contract {ContractId}", contractId);
                TempData["ErrorMessage"] = "An error occurred during mock signing.";
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Purchase/CompleteMockSigning - Complete mock signing
        [HttpPost]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CompleteMockSigning([FromBody] MockSigningRequest request)
        {
            _logger.LogInformation("=== CompleteMockSigning ACTION CALLED ===");
            try
            {
                _logger.LogInformation("CompleteMockSigning called with ContractId: {ContractId}", request?.contractId);
                
                if (request == null || request.contractId <= 0)
                {
                    _logger.LogWarning("Invalid request or ContractId: {ContractId}", request?.contractId);
                    return Json(new { success = false, message = "Invalid contract ID." });
                }
                
                var contract = await _context.Contracts
                    .Include(c => c.Purchase)
                    .FirstOrDefaultAsync(c => c.Id == request.contractId);

                if (contract == null)
                {
                    _logger.LogWarning("Contract not found for ID: {ContractId}", request.contractId);
                    return Json(new { success = false, message = "Contract not found." });
                }

                // Simulate successful signing
                contract.SigningStatus = "Completed";
                contract.Status = "Signed";
                contract.SignedDate = DateTime.UtcNow;
                contract.SignedDocumentUrl = $"mock_signed_document_{request.contractId}.pdf";

                // Update purchase status
                if (contract.Purchase != null)
                {
                    contract.Purchase.Status = "Contract Signed";
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Mock signing completed for contract {ContractId}", request.contractId);

                return Json(new { 
                    success = true, 
                    message = "Contract signed successfully!",
                    redirectUrl = Url.Action("PurchaseStatus", new { id = contract.PurchaseId ?? 0 })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing mock signing for contract {ContractId}", request.contractId);
                return Json(new { success = false, message = "An error occurred while completing the signing." });
            }
        }
    }
}