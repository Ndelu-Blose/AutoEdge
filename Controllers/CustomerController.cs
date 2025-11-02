using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoEdge.Data;
using AutoEdge.Models;
using AutoEdge.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using AutoEdge.Services;
using System.Security.Claims;

namespace AutoEdge.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOcrService _ocrService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<CustomerController> _logger;
        // Helper method to get Customer ID from current user
        //private async Task<int?> GetCustomerIdAsync()
        //{
        //    var userIdString = _userManager.GetUserId(User);
        //    if (string.IsNullOrEmpty(userIdString))
        //        return null;

        //    var customer = await _context.Customers
        //        .FirstOrDefaultAsync(c => c.UserId == userIdString);

        //    return customer?.Id;
        //}
        private async Task<int?> GetCustomerIdAsync()
        {
            var userIdString = _userManager.GetUserId(User);

            // If user is not logged in or UserId is null, exit early
            if (string.IsNullOrEmpty(userIdString))
                return null;

            // Now safely query the database
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == userIdString);

            // If no customer record exists, create one for existing users
            if (customer == null)
            {
                var user = await _userManager.FindByIdAsync(userIdString);
                if (user != null && await _userManager.IsInRoleAsync(user, "Customer"))
                {
                    customer = new Customer
                    {
                        UserId = userIdString,
                        CustomerType = "Individual",
                        PreferredContact = "Email",
                        CreatedDate = DateTime.UtcNow
                    };

                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                }
            }

            return customer?.Id;
        }

        public CustomerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IOcrService ocrService, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _userManager = userManager;
            _ocrService = ocrService;
            _serviceScopeFactory = serviceScopeFactory;
        }

        // GET: Customer Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var customerId = await GetCustomerIdAsync();
            if (!customerId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get user's inquiries
            var inquiries = await _context.Inquiries
                .Include(i => i.Vehicle)
                .Where(i => i.CustomerId == customerId.Value)
                .OrderByDescending(i => i.CreatedDate)
                .Take(5)
                .ToListAsync();

            // Get user's reservations
            var reservations = await _context.Reservations
                .Include(r => r.Vehicle)
                .Where(r => r.CustomerId == customerId.Value)
                .OrderByDescending(r => r.ReservationDate)
                .Take(5)
                .ToListAsync();

            // Get completed purchases that need delivery scheduling
            var completedPurchases = await _context.Purchases
                .Include(p => p.Vehicle)
                .Include(p => p.Contracts)
                .ThenInclude(c => c.Deliveries)
                .Where(p => p.CustomerId == customerId.Value && p.Status == "Completed")
                .OrderByDescending(p => p.ModifiedDate)
                .Take(5)
                .ToListAsync();

            // Get recent deliveries
            var recentDeliveries = await _context.Deliveries
                .Include(d => d.Contract)
                .ThenInclude(c => c.Purchase)
                .ThenInclude(p => p.Vehicle)
                .Where(d => d.Contract.Purchase.CustomerId == customerId.Value)
                .OrderByDescending(d => d.CreatedDate)
                .Take(3)
                .ToListAsync();

            // Get statistics
            ViewBag.TotalInquiries = await _context.Inquiries
                .CountAsync(i => i.CustomerId == customerId.Value);
            
            ViewBag.ActiveReservations = await _context.Reservations
                .CountAsync(r => r.CustomerId == customerId.Value && r.Status == "Active");
            
            ViewBag.TotalPurchases = await _context.Purchases
                .CountAsync(p => p.CustomerId == customerId.Value);
            
            ViewBag.PendingDeliveries = await _context.Deliveries
                .CountAsync(d => d.Contract.Purchase.CustomerId == customerId.Value && 
                           (d.Status == "Scheduled" || d.Status == "InTransit"));
            

            // Get user's service bookings
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var serviceBookings = await _context.ServiceBookings
                .Include(b => b.ServiceJob)
                .ThenInclude(j => j.Mechanic)
                .Include(b => b.ServiceJob)
                .ThenInclude(j => j.ServiceChecklist)
                .Where(b => b.CustomerId == userId)
                .OrderByDescending(b => b.CreatedAtUtc)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentInquiries = inquiries;
            ViewBag.RecentReservations = reservations;
            ViewBag.CompletedPurchases = completedPurchases;
            ViewBag.RecentDeliveries = recentDeliveries;
            ViewBag.ServiceBookings = serviceBookings;

            return View();
        }

        // GET: Customer/ServiceProgress
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> ServiceProgress(int bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Get the booking and verify it belongs to the customer
            var booking = await _context.ServiceBookings
                .Include(b => b.ServiceJob)
                .ThenInclude(j => j.Mechanic)
                .Include(b => b.ServiceJob)
                .ThenInclude(j => j.ServiceChecklist)
                .ThenInclude(c => c.Items)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.CustomerId == userId);

            if (booking == null)
            {
                TempData["ErrorMessage"] = "Service booking not found.";
                return RedirectToAction("Dashboard");
            }

            return View(booking);
        }


        // GET: Customer/Inquiries
        public async Task<IActionResult> Inquiries(int page = 1)
        {
            var customerId = await GetCustomerIdAsync();
            if (!customerId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }
            int pageSize = 10;

            var inquiries = _context.Inquiries
                .Include(i => i.Vehicle)
                .Where(i => i.CustomerId == customerId.Value)
                .OrderByDescending(i => i.CreatedDate);

            var totalCount = await inquiries.CountAsync();
            var inquiryList = await inquiries
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.TotalCount = totalCount;

            return View(inquiryList);
        }

        // GET: Customer/Reservations
        public async Task<IActionResult> Reservations(int page = 1)
        {
            var customerId = await GetCustomerIdAsync();
            if (!customerId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }
            int pageSize = 10;

            var reservations = _context.Reservations
                .Include(r => r.Vehicle)
                .Where(r => r.CustomerId == customerId.Value)
                .OrderByDescending(r => r.ReservationDate);

            var totalCount = await reservations.CountAsync();
            var reservationList = await reservations
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.TotalCount = totalCount;

            return View(reservationList);
        }

        // POST: Customer/CancelReservation
        [HttpPost]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var userIdString = _userManager.GetUserId(User);
            // TODO: Get actual customer ID from user mapping
            var userId = 1; // Placeholder - should map from Identity user to Customer ID
            var reservation = await _context.Reservations
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == id && r.CustomerId == userId);

            if (reservation == null)
            {
                return Json(new { success = false, message = "Reservation not found." });
            }

            if (reservation.Status != "Active")
            {
                return Json(new { success = false, message = "This reservation cannot be cancelled." });
            }

            // Update reservation status
            reservation.Status = "Cancelled";
            
            // Update vehicle status back to available
            if (reservation.Vehicle != null)
            {
                reservation.Vehicle.Status = "Available";
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Reservation cancelled successfully." });
        }

        // GET: Customer/InquiryDetails/5
        public async Task<IActionResult> InquiryDetails(int id)
        {
            var customerId = await GetCustomerIdAsync();
            if (!customerId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }
            var inquiry = await _context.Inquiries
                .Include(i => i.Vehicle)
                .FirstOrDefaultAsync(i => i.Id == id && i.CustomerId == customerId.Value);

            if (inquiry == null)
            {
                return NotFound();
            }

            return View(inquiry);
        }

        // GET: Customer/ReservationDetails/5
        public async Task<IActionResult> ReservationDetails(int id)
        {
            var customerId = await GetCustomerIdAsync();
            if (!customerId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }
            var reservation = await _context.Reservations
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == id && r.CustomerId == customerId.Value);

            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // POST: Customer/UpdateInquiry
        [HttpPost]
        public async Task<IActionResult> UpdateInquiry(int id, string message)
        {
            var userIdString = _userManager.GetUserId(User);
            // TODO: Get actual customer ID from user mapping
            var userId = 1; // Placeholder - should map from Identity user to Customer ID
            var inquiry = await _context.Inquiries
                .FirstOrDefaultAsync(i => i.Id == id && i.CustomerId == userId);

            if (inquiry == null)
            {
                return Json(new { success = false, message = "Inquiry not found." });
            }

            if (inquiry.Status == "Closed")
            {
                return Json(new { success = false, message = "This inquiry is closed and cannot be updated." });
            }

            inquiry.Message = message;
            inquiry.ModifiedDate = DateTime.Now; // Update timestamp

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Inquiry updated successfully." });
        }

        // GET: Customer/Profile
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Customer/UpdateProfile
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(ApplicationUser model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Update only allowed fields
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.City = model.City;
            user.State = model.State;
            user.ZipCode = model.ZipCode;
            user.Country = model.Country;
            user.DateOfBirth = model.DateOfBirth;

            var result = await _userManager.UpdateAsync(user);
            
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profile updated successfully.";
                return RedirectToAction(nameof(Profile));
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View("Profile", user);
            }
        }

        // GET: Customer/Documents
        public async Task<IActionResult> Documents(int? purchaseId = null)
        {
            var customerId = await GetCustomerIdAsync();
            if (!customerId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var documentsQuery = _context.Documents
                .Include(d => d.DocumentType)
                .Include(d => d.Reviewer)
                .Where(d => d.CustomerId == customerId.Value);

            // Filter by purchase if specified
            if (purchaseId.HasValue)
            {
                documentsQuery = documentsQuery.Where(d => d.PurchaseId == purchaseId.Value);
                ViewBag.PurchaseId = purchaseId.Value;
                
                // Get purchase details for context
                var purchase = await _context.Purchases
                    .Include(p => p.Vehicle)
                    .FirstOrDefaultAsync(p => p.Id == purchaseId.Value && p.CustomerId == customerId.Value);
                ViewBag.Purchase = purchase;
            }
            else
            {
                // Show only general documents (not associated with any purchase)
                documentsQuery = documentsQuery.Where(d => d.PurchaseId == null);
            }

            var documents = await documentsQuery
                .OrderByDescending(d => d.CreatedDate)
                .ToListAsync();

            var documentTypes = await _context.DocumentTypes
                .OrderBy(dt => dt.Name)
                .ToListAsync();

            ViewBag.DocumentTypes = documentTypes;
            return View(documents);
        }

        // POST: Customer/UploadDocument
        [HttpPost]
        public async Task<IActionResult> UploadDocument(IFormFile file, int documentTypeId, string description, int? purchaseId = null)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "Please select a file to upload." });
            }

            var customerId = await GetCustomerIdAsync();
            if (!customerId.HasValue)
            {
                return Json(new { success = false, message = "Customer not found. Please ensure you are logged in properly." });
            }

            // Get document type to validate file requirements
            var documentType = await _context.DocumentTypes.FindAsync(documentTypeId);
            if (documentType == null)
            {
                return Json(new { success = false, message = "Invalid document type." });
            }

            // Validate file size
            if (file.Length > documentType.MaxFileSizeBytes)
            {
                var maxSizeMB = documentType.MaxFileSizeBytes / (1024 * 1024);
                return Json(new { success = false, message = $"File size exceeds the maximum allowed size of {maxSizeMB} MB." });
            }

            // Validate file type
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = documentType.AllowedFileTypes.Split(',').Select(x => x.Trim().ToLowerInvariant()).ToList();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return Json(new { success = false, message = $"File type {fileExtension} is not allowed. Allowed types: {documentType.AllowedFileTypes}" });
            }

            try
            {
                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "documents");
                Directory.CreateDirectory(uploadsPath);

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Save file to disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Create document record
                var document = new Document
                {
                    CustomerId = customerId.Value,
                    DocumentTypeId = documentTypeId,
                    FileName = file.FileName,
                    FilePath = $"/uploads/documents/{fileName}",
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    Description = description,
                    Status = "Pending",
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    IsOcrProcessed = false,
                    PurchaseId = purchaseId
                };

                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                // Process OCR in background (simulate async processing)
                _ = Task.Run(async () => await ProcessDocumentOcrAsync(document.Id, filePath, documentType));

                // Update purchase status if this is a purchase-related document
                if (purchaseId.HasValue)
                {
                    await UpdatePurchaseDocumentStatus(purchaseId.Value);
                }

                return Json(new { success = true, message = "Document uploaded successfully and is being processed for review." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while uploading the document. Please try again." });
            }
        }

        // GET: Customer/DocumentDetails/5
        public async Task<IActionResult> DocumentDetails(int id)
        {
            var customerId = await GetCustomerIdAsync();
            if (!customerId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var document = await _context.Documents
                .Include(d => d.DocumentType)
                .Include(d => d.Reviewer)
                .FirstOrDefaultAsync(d => d.Id == id && d.CustomerId == customerId.Value);

            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        // POST: Customer/DeleteDocument
        [HttpPost]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var customerId = await GetCustomerIdAsync();
            if (!customerId.HasValue)
            {
                return Json(new { success = false, message = "Customer not found. Please ensure you are logged in properly." });
            }

            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == id && d.CustomerId == customerId.Value);

            if (document == null)
            {
                return Json(new { success = false, message = "Document not found." });
            }

            // Only allow deletion of pending or rejected documents
            if (document.Status == "Approved")
            {
                return Json(new { success = false, message = "Approved documents cannot be deleted." });
            }

            try
            {
                // Delete physical file
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", document.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                // Delete database record
                _context.Documents.Remove(document);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Document deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while deleting the document." });
            }
        }

        private async Task ProcessDocumentOcrAsync(int documentId, string filePath, DocumentType documentType)
        {
            try
            {
                // Extract text using OCR service
                var ocrResult = await _ocrService.ExtractTextAsync(filePath, documentType);
                
                if (ocrResult.Success)
                {
                    // Validate the extracted text
                    var validationResult = await _ocrService.ValidateDocumentAsync(ocrResult.ExtractedText, documentType);
                    
                    // Update document with OCR results
                     using var scope = _serviceScopeFactory.CreateScope();
                     var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var document = await context.Documents.FindAsync(documentId);
                    
                    if (document != null)
                    {
                        document.ExtractedText = ocrResult.ExtractedText;
                        document.IsOcrProcessed = true;
                        document.OcrProcessedDate = DateTime.Now;
                        document.OcrValidationResults = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            IsValid = validationResult.IsValid,
                            ConfidenceScore = validationResult.ConfidenceScore,
                            ValidationErrors = validationResult.ValidationErrors,
                            ExtractedFields = ocrResult.ExtractedFields
                        });
                        
                        // Auto-approve if validation is successful and confidence is high
                        if (validationResult.IsValid && validationResult.ConfidenceScore > 0.85)
                        {
                            document.Status = "Approved";
                            document.ReviewDate = DateTime.Now;
                            document.ValidationNotes = "Auto-approved based on OCR validation";
                        }
                        else if (!validationResult.IsValid)
                        {
                            document.Status = "Rejected";
                            document.ReviewDate = DateTime.Now;
                            document.ValidationNotes = string.Join("; ", validationResult.ValidationErrors);
                        }
                        
                        document.ModifiedDate = DateTime.Now;
                        await context.SaveChangesAsync();
                    }
                }
                else
                {
                    // OCR failed, mark as needing manual review
                     using var scope = _serviceScopeFactory.CreateScope();
                     var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var document = await context.Documents.FindAsync(documentId);
                    
                    if (document != null)
                    {
                        document.IsOcrProcessed = false;
                        document.ValidationNotes = $"OCR processing failed: {ocrResult.ErrorMessage}";
                        document.ModifiedDate = DateTime.Now;
                        await context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error and mark document for manual review
                 using var scope = _serviceScopeFactory.CreateScope();
                 var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var document = await context.Documents.FindAsync(documentId);
                
                if (document != null)
                {
                    document.IsOcrProcessed = false;
                    document.ValidationNotes = $"OCR processing error: {ex.Message}";
                    document.ModifiedDate = DateTime.Now;
                    await context.SaveChangesAsync();
                }
            }
        }

        private async Task UpdatePurchaseDocumentStatus(int purchaseId)
        {
            try
            {
                var purchase = await _context.Purchases
                    .Include(p => p.Documents)
                    .Include(p => p.StatusHistory)
                    .FirstOrDefaultAsync(p => p.Id == purchaseId);

                if (purchase == null) return;

                // Get required document types for purchase
                var requiredDocumentTypes = await _context.DocumentTypes
                    .Where(dt => dt.IsRequired && dt.IsActive)
                    .ToListAsync();

                // Check if at least one document is uploaded (modified for testing)
                var uploadedDocumentTypeIds = purchase.Documents
                    .Where(d => d.Status != "Rejected")
                    .Select(d => d.DocumentTypeId)
                    .Distinct()
                    .ToList();

                var hasAllRequiredDocuments = requiredDocumentTypes
                    .All(rdt => uploadedDocumentTypeIds.Contains(rdt.Id));
                
                // For testing: Allow progression with at least one document
                var hasAtLeastOneDocument = uploadedDocumentTypeIds.Any();

                // Update purchase status based on document status
                if (hasAtLeastOneDocument && purchase.Status == "Initiated")
                {
                    purchase.Status = "DocumentsUploaded";
                    purchase.DocumentsSubmittedDate = DateTime.UtcNow;
                    purchase.ModifiedDate = DateTime.UtcNow;

                    // Add status history
                    var statusHistory = new PurchaseStatusHistory
                    {
                        PurchaseId = purchaseId,
                        FromStatus = "Initiated",
                        ToStatus = "DocumentsUploaded",
                        ChangedDate = DateTime.UtcNow,
                        Notes = "Documents uploaded - ready for review",
                        IsSystemGenerated = true
                    };

                    _context.PurchaseStatusHistories.Add(statusHistory);
                    await _context.SaveChangesAsync();
                }
                else if (!hasAllRequiredDocuments && purchase.Status == "DocumentsUploaded")
                {
                    // Revert status if documents are missing
                    purchase.Status = "DocumentsRequired";
                    purchase.ModifiedDate = DateTime.UtcNow;

                    // Add status history
                    var statusHistory = new PurchaseStatusHistory
                    {
                        PurchaseId = purchaseId,
                        FromStatus = "DocumentsUploaded",
                        ToStatus = "DocumentsRequired",
                        ChangedDate = DateTime.UtcNow,
                        Notes = "Additional documents required",
                        IsSystemGenerated = true
                    };

                    _context.PurchaseStatusHistories.Add(statusHistory);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw to avoid breaking document upload
                // In a real application, you would use proper logging
                Console.WriteLine($"Error updating purchase document status: {ex.Message}");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> MyBookings()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized();
                }

                var bookings = await _context.ServiceBookings
                    .Include(b => b.ServiceJob)
                    .ThenInclude(j => j.Mechanic)
                    .Where(b => b.CustomerId == currentUser.Id)
                    .OrderByDescending(b => b.CreatedAtUtc)
                    .ToListAsync();

                return View(bookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer bookings");
                TempData["ErrorMessage"] = "Error retrieving your bookings. Please try again.";
                return RedirectToAction("Index");
            }
        }
    }
}