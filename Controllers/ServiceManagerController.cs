// using AutoEdge.Data;
// using AutoEdge.Models.Entities;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;

// namespace AutoEdge.Controllers
// {
//     [Authorize(Roles = "Administrator,ServiceManager")]
//     public class ServiceManagerController : Controller
//     {
//         private readonly ApplicationDbContext _context;
//         private readonly UserManager<ApplicationUser> _userManager;
//         private readonly ILogger<ServiceManagerController> _logger;

//         public ServiceManagerController(
//             ApplicationDbContext context,
//             UserManager<ApplicationUser> userManager,
//             ILogger<ServiceManagerController> logger)
//         {
//             _context = context;
//             _userManager = userManager;
//             _logger = logger;
//         }

//         // GET: ServiceManager
//         public async Task<IActionResult> Index()
//         {
//             var bookings = await _context.ServiceBookings
//                 .Include(b => b.Customer)
//                 .Include(b => b.ServiceSchedules)
//                 .OrderByDescending(b => b.CreatedAt)
//                 .ToListAsync();

//             return View(bookings);
//         }

//         // GET: ServiceManager/Schedule
//         public async Task<IActionResult> Schedule()
//         {
//             var schedules = await _context.ServiceSchedules
//                 .Include(s => s.ServiceBooking)
//                 .ThenInclude(b => b.Customer)
//                 .Include(s => s.Mechanic)
//                 .OrderBy(s => s.ScheduledDate)
//                 .ThenBy(s => s.ScheduledTime)
//                 .ToListAsync();

//             return View(schedules);
//         }

//         // GET: ServiceManager/ScheduleBooking/5
//         public async Task<IActionResult> ScheduleBooking(int id)
//         {
//             var booking = await _context.ServiceBookings
//                 .Include(b => b.Customer)
//                 .FirstOrDefaultAsync(b => b.ServiceBookingId == id);

//             if (booking == null) return NotFound();

//             // Get available mechanics
//             var mechanics = await _userManager.GetUsersInRoleAsync("Mechanic");
//             ViewBag.Mechanics = mechanics;

//             return View(booking);
//         }

//         // POST: ServiceManager/ScheduleBooking
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> ScheduleBooking(int serviceBookingId, string mechanicId, DateTime scheduledDate, TimeSpan scheduledTime, string serviceBay)
//         {
//             try
//             {
//                 var booking = await _context.ServiceBookings
//                     .FirstOrDefaultAsync(b => b.ServiceBookingId == serviceBookingId);

//                 if (booking == null) return NotFound();

//                 // Check for scheduling conflicts
//                 var conflict = await _context.ServiceSchedules
//                     .AnyAsync(s => s.MechanicId == mechanicId && 
//                                   s.ScheduledDate.Date == scheduledDate.Date && 
//                                   s.ScheduledTime == scheduledTime &&
//                                   s.ScheduleStatus != "Cancelled");

//                 if (conflict)
//                 {
//                     TempData["ErrorMessage"] = "Selected time slot is not available for this mechanic.";
//                     return RedirectToAction(nameof(ScheduleBooking), new { id = serviceBookingId });
//                 }

//                 // Create schedule
//                 var schedule = new ServiceSchedule
//                 {
//                     ServiceBookingId = serviceBookingId,
//                     MechanicId = mechanicId,
//                     ScheduledDate = scheduledDate,
//                     ScheduledTime = scheduledTime,
//                     ServiceBay = serviceBay,
//                     EstimatedCompletion = scheduledDate.AddMinutes(booking.EstimatedDurationMinutes),
//                     ScheduleStatus = "Scheduled",
//                     CreatedAt = DateTime.UtcNow,
//                     UpdatedAt = DateTime.UtcNow
//                 };

//                 _context.ServiceSchedules.Add(schedule);

//                 // Update booking status
//                 booking.BookingStatus = "Scheduled";
//                 booking.UpdatedAt = DateTime.UtcNow;

//                 await _context.SaveChangesAsync();

//                 TempData["SuccessMessage"] = "Service scheduled successfully!";
//                 return RedirectToAction(nameof(Schedule));
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error scheduling service");
//                 TempData["ErrorMessage"] = "Error scheduling service. Please try again.";
//                 return RedirectToAction(nameof(ScheduleBooking), new { id = serviceBookingId });
//             }
//         }

//         // GET: ServiceManager/AssignPickup/5
//         public async Task<IActionResult> AssignPickup(int id)
//         {
//             var pickup = await _context.VehiclePickups
//                 .Include(p => p.ServiceBooking)
//                 .ThenInclude(b => b.Customer)
//                 .FirstOrDefaultAsync(p => p.VehiclePickupId == id);

//             if (pickup == null) return NotFound();

//             // Get available drivers
//             var drivers = await _userManager.GetUsersInRoleAsync("Driver");
//             ViewBag.Drivers = drivers;

//             return View(pickup);
//         }

//         // POST: ServiceManager/AssignPickup
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> AssignPickup(int vehiclePickupId, string driverId)
//         {
//             try
//             {
//                 var pickup = await _context.VehiclePickups
//                     .FirstOrDefaultAsync(p => p.VehiclePickupId == vehiclePickupId);

//                 if (pickup == null) return NotFound();

//                 pickup.DriverId = driverId;
//                 pickup.UpdatedAt = DateTime.UtcNow;

//                 await _context.SaveChangesAsync();

//                 TempData["SuccessMessage"] = "Driver assigned to pickup successfully!";
//                 return RedirectToAction(nameof(PickupRequests));
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error assigning pickup driver");
//                 TempData["ErrorMessage"] = "Error assigning driver. Please try again.";
//                 return RedirectToAction(nameof(AssignPickup), new { id = vehiclePickupId });
//             }
//         }

//         // GET: ServiceManager/PickupRequests
//         public async Task<IActionResult> PickupRequests()
//         {
//             var pickups = await _context.VehiclePickups
//                 .Include(p => p.ServiceBooking)
//                 .ThenInclude(b => b.Customer)
//                 .Include(p => p.Driver)
//                 .OrderBy(p => p.PickupDate)
//                 .ToListAsync();

//             return View(pickups);
//         }

//         // GET: ServiceManager/Inspections
//         public async Task<IActionResult> Inspections()
//         {
//             var inspections = await _context.VehicleInspections
//                 .Include(i => i.ServiceBooking)
//                 .ThenInclude(b => b.Customer)
//                 .Include(i => i.Inspector)
//                 .OrderByDescending(i => i.CheckInTime)
//                 .ToListAsync();

//             return View(inspections);
//         }

//         // GET: ServiceManager/StartInspection/5
//         public async Task<IActionResult> StartInspection(int serviceBookingId)
//         {
//             var booking = await _context.ServiceBookings
//                 .Include(b => b.Customer)
//                 .FirstOrDefaultAsync(b => b.ServiceBookingId == serviceBookingId);

//             if (booking == null) return NotFound();

//             var user = await _userManager.GetUserAsync(User);
//             if (user == null) return NotFound();

//             // Create inspection record
//             var inspection = new VehicleInspection
//             {
//                 ServiceBookingId = serviceBookingId,
//                 InspectorId = user.Id,
//                 CheckInTime = DateTime.UtcNow,
//                 InspectionStatus = "InProgress",
//                 CreatedAt = DateTime.UtcNow,
//                 UpdatedAt = DateTime.UtcNow
//             };

//             _context.VehicleInspections.Add(inspection);
//             await _context.SaveChangesAsync();

//             return RedirectToAction(nameof(InspectionDetails), new { id = inspection.VehicleInspectionId });
//         }

//         // GET: ServiceManager/InspectionDetails/5
//         public async Task<IActionResult> InspectionDetails(int id)
//         {
//             var inspection = await _context.VehicleInspections
//                 .Include(i => i.ServiceBooking)
//                 .ThenInclude(b => b.Customer)
//                 .Include(i => i.Inspector)
//                 .FirstOrDefaultAsync(i => i.VehicleInspectionId == id);

//             if (inspection == null) return NotFound();

//             return View(inspection);
//         }

//         // POST: ServiceManager/CompleteInspection
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> CompleteInspection(int vehicleInspectionId, VehicleInspection model)
//         {
//             try
//             {
//                 var inspection = await _context.VehicleInspections
//                     .FirstOrDefaultAsync(i => i.VehicleInspectionId == vehicleInspectionId);

//                 if (inspection == null) return NotFound();

//                 // Update inspection details
//                 inspection.OdometerReading = model.OdometerReading;
//                 inspection.ExteriorCondition = model.ExteriorCondition;
//                 inspection.InteriorCondition = model.InteriorCondition;
//                 inspection.DamageDocumentation = model.DamageDocumentation;
//                 inspection.FluidLevels = model.FluidLevels;
//                 inspection.FuelLevel = model.FuelLevel;
//                 inspection.RequiredMaintenance = model.RequiredMaintenance;
//                 inspection.InspectionPhotos = model.InspectionPhotos;
//                 inspection.InspectionStatus = "Completed";
//                 inspection.UpdatedAt = DateTime.UtcNow;

//                 await _context.SaveChangesAsync();

//                 TempData["SuccessMessage"] = "Vehicle inspection completed successfully!";
//                 return RedirectToAction(nameof(Inspections));
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error completing inspection");
//                 TempData["ErrorMessage"] = "Error completing inspection. Please try again.";
//                 return RedirectToAction(nameof(InspectionDetails), new { id = vehicleInspectionId });
//             }
//         }

//         // GET: ServiceManager/Executions
//         public async Task<IActionResult> Executions()
//         {
//             var executions = await _context.ServiceExecutions
//                 .Include(e => e.ServiceBooking)
//                 .ThenInclude(b => b.Customer)
//                 .Include(e => e.Technician)
//                 .OrderByDescending(e => e.CreatedAt)
//                 .ToListAsync();

//             return View(executions);
//         }

//         // GET: ServiceManager/StartExecution/5
//         public async Task<IActionResult> StartExecution(int serviceBookingId)
//         {
//             var booking = await _context.ServiceBookings
//                 .Include(b => b.Customer)
//                 .FirstOrDefaultAsync(b => b.ServiceBookingId == serviceBookingId);

//             if (booking == null) return NotFound();

//             var user = await _userManager.GetUserAsync(User);
//             if (user == null) return NotFound();

//             // Create execution record
//             var execution = new ServiceExecution
//             {
//                 ServiceBookingId = serviceBookingId,
//                 TechnicianId = user.Id,
//                 StartTime = DateTime.UtcNow,
//                 ExecutionStatus = "InProgress",
//                 CreatedAt = DateTime.UtcNow,
//                 UpdatedAt = DateTime.UtcNow
//             };

//             _context.ServiceExecutions.Add(execution);
//             await _context.SaveChangesAsync();

//             return RedirectToAction(nameof(ExecutionDetails), new { id = execution.ServiceExecutionId });
//         }

//         // GET: ServiceManager/ExecutionDetails/5
//         public async Task<IActionResult> ExecutionDetails(int id)
//         {
//             var execution = await _context.ServiceExecutions
//                 .Include(e => e.ServiceBooking)
//                 .ThenInclude(b => b.Customer)
//                 .Include(e => e.Technician)
//                 .FirstOrDefaultAsync(e => e.ServiceExecutionId == id);

//             if (execution == null) return NotFound();

//             return View(execution);
//         }

//         // POST: ServiceManager/CompleteExecution
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> CompleteExecution(int serviceExecutionId, ServiceExecution model)
//         {
//             try
//             {
//                 var execution = await _context.ServiceExecutions
//                     .FirstOrDefaultAsync(e => e.ServiceExecutionId == serviceExecutionId);

//                 if (execution == null) return NotFound();

//                 // Update execution details
//                 execution.EndTime = DateTime.UtcNow;
//                 execution.TasksCompleted = model.TasksCompleted;
//                 execution.PartsUsed = model.PartsUsed;
//                 execution.LaborHours = model.LaborHours;
//                 execution.LaborRate = model.LaborRate;
//                 execution.AdditionalIssues = model.AdditionalIssues;
//                 execution.AdditionalWorkApproved = model.AdditionalWorkApproved;
//                 execution.QualityCheckPassed = model.QualityCheckPassed;
//                 execution.TestDriveNotes = model.TestDriveNotes;
//                 execution.TotalCost = model.TotalCost;
//                 execution.ExecutionStatus = "Completed";
//                 execution.UpdatedAt = DateTime.UtcNow;

//                 await _context.SaveChangesAsync();

//                 TempData["SuccessMessage"] = "Service execution completed successfully!";
//                 return RedirectToAction(nameof(Executions));
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error completing execution");
//                 TempData["ErrorMessage"] = "Error completing execution. Please try again.";
//                 return RedirectToAction(nameof(ExecutionDetails), new { id = serviceExecutionId });
//             }
//         }

//         // POST: ServiceManager/ApproveExecution
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> ApproveExecution(int serviceExecutionId)
//         {
//             try
//             {
//                 var execution = await _context.ServiceExecutions
//                     .FirstOrDefaultAsync(e => e.ServiceExecutionId == serviceExecutionId);

//                 if (execution == null) return NotFound();

//                 var user = await _userManager.GetUserAsync(User);
//                 if (user == null) return NotFound();

//                 execution.AdvisorApproval = true;
//                 execution.ApprovedBy = user.Id;
//                 execution.ApprovedAt = DateTime.UtcNow;
//                 execution.ExecutionStatus = "ApprovedForBilling";
//                 execution.UpdatedAt = DateTime.UtcNow;

//                 // Create invoice
//                 var invoice = new ServiceInvoice
//                 {
//                     ServiceBookingId = execution.ServiceBookingId,
//                     ServiceExecutionId = serviceExecutionId,
//                     InvoiceNumber = GenerateInvoiceNumber(),
//                     PartsCost = 0, // Will be calculated from PartsUsed JSON
//                     LaborCost = execution.LaborHours * execution.LaborRate,
//                     AdditionalFees = 0,
//                     Subtotal = execution.TotalCost,
//                     TaxAmount = execution.TotalCost * 0.1m, // 10% tax
//                     TotalAmount = execution.TotalCost * 1.1m,
//                     InvoiceDate = DateTime.UtcNow,
//                     InvoiceStatus = "Issued",
//                     CreatedAt = DateTime.UtcNow,
//                     UpdatedAt = DateTime.UtcNow
//                 };

//                 _context.ServiceInvoices.Add(invoice);
//                 await _context.SaveChangesAsync();

//                 TempData["SuccessMessage"] = "Service execution approved and invoice generated!";
//                 return RedirectToAction(nameof(Executions));
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error approving execution");
//                 TempData["ErrorMessage"] = "Error approving execution. Please try again.";
//                 return RedirectToAction(nameof(Executions));
//             }
//         }

//         private string GenerateInvoiceNumber()
//         {
//             return "INV" + DateTime.Now.ToString("yyyyMMdd") + Guid.NewGuid().ToString("N")[..6].ToUpper();
//         }
//     }
// }
