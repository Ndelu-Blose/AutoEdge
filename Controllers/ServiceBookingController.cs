// using AutoEdge.Data;
// using AutoEdge.Models.Entities;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;

// namespace AutoEdge.Controllers
// {
//     [Authorize]
//     public class ServiceBookingController : Controller
//     {
//         private readonly ApplicationDbContext _context;
//         private readonly UserManager<ApplicationUser> _userManager;
//         private readonly ILogger<ServiceBookingController> _logger;

//         public ServiceBookingController(
//             ApplicationDbContext context,
//             UserManager<ApplicationUser> userManager,
//             ILogger<ServiceBookingController> logger)
//         {
//             _context = context;
//             _userManager = userManager;
//             _logger = logger;
//         }

//         // GET: ServiceBooking
//         public async Task<IActionResult> Index()
//         {
//             var user = await _userManager.GetUserAsync(User);
//             if (user == null) return NotFound();

//             var bookings = await _context.ServiceBookings
//                 .Where(b => b.CustomerId == user.Id)
//                 .OrderByDescending(b => b.CreatedAt)
//                 .ToListAsync();

//             return View(bookings);
//         }

//         // GET: ServiceBooking/Create
//         public IActionResult Create()
//         {
//             return View();
//         }

//         // POST: ServiceBooking/Create
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> Create(ServiceBooking model)
//         {
//             try
//             {
//                 var user = await _userManager.GetUserAsync(User);
//                 if (user == null) return NotFound();

//                 if (ModelState.IsValid)
//                 {
//                     // Generate unique reference number
//                     model.ReferenceNumber = GenerateReferenceNumber();
//                     model.CustomerId = user.Id;
//                     model.BookingStatus = "Requested";
//                     model.CreatedAt = DateTime.UtcNow;
//                     model.UpdatedAt = DateTime.UtcNow;

//                     // Set default estimated costs based on service type
//                     SetEstimatedCosts(model);

//                     _context.ServiceBookings.Add(model);
//                     await _context.SaveChangesAsync();

//                     TempData["SuccessMessage"] = $"Service booking created successfully! Reference: {model.ReferenceNumber}";
//                     return RedirectToAction(nameof(Index));
//                 }
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error creating service booking");
//                 TempData["ErrorMessage"] = "Error creating service booking. Please try again.";
//             }

//             return View(model);
//         }

//         // GET: ServiceBooking/Details/5
//         public async Task<IActionResult> Details(int id)
//         {
//             var user = await _userManager.GetUserAsync(User);
//             if (user == null) return NotFound();

//             var booking = await _context.ServiceBookings
//                 .Include(b => b.ServiceSchedules)
//                 .Include(b => b.VehiclePickups)
//                 .Include(b => b.VehicleInspections)
//                 .Include(b => b.ServiceExecutions)
//                 .Include(b => b.ServiceInvoices)
//                 .Include(b => b.ServicePayments)
//                 .FirstOrDefaultAsync(b => b.ServiceBookingId == id && b.CustomerId == user.Id);

//             if (booking == null) return NotFound();

//             return View(booking);
//         }

//         // GET: ServiceBooking/RequestPickup/5
//         public async Task<IActionResult> RequestPickup(int id)
//         {
//             var user = await _userManager.GetUserAsync(User);
//             if (user == null) return NotFound();

//             var booking = await _context.ServiceBookings
//                 .FirstOrDefaultAsync(b => b.ServiceBookingId == id && b.CustomerId == user.Id);

//             if (booking == null) return NotFound();

//             return View(booking);
//         }

//         // POST: ServiceBooking/RequestPickup
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> RequestPickup(int serviceBookingId, string pickupLocation, DateTime pickupDate, TimeSpan pickupTime)
//         {
//             try
//             {
//                 var user = await _userManager.GetUserAsync(User);
//                 if (user == null) return NotFound();

//                 var booking = await _context.ServiceBookings
//                     .FirstOrDefaultAsync(b => b.ServiceBookingId == serviceBookingId && b.CustomerId == user.Id);

//                 if (booking == null) return NotFound();

//                 // Create pickup request
//                 var pickup = new VehiclePickup
//                 {
//                     ServiceBookingId = serviceBookingId,
//                     DriverId = "pending", // Will be assigned by service manager
//                     PickupLocation = pickupLocation,
//                     PickupDate = pickupDate,
//                     PickupTime = pickupTime,
//                     PickupStatus = "Scheduled",
//                     CreatedAt = DateTime.UtcNow,
//                     UpdatedAt = DateTime.UtcNow
//                 };

//                 _context.VehiclePickups.Add(pickup);
//                 await _context.SaveChangesAsync();

//                 TempData["SuccessMessage"] = "Vehicle pickup requested successfully!";
//                 return RedirectToAction(nameof(Details), new { id = serviceBookingId });
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error requesting vehicle pickup");
//                 TempData["ErrorMessage"] = "Error requesting pickup. Please try again.";
//                 return RedirectToAction(nameof(Details), new { id = serviceBookingId });
//             }
//         }

//         // GET: ServiceBooking/Payment/5
//         public async Task<IActionResult> Payment(int id)
//         {
//             var user = await _userManager.GetUserAsync(User);
//             if (user == null) return NotFound();

//             var booking = await _context.ServiceBookings
//                 .Include(b => b.ServiceInvoices)
//                 .FirstOrDefaultAsync(b => b.ServiceBookingId == id && b.CustomerId == user.Id);

//             if (booking == null) return NotFound();

//             var invoice = booking.ServiceInvoices.FirstOrDefault();
//             if (invoice == null)
//             {
//                 TempData["ErrorMessage"] = "No invoice found for this booking.";
//                 return RedirectToAction(nameof(Details), new { id });
//             }

//             return View(invoice);
//         }

//         // POST: ServiceBooking/ProcessPayment
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> ProcessPayment(int serviceInvoiceId, string paymentMethod, decimal amount)
//         {
//             try
//             {
//                 var user = await _userManager.GetUserAsync(User);
//                 if (user == null) return NotFound();

//                 var invoice = await _context.ServiceInvoices
//                     .Include(i => i.ServiceBooking)
//                     .FirstOrDefaultAsync(i => i.ServiceInvoiceId == serviceInvoiceId && i.ServiceBooking.CustomerId == user.Id);

//                 if (invoice == null) return NotFound();

//                 // Create payment record
//                 var payment = new ServicePayment
//                 {
//                     ServiceBookingId = invoice.ServiceBookingId,
//                     ServiceInvoiceId = serviceInvoiceId,
//                     CustomerId = user.Id,
//                     PaymentAmount = amount,
//                     PaymentMethod = paymentMethod,
//                     PaymentStatus = paymentMethod == "Cash" ? "Pending" : "Processing",
//                     PaymentDate = DateTime.UtcNow,
//                     CreatedAt = DateTime.UtcNow,
//                     UpdatedAt = DateTime.UtcNow
//                 };

//                 _context.ServicePayments.Add(payment);

//                 // Update invoice status
//                 invoice.InvoiceStatus = paymentMethod == "Cash" ? "Issued" : "Paid";
//                 invoice.UpdatedAt = DateTime.UtcNow;

//                 await _context.SaveChangesAsync();

//                 TempData["SuccessMessage"] = $"Payment processed successfully! Amount: ${amount:F2}";
//                 return RedirectToAction(nameof(Details), new { id = invoice.ServiceBookingId });
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error processing payment");
//                 TempData["ErrorMessage"] = "Error processing payment. Please try again.";
//                 return RedirectToAction(nameof(Payment), new { id = serviceInvoiceId });
//             }
//         }

//         private string GenerateReferenceNumber()
//         {
//             return "SB" + DateTime.Now.ToString("yyyyMMdd") + Guid.NewGuid().ToString("N")[..6].ToUpper();
//         }

//         private void SetEstimatedCosts(ServiceBooking booking)
//         {
//             switch (booking.ServiceType.ToLower())
//             {
//                 case "maintenance":
//                     booking.EstimatedCostMin = 50;
//                     booking.EstimatedCostMax = 200;
//                     booking.EstimatedDurationMinutes = 60;
//                     break;
//                 case "repairs":
//                     booking.EstimatedCostMin = 100;
//                     booking.EstimatedCostMax = 500;
//                     booking.EstimatedDurationMinutes = 120;
//                     break;
//                 case "inspection":
//                     booking.EstimatedCostMin = 30;
//                     booking.EstimatedCostMax = 100;
//                     booking.EstimatedDurationMinutes = 45;
//                     break;
//                 default:
//                     booking.EstimatedCostMin = 50;
//                     booking.EstimatedCostMax = 300;
//                     booking.EstimatedDurationMinutes = 90;
//                     break;
//             }
//         }
//     }
// }
