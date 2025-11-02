// using AutoEdge.Data;
// using AutoEdge.Models.Entities;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;

// namespace AutoEdge.Controllers
// {
//     [Authorize(Roles = "Driver")]
//     public class DriverController : Controller
//     {
//         private readonly ApplicationDbContext _context;
//         private readonly UserManager<ApplicationUser> _userManager;
//         private readonly ILogger<DriverController> _logger;

//         public DriverController(
//             ApplicationDbContext context,
//             UserManager<ApplicationUser> userManager,
//             ILogger<DriverController> logger)
//         {
//             _context = context;
//             _userManager = userManager;
//             _logger = logger;
//         }

//         // GET: Driver
//         public async Task<IActionResult> Index()
//         {
//             var user = await _userManager.GetUserAsync(User);
//             if (user == null) return NotFound();

//             var pickups = await _context.VehiclePickups
//                 .Include(p => p.ServiceBooking)
//                 .ThenInclude(b => b.Customer)
//                 .Where(p => p.DriverId == user.Id)
//                 .OrderBy(p => p.PickupDate)
//                 .ThenBy(p => p.PickupTime)
//                 .ToListAsync();

//             return View(pickups);
//         }

//         // GET: Driver/MyJobs
//         public async Task<IActionResult> MyJobs()
//         {
//             var user = await _userManager.GetUserAsync(User);
//             if (user == null) return NotFound();

//             var today = DateTime.Today;
//             var jobs = await _context.VehiclePickups
//                 .Include(p => p.ServiceBooking)
//                 .ThenInclude(b => b.Customer)
//                 .Where(p => p.DriverId == user.Id && 
//                            p.PickupDate.Date == today &&
//                            p.PickupStatus != "Completed" &&
//                            p.PickupStatus != "Delivered")
//                 .OrderBy(p => p.PickupTime)
//                 .ToListAsync();

//             return View(jobs);
//         }

//         // GET: Driver/JobDetails/5
//         public async Task<IActionResult> JobDetails(int id)
//         {
//             var user = await _userManager.GetUserAsync(User);
//             if (user == null) return NotFound();

//             var pickup = await _context.VehiclePickups
//                 .Include(p => p.ServiceBooking)
//                 .ThenInclude(b => b.Customer)
//                 .FirstOrDefaultAsync(p => p.VehiclePickupId == id && p.DriverId == user.Id);

//             if (pickup == null) return NotFound();

//             return View(pickup);
//         }

//         // POST: Driver/UpdateStatus
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> UpdateStatus(int vehiclePickupId, string status, string? notes = null)
//         {
//             try
//             {
//                 var user = await _userManager.GetUserAsync(User);
//                 if (user == null) return NotFound();

//                 var pickup = await _context.VehiclePickups
//                     .FirstOrDefaultAsync(p => p.VehiclePickupId == vehiclePickupId && p.DriverId == user.Id);

//                 if (pickup == null) return NotFound();

//                 pickup.PickupStatus = status;
//                 pickup.UpdatedAt = DateTime.UtcNow;

//                 // Add status-specific logic
//                 switch (status)
//                 {
//                     case "OnRoute":
//                         // Driver is on the way to pickup location
//                         break;
//                     case "Collected":
//                         // Vehicle has been collected, record pickup details
//                         if (string.IsNullOrEmpty(pickup.VehicleConditionPickup))
//                         {
//                             pickup.VehicleConditionPickup = notes ?? "Vehicle collected in good condition";
//                         }
//                         break;
//                     case "InService":
//                         // Vehicle delivered to service center
//                         break;
//                     case "Completed":
//                         // Service completed, ready for return
//                         break;
//                     case "Delivered":
//                         // Vehicle returned to customer
//                         if (string.IsNullOrEmpty(pickup.VehicleConditionDropoff))
//                         {
//                             pickup.VehicleConditionDropoff = notes ?? "Vehicle returned in good condition";
//                         }
//                         pickup.DropoffDate = DateTime.Now.Date;
//                         pickup.DropoffTime = DateTime.Now.TimeOfDay;
//                         break;
//                     case "Failed":
//                         // Pickup/delivery failed
//                         break;
//                 }

//                 await _context.SaveChangesAsync();

//                 TempData["SuccessMessage"] = $"Status updated to: {status}";
//                 return RedirectToAction(nameof(JobDetails), new { id = vehiclePickupId });
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error updating pickup status");
//                 TempData["ErrorMessage"] = "Error updating status. Please try again.";
//                 return RedirectToAction(nameof(JobDetails), new { id = vehiclePickupId });
//             }
//         }

//         // GET: Driver/RecordPickupDetails/5
//         public async Task<IActionResult> RecordPickupDetails(int id)
//         {
//             var user = await _userManager.GetUserAsync(User);
//             if (user == null) return NotFound();

//             var pickup = await _context.VehiclePickups
//                 .Include(p => p.ServiceBooking)
//                 .ThenInclude(b => b.Customer)
//                 .FirstOrDefaultAsync(p => p.VehiclePickupId == id && p.DriverId == user.Id);

//             if (pickup == null) return NotFound();

//             return View(pickup);
//         }

//         // POST: Driver/RecordPickupDetails
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> RecordPickupDetails(int vehiclePickupId, VehiclePickup model)
//         {
//             try
//             {
//                 var user = await _userManager.GetUserAsync(User);
//                 if (user == null) return NotFound();

//                 var pickup = await _context.VehiclePickups
//                     .FirstOrDefaultAsync(p => p.VehiclePickupId == vehiclePickupId && p.DriverId == user.Id);

//                 if (pickup == null) return NotFound();

//                 // Update pickup details
//                 pickup.VehicleConditionPickup = model.VehicleConditionPickup;
//                 pickup.MileagePickup = model.MileagePickup;
//                 pickup.FuelLevelPickup = model.FuelLevelPickup;
//                 pickup.PickupPhotos = model.PickupPhotos;
//                 pickup.CustomerSignatures = model.CustomerSignatures;
//                 pickup.UpdatedAt = DateTime.UtcNow;

//                 await _context.SaveChangesAsync();

//                 TempData["SuccessMessage"] = "Pickup details recorded successfully!";
//                 return RedirectToAction(nameof(JobDetails), new { id = vehiclePickupId });
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error recording pickup details");
//                 TempData["ErrorMessage"] = "Error recording details. Please try again.";
//                 return RedirectToAction(nameof(RecordPickupDetails), new { id = vehiclePickupId });
//             }
//         }

//         // GET: Driver/RecordDropoffDetails/5
//         public async Task<IActionResult> RecordDropoffDetails(int id)
//         {
//             var user = await _userManager.GetUserAsync(User);
//             if (user == null) return NotFound();

//             var pickup = await _context.VehiclePickups
//                 .Include(p => p.ServiceBooking)
//                 .ThenInclude(b => b.Customer)
//                 .FirstOrDefaultAsync(p => p.VehiclePickupId == id && p.DriverId == user.Id);

//             if (pickup == null) return NotFound();

//             return View(pickup);
//         }

//         // POST: Driver/RecordDropoffDetails
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> RecordDropoffDetails(int vehiclePickupId, VehiclePickup model)
//         {
//             try
//             {
//                 var user = await _userManager.GetUserAsync(User);
//                 if (user == null) return NotFound();

//                 var pickup = await _context.VehiclePickups
//                     .FirstOrDefaultAsync(p => p.VehiclePickupId == vehiclePickupId && p.DriverId == user.Id);

//                 if (pickup == null) return NotFound();

//                 // Update dropoff details
//                 pickup.VehicleConditionDropoff = model.VehicleConditionDropoff;
//                 pickup.MileageDropoff = model.MileageDropoff;
//                 pickup.FuelLevelDropoff = model.FuelLevelDropoff;
//                 pickup.DropoffPhotos = model.DropoffPhotos;
//                 pickup.CustomerSignatures = model.CustomerSignatures;
//                 pickup.DropoffDate = DateTime.Now.Date;
//                 pickup.DropoffTime = DateTime.Now.TimeOfDay;
//                 pickup.UpdatedAt = DateTime.UtcNow;

//                 await _context.SaveChangesAsync();

//                 TempData["SuccessMessage"] = "Dropoff details recorded successfully!";
//                 return RedirectToAction(nameof(JobDetails), new { id = vehiclePickupId });
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error recording dropoff details");
//                 TempData["ErrorMessage"] = "Error recording details. Please try again.";
//                 return RedirectToAction(nameof(RecordDropoffDetails), new { id = vehiclePickupId });
//             }
//         }

//         // GET: Driver/RouteOptimization
//         public async Task<IActionResult> RouteOptimization()
//         {
//             var user = await _userManager.GetUserAsync(User);
//             if (user == null) return NotFound();

//             var today = DateTime.Today;
//             var jobs = await _context.VehiclePickups
//                 .Include(p => p.ServiceBooking)
//                 .ThenInclude(b => b.Customer)
//                 .Where(p => p.DriverId == user.Id && 
//                            p.PickupDate.Date == today &&
//                            p.PickupStatus == "Scheduled")
//                 .OrderBy(p => p.PickupTime)
//                 .ToListAsync();

//             // Simple route optimization - sort by pickup time for now
//             // In a real implementation, you would use a mapping service like Google Maps API
//             var optimizedRoute = jobs.OrderBy(j => j.PickupTime).ToList();

//             return View(optimizedRoute);
//         }

//         // GET: Driver/History
//         public async Task<IActionResult> History()
//         {
//             var user = await _userManager.GetUserAsync(User);
//             if (user == null) return NotFound();

//             var history = await _context.VehiclePickups
//                 .Include(p => p.ServiceBooking)
//                 .ThenInclude(b => b.Customer)
//                 .Where(p => p.DriverId == user.Id)
//                 .OrderByDescending(p => p.PickupDate)
//                 .Take(50) // Last 50 jobs
//                 .ToListAsync();

//             return View(history);
//         }
//     }
// }


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoEdge.Data;
using AutoEdge.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace AutoEdge.Controllers
{
    [Authorize(Roles = "Driver")]
    public class DriverController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DriverController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get the Driver entity for this user
            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == currentUser.Id);
            int? driverId = driver?.Id;

            // Get deliveries assigned to this driver
            var assignedDeliveries = await _context.Deliveries
                .Include(d => d.Contract)
                    .ThenInclude(c => c.Customer)
                        .ThenInclude(cu => cu.User)
                .Include(d => d.Contract)
                    .ThenInclude(c => c.Vehicle)
                .Where(d => d.DriverUserId == currentUser.Id)
                .OrderBy(d => d.ScheduledDate)
                .ToListAsync();

            // Get pickups assigned to this driver
            var assignedPickups = new List<VehiclePickup>();
            if (driverId.HasValue)
            {
                assignedPickups = await _context.VehiclePickups
                    .Include(p => p.ServiceBooking)
                        .ThenInclude(sb => sb.Customer)
                    .Where(p => p.DriverId == driverId.Value)
                    .OrderBy(p => p.PickupDate)
                    .ThenBy(p => p.PickupTime)
                    .ToListAsync();
            }

            // Calculate combined statistics
            var totalDeliveries = assignedDeliveries.Count;
            var totalPickups = assignedPickups.Count;
            var totalAssignments = totalDeliveries + totalPickups;
            
            var completedDeliveries = assignedDeliveries.Count(d => d.Status == "Delivered");
            var completedPickups = assignedPickups.Count(p => p.PickupStatus == "Completed" || p.PickupStatus == "Delivered");
            
            var pendingDeliveries = assignedDeliveries.Count(d => d.Status == "Pending" || d.Status == "In Transit");
            var pendingPickups = assignedPickups.Count(p => p.PickupStatus == "Scheduled" || p.PickupStatus == "Assigned" || p.PickupStatus == "EnRoute");
            
            var todayDeliveries = assignedDeliveries.Count(d => d.ScheduledDate.Date == DateTime.Today);
            var todayPickups = assignedPickups.Count(p => p.PickupDate.Date == DateTime.Today);
            
            var totalCompleted = completedDeliveries + completedPickups;
            var totalPending = pendingDeliveries + pendingPickups;
            var totalToday = todayDeliveries + todayPickups;

            ViewBag.TotalDeliveries = totalAssignments;
            ViewBag.CompletedDeliveries = totalCompleted;
            ViewBag.PendingDeliveries = totalPending;
            ViewBag.TodayDeliveries = totalToday;
            ViewBag.DriverName = currentUser.UserName;
            
            // Pass both lists to the view
            ViewBag.AssignedDeliveries = assignedDeliveries;
            ViewBag.AssignedPickups = assignedPickups;

            return View(assignedDeliveries);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDeliveryStatus(int deliveryId, string status)
        {
            var delivery = await _context.Deliveries.FindAsync(deliveryId);
            if (delivery == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (delivery.DriverUserId != currentUser.Id)
            {
                return Forbid();
            }

            delivery.Status = status;
            if (status == "Delivered")
            {
                delivery.CompletedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Dashboard));
        }

        public async Task<IActionResult> DeliveryDetails(int id)
        {
            var delivery = await _context.Deliveries
                .Include(d => d.Contract)
                    .ThenInclude(c => c.Customer)
                        .ThenInclude(cu => cu.User)
                .Include(d => d.Contract)
                    .ThenInclude(c => c.Vehicle)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (delivery == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (delivery.DriverUserId != currentUser.Id)
            {
                return Forbid();
            }

            return View(delivery);
        }
    }
}