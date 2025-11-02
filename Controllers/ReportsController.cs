using AutoEdge.Data;
using AutoEdge.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace AutoEdge.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<ReportsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Reports
        public async Task<IActionResult> Index()
        {
            try
            {
                // Sales and Revenue Statistics
                var totalSales = await _context.Purchases
                    .Where(p => p.Status == "Completed")
                    .CountAsync();

                var totalRevenue = await _context.Purchases
                    .Where(p => p.Status == "Completed")
                    .SumAsync(p => p.PurchasePrice);

                var monthlySales = await _context.Purchases
                    .Where(p => p.Status == "Completed" && p.CreatedDate >= DateTime.UtcNow.AddMonths(-1))
                    .CountAsync();

                var monthlyRevenue = await _context.Purchases
                    .Where(p => p.Status == "Completed" && p.CreatedDate >= DateTime.UtcNow.AddMonths(-1))
                    .SumAsync(p => p.PurchasePrice);

                // User Statistics
                var totalUsers = await _userManager.Users.CountAsync();
                var activeUsers = await _userManager.Users
                    .Where(u => u.IsActive)
                    .CountAsync();

                var newUsersThisMonth = await _userManager.Users
                    .Where(u => u.CreatedDate >= DateTime.UtcNow.AddMonths(-1))
                    .CountAsync();

                // Vehicle Statistics
                var totalVehicles = await _context.Vehicles.CountAsync();
                var availableVehicles = await _context.Vehicles
                    .Where(v => v.Status == "Available")
                    .CountAsync();

                var soldVehicles = await _context.Purchases
                    .Where(p => p.Status == "Completed")
                    .CountAsync();

                // Delivery Statistics
                var totalDeliveries = await _context.Deliveries.CountAsync();
                var completedDeliveries = await _context.Deliveries
                    .Where(d => d.Status == "Delivered")
                    .CountAsync();

                var pendingDeliveries = await _context.Deliveries
                    .Where(d => d.Status == "Scheduled" || d.Status == "InTransit")
                    .CountAsync();

                // Inquiry Statistics
                var totalInquiries = await _context.Inquiries.CountAsync();
                var pendingInquiries = await _context.Inquiries
                    .Where(i => i.Status == "Pending")
                    .CountAsync();

                var respondedInquiries = await _context.Inquiries
                    .Where(i => i.Status == "Responded")
                    .CountAsync();

                // Recent Sales Data for Charts
                var recentSales = await _context.Purchases
                    .Where(p => p.Status == "Completed" && p.CreatedDate >= DateTime.UtcNow.AddDays(-30))
                    .GroupBy(p => p.CreatedDate.Date)
                    .Select(g => new { Date = g.Key, Count = g.Count(), Revenue = g.Sum(p => p.PurchasePrice) })
                    .OrderBy(x => x.Date)
                    .ToListAsync();

                // Top Selling Vehicles
                var topVehicles = await _context.Purchases
                    .Where(p => p.Status == "Completed")
                    .Include(p => p.Vehicle)
                    .GroupBy(p => new { p.Vehicle.Make, p.Vehicle.Model })
                    .Select(g => new { 
                        Vehicle = g.Key.Make + " " + g.Key.Model, 
                        Count = g.Count(),
                        Revenue = g.Sum(p => p.PurchasePrice)
                    })
                    .OrderByDescending(x => x.Count)
                    .Take(5)
                    .ToListAsync();

                // Pass data to view
                ViewBag.TotalSales = totalSales;
                ViewBag.TotalRevenue = totalRevenue;
                ViewBag.MonthlySales = monthlySales;
                ViewBag.MonthlyRevenue = monthlyRevenue;
                ViewBag.TotalUsers = totalUsers;
                ViewBag.ActiveUsers = activeUsers;
                ViewBag.NewUsersThisMonth = newUsersThisMonth;
                ViewBag.TotalVehicles = totalVehicles;
                ViewBag.AvailableVehicles = availableVehicles;
                ViewBag.SoldVehicles = soldVehicles;
                ViewBag.TotalDeliveries = totalDeliveries;
                ViewBag.CompletedDeliveries = completedDeliveries;
                ViewBag.PendingDeliveries = pendingDeliveries;
                ViewBag.TotalInquiries = totalInquiries;
                ViewBag.PendingInquiries = pendingInquiries;
                ViewBag.RespondedInquiries = respondedInquiries;
                ViewBag.RecentSales = recentSales;
                ViewBag.TopVehicles = topVehicles;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reports dashboard");
                TempData["Error"] = "An error occurred while loading the reports. Please try again.";
                return RedirectToAction("Index", "Admin");
            }
        }

        // GET: Reports/Sales
        public async Task<IActionResult> Sales(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddMonths(-3);
                var end = endDate ?? DateTime.UtcNow;

                var salesData = await _context.Purchases
                    .Where(p => p.Status == "Completed" && p.CreatedDate >= start && p.CreatedDate <= end)
                    .Include(p => p.Vehicle)
                    .Include(p => p.Customer)
                        .ThenInclude(c => c.User)
                    .OrderByDescending(p => p.CreatedDate)
                    .ToListAsync();

                ViewBag.StartDate = start;
                ViewBag.EndDate = end;
                ViewBag.TotalSales = salesData.Count;
                ViewBag.TotalRevenue = salesData.Sum(p => p.PurchasePrice);
                ViewBag.AverageOrderValue = salesData.Any() ? salesData.Average(p => p.PurchasePrice) : 0;

                return View(salesData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sales report");
                TempData["Error"] = "An error occurred while loading the sales report. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // GET: Reports/Users
        public async Task<IActionResult> Users()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                var userRoles = new Dictionary<string, IList<string>>();
                var userStats = new Dictionary<string, object>();

                foreach (var user in users)
                {
                    userRoles[user.Id] = await _userManager.GetRolesAsync(user);
                }

                // User registration trends
                var registrationTrends = users
                    .GroupBy(u => u.CreatedDate.Date)
                    .Select(g => new { Date = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Date)
                    .TakeLast(30)
                    .ToList();

                // Role distribution
                var roleDistribution = new Dictionary<string, int>();
                foreach (var role in userRoles.Values.SelectMany(r => r).Distinct())
                {
                    roleDistribution[role] = userRoles.Values.Count(r => r.Contains(role));
                }

                ViewBag.UserRoles = userRoles;
                ViewBag.RegistrationTrends = registrationTrends;
                ViewBag.RoleDistribution = roleDistribution;
                ViewBag.TotalUsers = users.Count;
                ViewBag.ActiveUsers = users.Count(u => u.IsActive);
                ViewBag.InactiveUsers = users.Count(u => !u.IsActive);

                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users report");
                TempData["Error"] = "An error occurred while loading the users report. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // GET: Reports/Deliveries
        public async Task<IActionResult> Deliveries()
        {
            try
            {
                var deliveries = await _context.Deliveries
                    .Include(d => d.Contract)
                        .ThenInclude(c => c.Purchase)
                            .ThenInclude(p => p.Vehicle)
                    .Include(d => d.Contract)
                        .ThenInclude(c => c.Purchase)
                            .ThenInclude(p => p.Customer)
                                .ThenInclude(c => c.User)
                    .OrderByDescending(d => d.CreatedDate)
                    .ToListAsync();

                // Delivery status distribution
                var statusDistribution = deliveries
                    .GroupBy(d => d.Status)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Average delivery time
                var completedDeliveries = deliveries.Where(d => d.Status == "Delivered" && d.CompletedDate.HasValue);
                var averageDeliveryTime = completedDeliveries.Any() 
                    ? completedDeliveries.Average(d => (d.CompletedDate.Value - d.CreatedDate).TotalDays)
                    : 0;

                // Driver performance
                var driverPerformance = deliveries
                    .Where(d => d.DriverUserId != null)
                    .GroupBy(d => d.DriverUserId)
                    .Select(g => new {
                        Driver = g.Key,
                        TotalDeliveries = g.Count(),
                        CompletedDeliveries = g.Count(d => d.Status == "Delivered"),
                        SuccessRate = g.Any() ? (double)g.Count(d => d.Status == "Delivered") / g.Count() * 100 : 0
                    })
                    .OrderByDescending(x => x.TotalDeliveries)
                    .ToList();

                ViewBag.StatusDistribution = statusDistribution;
                ViewBag.AverageDeliveryTime = Math.Round((double)averageDeliveryTime, 1);
                ViewBag.DriverPerformance = driverPerformance;
                ViewBag.TotalDeliveries = deliveries.Count;
                ViewBag.CompletedDeliveries = deliveries.Count(d => d.Status == "Delivered");
                ViewBag.PendingDeliveries = deliveries.Count(d => d.Status == "Scheduled" || d.Status == "InTransit");

                return View(deliveries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading deliveries report");
                TempData["Error"] = "An error occurred while loading the deliveries report. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // GET: Reports/Vehicles
        public async Task<IActionResult> Vehicles()
        {
            try
            {
                var vehicles = await _context.Vehicles
                    .Include(v => v.VehicleImages)
                    .ToListAsync();

                var purchases = await _context.Purchases
                    .Where(p => p.Status == "Completed")
                    .Include(p => p.Vehicle)
                    .ToListAsync();

                // Vehicle performance metrics
                var vehiclePerformance = vehicles.Select(v => new {
                    Vehicle = v,
                    TimesSold = purchases.Count(p => p.VehicleId == v.Id),
                    Revenue = purchases.Where(p => p.VehicleId == v.Id).Sum(p => p.PurchasePrice),
                    DaysOnLot = v.Status == "Available" ? (DateTime.UtcNow - v.CreatedDate).Days : 0
                }).ToList();

                // Popular makes and models
                var popularMakes = vehicles
                    .GroupBy(v => v.Make)
                    .Select(g => new { Make = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToList();

                ViewBag.VehiclePerformance = vehiclePerformance;
                ViewBag.PopularMakes = popularMakes;
                ViewBag.TotalVehicles = vehicles.Count;
                ViewBag.AvailableVehicles = vehicles.Count(v => v.Status == "Available");
                ViewBag.SoldVehicles = purchases.Count;
                ViewBag.AveragePrice = vehicles.Any() ? vehicles.Average(v => v.SellingPrice) : 0;

                return View(vehicles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vehicles report");
                TempData["Error"] = "An error occurred while loading the vehicles report. Please try again.";
                return RedirectToAction("Index");
            }
        }
    }
}