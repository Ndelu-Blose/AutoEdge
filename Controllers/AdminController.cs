using AutoEdge.Models.Entities;
using AutoEdge.Data;
using AutoEdge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoEdge.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;
        private readonly IContractService _contractService;
         private readonly IBookingService _bookingService;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            ILogger<AdminController> logger,
            IContractService contractService,
             IBookingService bookingService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
            _contractService = contractService;
            _bookingService = bookingService;
        }

        // GET: Admin
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            

            // Service Booking Statistics
            var totalBookings = await _context.ServiceBookings.CountAsync();
            var pendingBookings = await _context.ServiceBookings
                .Where(b => b.Status == ServiceBookingStatus.Pending)
                .CountAsync();
            var confirmedBookings = await _context.ServiceBookings
                .Where(b => b.Status == ServiceBookingStatus.Confirmed)
                .CountAsync();
            var completedBookings = await _context.ServiceBookings
                .Where(b => b.Status == ServiceBookingStatus.Completed)
                .CountAsync();
            var canceledBookings = await _context.ServiceBookings
                .Where(b => b.Status == ServiceBookingStatus.Canceled)
                .CountAsync();


            // Delivery Statistics
            var totalDeliveries = await _context.Deliveries.CountAsync();
            var pendingDeliveries = await _context.Deliveries
                .Where(d => d.Status == "Scheduled")
                .CountAsync();
            var inTransitDeliveries = await _context.Deliveries
                .Where(d => d.Status == "InTransit")
                .CountAsync();
            var completedDeliveries = await _context.Deliveries
                .Where(d => d.Status == "Delivered")
                .CountAsync();
            var failedDeliveries = await _context.Deliveries
                .Where(d => d.Status == "Failed")
                .CountAsync();
            
            // Purchases needing delivery scheduling
            var purchasesNeedingDelivery = await _context.Purchases
                .Include(p => p.Contracts)
                    .ThenInclude(c => c.Deliveries)
                .Include(p => p.Vehicle)
                .Include(p => p.Customer)
                .Where(p => p.Status == "Completed" && 
                           p.Contracts.Any() && 
                           !p.Contracts.Any(c => c.Deliveries.Any()))
                .ToListAsync();
            
            // Recent deliveries for admin overview
            var recentDeliveries = await _context.Deliveries
                .Include(d => d.Contract)
                    .ThenInclude(c => c.Purchase)
                        .ThenInclude(p => p.Vehicle)
                .Include(d => d.Contract)
                    .ThenInclude(c => c.Purchase)
                        .ThenInclude(p => p.Customer)
                .OrderByDescending(d => d.CreatedDate)
                .Take(5)
                .ToListAsync();
            
            // Unassigned deliveries (no driver assigned)
            var unassignedDeliveries = await _context.Deliveries
                .Include(d => d.Contract)
                    .ThenInclude(c => c.Purchase)
                        .ThenInclude(p => p.Vehicle)
                .Include(d => d.Contract)
                    .ThenInclude(c => c.Purchase)
                        .ThenInclude(p => p.Customer)
                .Where(d => d.DriverUserId == null && d.Status == "Scheduled")
                .ToListAsync();
            
            // Pass delivery data to view
            ViewBag.TotalDeliveries = totalDeliveries;
            ViewBag.PendingDeliveries = pendingDeliveries;
            ViewBag.InTransitDeliveries = inTransitDeliveries;
            ViewBag.CompletedDeliveries = completedDeliveries;
            ViewBag.FailedDeliveries = failedDeliveries;
            ViewBag.PurchasesNeedingDelivery = purchasesNeedingDelivery;
            ViewBag.RecentDeliveries = recentDeliveries;
            ViewBag.UnassignedDeliveries = unassignedDeliveries;

            // Pass service booking data to view
            ViewBag.TotalBookings = totalBookings;
            ViewBag.PendingBookings = pendingBookings;
            ViewBag.ConfirmedBookings = confirmedBookings;
            ViewBag.CompletedBookings = completedBookings;
            ViewBag.CanceledBookings = canceledBookings;
            
            return View(users);
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRoles = new Dictionary<string, IList<string>>();
            
            foreach (var user in users)
            {
                userRoles[user.Id] = await _userManager.GetRolesAsync(user);
            }
            
            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        // GET: Admin/UserDetails/5
        public async Task<IActionResult> UserDetails(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.UserRoles = roles;
            return View(user);
        }

        // GET: Admin/EditUser/5
        public async Task<IActionResult> EditUser(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.ToListAsync();
            
            ViewBag.UserRoles = userRoles;
            ViewBag.AllRoles = allRoles;
            return View(user);
        }

        // POST: Admin/EditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, ApplicationUser model, string[] selectedRoles)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Update user properties
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.IsActive = model.IsActive;
            user.Address = model.Address;
            user.City = model.City;
            user.State = model.State;
            user.ZipCode = model.ZipCode;
            user.Country = model.Country;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // Update user roles
                var userRoles = await _userManager.GetRolesAsync(user);
                var rolesToRemove = userRoles.Except(selectedRoles ?? new string[0]);
                var rolesToAdd = (selectedRoles ?? new string[0]).Except(userRoles);

                _logger.LogInformation($"Current roles for {user.Email}: {string.Join(", ", userRoles)}");
                _logger.LogInformation($"Selected roles for {user.Email}: {string.Join(", ", selectedRoles ?? new string[0])}");
                _logger.LogInformation($"Roles to remove: {string.Join(", ", rolesToRemove)}");
                _logger.LogInformation($"Roles to add: {string.Join(", ", rolesToAdd)}");

                if (rolesToRemove.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    if (!removeResult.Succeeded)
                    {
                        _logger.LogError($"Failed to remove roles from {user.Email}: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}");
                    }
                }
                
                if (rolesToAdd.Any())
                {
                    var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                    if (!addResult.Succeeded)
                    {
                        _logger.LogError($"Failed to add roles to {user.Email}: {string.Join(", ", addResult.Errors.Select(e => e.Description))}");
                        TempData["Error"] = "User updated but role assignment failed: " + string.Join(", ", addResult.Errors.Select(e => e.Description));
                        return RedirectToAction(nameof(Users));
                    }
                }

                _logger.LogInformation($"User {user.Email} updated by admin {User.Identity.Name}");
                TempData["Success"] = "User updated successfully.";
                return RedirectToAction(nameof(Users));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            var allRoles = await _roleManager.Roles.ToListAsync();
            ViewBag.UserRoles = await _userManager.GetRolesAsync(user);
            ViewBag.AllRoles = allRoles;
            return View(model);
        }

        // POST: Admin/ToggleUserStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = !user.IsActive;
            var result = await _userManager.UpdateAsync(user);
            
            if (result.Succeeded)
            {
                _logger.LogInformation($"User {user.Email} status toggled to {(user.IsActive ? "Active" : "Inactive")} by admin {User.Identity.Name}");
                TempData["Success"] = $"User {(user.IsActive ? "activated" : "deactivated")} successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to update user status.";
            }

            return RedirectToAction(nameof(Users));
        }

        // GET: Admin/CreateUser
        public async Task<IActionResult> CreateUser()
        {
            var allRoles = await _roleManager.Roles.ToListAsync();
            ViewBag.AllRoles = allRoles;
            return View();
        }

        // POST: Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(ApplicationUser model, string password, string confirmPassword, string[] selectedRoles)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Password", "Password is required.");
            }
            else if (password != confirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Password and confirmation password do not match.");
            }
            else if (password.Length < 6)
            {
                ModelState.AddModelError("Password", "Password must be at least 6 characters long.");
            }

            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "A user with this email already exists.");
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    City = model.City,
                    State = model.State,
                    ZipCode = model.ZipCode,
                    Country = model.Country,
                    DateOfBirth = model.DateOfBirth,
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedDate = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    // Add user to selected roles
                    if (selectedRoles != null && selectedRoles.Length > 0)
                    {
                        _logger.LogInformation($"Attempting to assign roles {string.Join(", ", selectedRoles)} to user {user.Email}");
                        var roleResult = await _userManager.AddToRolesAsync(user, selectedRoles);
                        if (roleResult.Succeeded)
                        {
                            _logger.LogInformation($"Successfully assigned roles to user {user.Email}");
                        }
                        else
                        {
                            _logger.LogError($"Failed to assign roles to user {user.Email}: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                            TempData["Error"] = "User created but role assignment failed: " + string.Join(", ", roleResult.Errors.Select(e => e.Description));
                            return RedirectToAction(nameof(Users));
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"No roles selected for user {user.Email}");
                    }

                    _logger.LogInformation($"User {user.Email} created by admin {User.Identity.Name}");
                    TempData["Success"] = "User created successfully.";
                    return RedirectToAction(nameof(Users));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            var allRoles = await _roleManager.Roles.ToListAsync();
            ViewBag.AllRoles = allRoles;
            ViewBag.SelectedRoles = selectedRoles;
            ViewBag.Password = password;
            ViewBag.ConfirmPassword = confirmPassword;
            return View(model);
        }

        // GET: Admin/TestUserRoles - Debug action to check user roles
        public async Task<IActionResult> TestUserRoles(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { error = "Email parameter required" });
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Json(new { error = "User not found" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            return Json(new { 
                userId = user.Id,
                email = user.Email,
                name = $"{user.FirstName} {user.LastName}",
                roles = roles.ToArray()
            });
        }

        // GET: Admin/Roles
        public async Task<IActionResult> Roles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }

        // GET: Admin/CreateRole
        public IActionResult CreateRole()
        {
            return View();
        }

        // POST: Admin/CreateRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                ModelState.AddModelError("", "Role name is required.");
                return View();
            }

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (roleExists)
            {
                ModelState.AddModelError("", "Role already exists.");
                return View();
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (result.Succeeded)
            {
                _logger.LogInformation($"Role {roleName} created by admin {User.Identity.Name}");
                TempData["Success"] = "Role created successfully.";
                return RedirectToAction(nameof(Roles));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View();
        }

        // POST: Admin/DeleteRole/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            // Check if any users are assigned to this role
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
            if (usersInRole.Any())
            {
                TempData["Error"] = "Cannot delete role that has users assigned to it.";
                return RedirectToAction(nameof(Roles));
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                _logger.LogInformation($"Role {role.Name} deleted by admin {User.Identity.Name}");
                TempData["Success"] = "Role deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to delete role.";
            }

            return RedirectToAction(nameof(Roles));
        }

        // GET: Admin/Inquiries
        public async Task<IActionResult> Inquiries(string status = "All", int page = 1, int pageSize = 10)
        {
            var inquiriesQuery = _context.Inquiries
                .Include(i => i.Vehicle)
                .Include(i => i.Customer)
                    .ThenInclude(c => c.User)
                .Include(i => i.AssignedTo)
                .AsQueryable();

            if (status != "All")
            {
                inquiriesQuery = inquiriesQuery.Where(i => i.Status == status);
            }

            var totalInquiries = await inquiriesQuery.CountAsync();
            var inquiries = await inquiriesQuery
                .OrderByDescending(i => i.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentStatus = status;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalInquiries / pageSize);
            ViewBag.TotalInquiries = totalInquiries;

            return View(inquiries);
        }

        // GET: Admin/InquiryDetails/5
        public async Task<IActionResult> InquiryDetails(int id)
        {
            var inquiry = await _context.Inquiries
                .Include(i => i.Vehicle)
                    .ThenInclude(v => v.VehicleImages)
                .Include(i => i.Customer)
                    .ThenInclude(c => c.User)
                .Include(i => i.AssignedTo)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inquiry == null)
            {
                return NotFound();
            }

            // Get available staff members for assignment
            var staffMembers = await _userManager.GetUsersInRoleAsync("SalesRepresentative");
            var adminUsers = await _userManager.GetUsersInRoleAsync("Administrator");
            ViewBag.StaffMembers = staffMembers.Concat(adminUsers).ToList();

            return View(inquiry);
        }

        // POST: Admin/UpdateInquiryStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateInquiryStatus(int inquiryId, string status, string? assignedToUserId, string? adminResponse)
        {
            var inquiry = await _context.Inquiries.FindAsync(inquiryId);
            if (inquiry == null)
            {
                return NotFound();
            }

            inquiry.Status = status;
            inquiry.AssignedToUserId = string.IsNullOrEmpty(assignedToUserId) ? null : assignedToUserId;
            inquiry.Response = adminResponse;
            inquiry.ResponseDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Inquiry {inquiryId} status updated to {status} by admin {User.Identity.Name}");
            TempData["Success"] = "Inquiry status updated successfully.";

            return RedirectToAction(nameof(InquiryDetails), new { id = inquiryId });
        }

        // GET: Admin/Reservations
        public async Task<IActionResult> Reservations(string status = "All", int page = 1, int pageSize = 10)
        {
            var reservationsQuery = _context.Reservations
                .Include(r => r.Vehicle)
                .Include(r => r.Customer)
                    .ThenInclude(c => c.User)
                .AsQueryable();

            if (status != "All")
            {
                reservationsQuery = reservationsQuery.Where(r => r.Status == status);
            }

            var totalReservations = await reservationsQuery.CountAsync();
            var reservations = await reservationsQuery
                .OrderByDescending(r => r.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentStatus = status;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalReservations / pageSize);
            ViewBag.TotalReservations = totalReservations;

            return View(reservations);
        }

        // GET: Admin/ReservationDetails/5
        public async Task<IActionResult> ReservationDetails(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Vehicle)
                    .ThenInclude(v => v.VehicleImages)
                .Include(r => r.Customer)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // POST: Admin/UpdateReservationStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateReservationStatus(int reservationId, string status, decimal? refundAmount = null)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
            {
                return NotFound();
            }

            var oldStatus = reservation.Status;
            reservation.Status = status;

            // Handle refund amount for cancelled reservations
            if (status == "Cancelled" && refundAmount.HasValue)
            {
                reservation.RefundAmount = Math.Min(refundAmount.Value, reservation.DepositAmount);
            }

            // Update vehicle availability based on reservation status
            if (oldStatus == "Active" && (status == "Cancelled" || status == "Expired"))
            {
                // Make vehicle available again
                reservation.Vehicle.Status = "Available";
            }
            else if (status == "Confirmed" && reservation.Vehicle.Status != "Sold")
            {
                // Keep vehicle reserved when confirmed
                reservation.Vehicle.Status = "Reserved";
            }
            else if (status == "Completed")
            {
                // Mark vehicle as sold when reservation is completed
                reservation.Vehicle.Status = "Sold";
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Reservation status updated to {status} successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while updating the reservation status.";
            }

            return RedirectToAction(nameof(ReservationDetails), new { id = reservationId });
        }

        // GET: Admin/Purchases
        public async Task<IActionResult> Purchases(string status = "All", int page = 1, int pageSize = 10)
        {
            var purchasesQuery = _context.Purchases
                .Include(p => p.Vehicle)
                .Include(p => p.Customer)
                    .ThenInclude(c => c.User)
                .Include(p => p.StatusHistory)
                .Where(p => p.Status != null) // Filter out null status
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                purchasesQuery = purchasesQuery.Where(p => p.Status == status);
            }

            var totalPurchases = await purchasesQuery.CountAsync();
            var purchases = await purchasesQuery
                .OrderByDescending(p => p.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentStatus = status ?? "All";
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalPurchases / pageSize);
            ViewBag.TotalPurchases = totalPurchases;

            return View(purchases);
        }

        // GET: Admin/PurchaseDetails/5
        public async Task<IActionResult> PurchaseDetails(int id)
        {
            var purchase = await _context.Purchases
                .Include(p => p.Vehicle)
                    .ThenInclude(v => v.VehicleImages)
                .Include(p => p.Customer)
                    .ThenInclude(c => c.User)
                .Include(p => p.Customer)
                    .ThenInclude(c => c.Documents)
                        .ThenInclude(d => d.DocumentType)
                .Include(p => p.StatusHistory)
                .Include(p => p.Contracts)
                    .ThenInclude(c => c.Payments)
                .Where(p => p.Status != null) // Filter out null status
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchase == null)
            {
                return NotFound();
            }

            return View(purchase);
        }

        // POST: Admin/UpdatePurchaseStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePurchaseStatus(int purchaseId, string status, string? adminNotes)
        {
            var purchase = await _context.Purchases
                .Include(p => p.Vehicle)
                .Include(p => p.StatusHistory)
                .FirstOrDefaultAsync(p => p.Id == purchaseId);

            if (purchase == null)
            {
                return NotFound();
            }

            var oldStatus = purchase.Status;
            purchase.Status = status;
            purchase.ModifiedDate = DateTime.UtcNow;
            
            if (!string.IsNullOrEmpty(adminNotes))
            {
                purchase.Notes = adminNotes;
            }

            // Add status history entry
            var statusHistory = new PurchaseStatusHistory
            {
                PurchaseId = purchase.Id,
                Status = status,
                ChangedDate = DateTime.UtcNow,
                ChangedBy = User.Identity.Name ?? "Admin",
                Notes = adminNotes ?? $"Status changed from {oldStatus} to {status}"
            };
            _context.PurchaseStatusHistories.Add(statusHistory);

            // Update vehicle status based on purchase status
            if (status == "Completed")
            {
                purchase.Vehicle.Status = "Sold";
            }
            else if (status == "Cancelled" && oldStatus != "Cancelled")
            {
                purchase.Vehicle.Status = "Available";
            }

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Purchase {purchaseId} status updated to {status} by admin {User.Identity.Name}");
                TempData["Success"] = $"Purchase status updated to {status} successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating purchase {purchaseId} status");
                TempData["Error"] = "An error occurred while updating the purchase status.";
            }

            return RedirectToAction(nameof(PurchaseDetails), new { id = purchaseId });
        }

        // POST: Admin/ApproveDocuments
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveDocuments(int purchaseId, string action, string? notes)
        {
            var purchase = await _context.Purchases
                .Include(p => p.Documents)
                    .ThenInclude(d => d.DocumentType)
                .Include(p => p.Customer)
                .Include(p => p.StatusHistory)
                .FirstOrDefaultAsync(p => p.Id == purchaseId);

            if (purchase == null)
            {
                return NotFound();
            }

            string newStatus;
            string actionMessage;

            if (action == "approve")
            {
                newStatus = "DocumentsApproved";
                actionMessage = "Documents approved";
                
                // Get all documents related to this purchase
                var documentsToUpdate = await _context.Documents
                    .Where(d => d.PurchaseId == purchaseId || (d.CustomerId == purchase.CustomerId && d.PurchaseId == null))
                    .ToListAsync();
                
                foreach (var document in documentsToUpdate)
                {
                    document.Status = "Approved";
                    document.ReviewDate = DateTime.UtcNow;
                    document.ReviewedBy = _userManager.GetUserId(User);
                }
            }
            else if (action == "reject")
            {
                newStatus = "DocumentsRejected";
                actionMessage = "Documents rejected";
                
                // Get all documents related to this purchase
                var documentsToUpdate = await _context.Documents
                    .Where(d => d.PurchaseId == purchaseId || (d.CustomerId == purchase.CustomerId && d.PurchaseId == null))
                    .ToListAsync();
                
                foreach (var document in documentsToUpdate)
                {
                    document.Status = "Rejected";
                    document.ReviewDate = DateTime.UtcNow;
                    document.ReviewedBy = _userManager.GetUserId(User);
                }
            }
            else
            {
                TempData["Error"] = "Invalid action specified.";
                return RedirectToAction(nameof(PurchaseDetails), new { id = purchaseId });
            }

            purchase.Status = newStatus;
            purchase.ModifiedDate = DateTime.UtcNow;
            purchase.Notes = notes ?? actionMessage;

            // Add status history entry
            var statusHistory = new PurchaseStatusHistory
            {
                PurchaseId = purchase.Id,
                Status = newStatus,
                ChangedDate = DateTime.UtcNow,
                ChangedBy = User.Identity.Name ?? "Admin",
                Notes = notes ?? actionMessage
            };
            _context.PurchaseStatusHistories.Add(statusHistory);

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Purchase {purchaseId} documents {action}d by admin {User.Identity.Name}");
                TempData["Success"] = $"{actionMessage} successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error {action}ing documents for purchase {purchaseId}");
                TempData["Error"] = $"An error occurred while {action}ing the documents.";
            }

            return RedirectToAction(nameof(PurchaseDetails), new { id = purchaseId });
        }

        // POST: Admin/GenerateContractForCustomer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateContractForCustomer(int purchaseId)
        {
            var purchase = await _context.Purchases
                .Include(p => p.Vehicle)
                .Include(p => p.Customer)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.Id == purchaseId);

            if (purchase == null)
            {
                TempData["Error"] = "Purchase not found.";
                return RedirectToAction(nameof(Purchases));
            }

            try
            {
                // Generate contract
                var contractResult = await _contractService.GenerateContractAsync(purchase);

                if (!contractResult.Success)
                {
                    TempData["Error"] = $"Failed to generate contract: {contractResult.ErrorMessage}";
                    return RedirectToAction(nameof(PurchaseDetails), new { id = purchaseId });
                }

                // Update purchase status
                purchase.Status = "ContractGenerated";
                purchase.ModifiedDate = DateTime.UtcNow;

                // Add status history entry
                var statusHistory = new PurchaseStatusHistory
                {
                    PurchaseId = purchase.Id,
                    Status = "ContractGenerated",
                    ChangedDate = DateTime.UtcNow,
                    ChangedBy = User.Identity.Name ?? "Admin",
                    Notes = $"Contract {contractResult.ContractNumber} generated by admin"
                };
                _context.PurchaseStatusHistories.Add(statusHistory);

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Contract generated for purchase {purchaseId} by admin {User.Identity.Name}");
                TempData["Success"] = $"Contract {contractResult.ContractNumber} generated successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating contract for purchase {purchaseId}");
                TempData["Error"] = "An error occurred while generating the contract.";
            }

            return RedirectToAction(nameof(PurchaseDetails), new { id = purchaseId });
        }

        // GET: Admin/ViewContract
        public async Task<IActionResult> ViewContract(int purchaseId)
        {
            var purchase = await _context.Purchases
                .Include(p => p.Vehicle)
                .Include(p => p.Customer)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.Id == purchaseId);

            if (purchase == null)
            {
                TempData["Error"] = "Purchase not found.";
                return RedirectToAction(nameof(Purchases));
            }

            // Get the contract for this purchase with digital signatures
            var contract = await _context.Contracts
                .Include(c => c.DigitalSignatures.Where(ds => ds.IsActive))
                .FirstOrDefaultAsync(c => c.PurchaseId == purchaseId);

            try
            {
                // Generate contract for viewing
                var contractResult = await _contractService.GenerateContractAsync(purchase);

                if (!contractResult.Success)
                {
                    TempData["Error"] = $"Failed to generate contract: {contractResult.ErrorMessage}";
                    return RedirectToAction(nameof(PurchaseDetails), new { id = purchaseId });
                }

                ViewBag.ContractHtml = contractResult.ContractHtml;
                ViewBag.ContractNumber = contractResult.ContractNumber;
                ViewBag.PurchaseId = purchaseId;
                ViewBag.Contract = contract; // Pass contract entity to view

                return View(purchase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error viewing contract for purchase {purchaseId}");
                TempData["Error"] = "An error occurred while loading the contract.";
                return RedirectToAction(nameof(PurchaseDetails), new { id = purchaseId });
            }
        }

        // GET: Admin/DownloadContract
        public async Task<IActionResult> DownloadContract(int purchaseId)
        {
            var purchase = await _context.Purchases
                .Include(p => p.Vehicle)
                .Include(p => p.Customer)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.Id == purchaseId);

            if (purchase == null)
            {
                TempData["Error"] = "Purchase not found.";
                return RedirectToAction(nameof(Purchases));
            }

            try
            {
                // Generate contract
                var contractResult = await _contractService.GenerateContractAsync(purchase);

                if (!contractResult.Success)
                {
                    TempData["Error"] = $"Failed to generate contract: {contractResult.ErrorMessage}";
                    return RedirectToAction(nameof(PurchaseDetails), new { id = purchaseId });
                }

                // Return contract file
                var fileName = $"Contract_{contractResult.ContractNumber}.html";
                return File(contractResult.PdfBytes, "text/html", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading contract for purchase {purchaseId}");
                TempData["Error"] = "An error occurred while downloading the contract.";
                return RedirectToAction(nameof(PurchaseDetails), new { id = purchaseId });
            }
        }
        
        // GET: Admin/Bookings
        public async Task<IActionResult> Bookings()
        {
            var bookings = await _bookingService.GetAllBookingsAsync();
            var mechanics = await _context.Mechanics.Where(m => m.IsAvailable).ToListAsync();
            
            ViewBag.Mechanics = mechanics;
            return View(bookings);
        }

        // GET: Admin/Bookings/Details/{id}
        public async Task<IActionResult> BookingDetails(int id)
        {
            var booking = await _bookingService.GetBookingWithJobAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            var mechanics = await _context.Mechanics.Where(m => m.IsAvailable).ToListAsync();
            ViewBag.Mechanics = mechanics;
            
            return View(booking);
        }

        // POST: Admin/Bookings/Reassign
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReassignBooking(int bookingId, int mechanicId)
        {
            try
            {
                var success = await _bookingService.ReassignBookingToMechanicAsync(bookingId, mechanicId);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Booking reassigned successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to reassign booking. The selected mechanic may not be available at the scheduled time.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reassigning booking {BookingId} to mechanic {MechanicId}", bookingId, mechanicId);
                TempData["ErrorMessage"] = "An error occurred while reassigning the booking.";
            }

            return RedirectToAction(nameof(BookingDetails), new { id = bookingId });
        }

        // POST: Admin/Bookings/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBookingStatus(int bookingId, ServiceBookingStatus status)
        {
            try
            {
                var booking = await _context.ServiceBookings.FindAsync(bookingId);
                if (booking == null)
                {
                    return NotFound();
                }

                booking.Status = status;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Booking status updated successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking status for booking {BookingId}", bookingId);
                TempData["ErrorMessage"] = "An error occurred while updating the booking status.";
            }

            return RedirectToAction(nameof(BookingDetails), new { id = bookingId });
        }

        [HttpGet]
        public async Task<IActionResult> ServiceBookings()
        {
            try
            {
                var bookings = await _context.ServiceBookings
                    .Include(b => b.ServiceJob)
                    .ThenInclude(j => j.Mechanic)
                    .OrderByDescending(b => b.CreatedAtUtc)
                    .ToListAsync();

                return View(bookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service bookings");
                TempData["ErrorMessage"] = "Error retrieving service bookings. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ServiceBookingDetails(int id)
        {
            try
            {
                var booking = await _context.ServiceBookings
                    .Include(b => b.ServiceJob)
                    .ThenInclude(j => j.Mechanic)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (booking == null)
                {
                    TempData["ErrorMessage"] = "Service booking not found.";
                    return RedirectToAction("ServiceBookings");
                }

                return View(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service booking details for booking {BookingId}", id);
                TempData["ErrorMessage"] = "Error retrieving booking details. Please try again.";
                return RedirectToAction("ServiceBookings");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateServiceBookingStatus(int id, ServiceBookingStatus status)
        {
            try
            {
                var booking = await _context.ServiceBookings.FindAsync(id);
                if (booking == null)
                {
                    TempData["ErrorMessage"] = "Service booking not found.";
                    return RedirectToAction("ServiceBookings");
                }

                booking.Status = status;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Service booking {BookingId} status updated to {Status}", id, status);
                TempData["SuccessMessage"] = "Booking status updated successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service booking status for booking {BookingId}", id);
                TempData["ErrorMessage"] = "An error occurred while updating the booking status.";
            }

            return RedirectToAction(nameof(ServiceBookingDetails), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> ReassignServiceBooking(int id, int mechanicId)
        {
            try
            {
                var success = await _bookingService.ReassignBookingToMechanicAsync(id, mechanicId);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Booking reassigned successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to reassign booking. The mechanic may not be available at the scheduled time.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reassigning service booking {BookingId} to mechanic {MechanicId}", id, mechanicId);
                TempData["ErrorMessage"] = "An error occurred while reassigning the booking.";
            }

            return RedirectToAction(nameof(ServiceBookingDetails), new { id });
        }
    }
}