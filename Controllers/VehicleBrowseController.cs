using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoEdge.Data;
using AutoEdge.Models;
using AutoEdge.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace AutoEdge.Controllers
{
    public class VehicleBrowseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public VehicleBrowseController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: VehicleBrowse
        public async Task<IActionResult> Index(string searchTerm, string make, string condition, 
            decimal? minPrice, decimal? maxPrice, int? minYear, int? maxYear, 
            string engineType, string transmission, string bodyType, string fuelType, string sortBy = "price_asc", int page = 1)
        {
            var vehicles = _context.Vehicles
                .Include(v => v.VehicleImages)
                .Where(v => v.Status == "Available")
                .AsQueryable();

            // Apply search filters with proper null handling
            if (!string.IsNullOrEmpty(searchTerm))
            {
                vehicles = vehicles.Where(v => (v.Make != null && EF.Functions.Like(v.Make, $"%{searchTerm}%")) || 
                                             (v.Model != null && EF.Functions.Like(v.Model, $"%{searchTerm}%")) ||
                                             (v.Features != null && EF.Functions.Like(v.Features, $"%{searchTerm}%")));
            }

            if (!string.IsNullOrEmpty(make))
            {
                vehicles = vehicles.Where(v => v.Make != null && v.Make == make);
            }

            if (!string.IsNullOrEmpty(condition))
            {
                vehicles = vehicles.Where(v => v.Condition != null && v.Condition == condition);
            }

            if (minPrice.HasValue)
            {
                vehicles = vehicles.Where(v => v.SellingPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                vehicles = vehicles.Where(v => v.SellingPrice <= maxPrice.Value);
            }

            if (minYear.HasValue)
            {
                vehicles = vehicles.Where(v => v.Year >= minYear.Value);
            }

            if (maxYear.HasValue)
            {
                vehicles = vehicles.Where(v => v.Year <= maxYear.Value);
            }

            if (!string.IsNullOrEmpty(engineType))
            {
                vehicles = vehicles.Where(v => v.EngineType != null && v.EngineType == engineType);
            }

            if (!string.IsNullOrEmpty(transmission))
            {
                vehicles = vehicles.Where(v => v.Transmission != null && v.Transmission == transmission);
            }

            if (!string.IsNullOrEmpty(bodyType))
            {
                vehicles = vehicles.Where(v => v.EngineType != null && v.EngineType == bodyType); // Using EngineType as body type for now
            }

            if (!string.IsNullOrEmpty(fuelType))
            {
                vehicles = vehicles.Where(v => v.EngineType != null && v.EngineType == fuelType); // Using EngineType as fuel type for now
            }

            // Apply sorting
            vehicles = sortBy switch
            {
                "price_desc" => vehicles.OrderByDescending(v => v.SellingPrice),
                "year_asc" => vehicles.OrderBy(v => v.Year),
                "year_desc" => vehicles.OrderByDescending(v => v.Year),
                "mileage_asc" => vehicles.OrderBy(v => v.Mileage),
                "mileage_desc" => vehicles.OrderByDescending(v => v.Mileage),
                "make_asc" => vehicles.OrderBy(v => v.Make ?? "").ThenBy(v => v.Model ?? ""),
                _ => vehicles.OrderBy(v => v.SellingPrice)
            };

            // Pagination
            int pageSize = 12;
            var totalCount = await vehicles.CountAsync();
            var vehicleList = await vehicles
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get filter options for dropdowns
            ViewBag.Makes = await _context.Vehicles
                .Where(v => v.Status == "Available" && v.Make != null)
                .Select(v => v.Make)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();

            ViewBag.Conditions = await _context.Vehicles
                .Where(v => v.Status == "Available" && v.Condition != null)
                .Select(v => v.Condition)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            ViewBag.EngineTypes = await _context.Vehicles
                .Where(v => v.Status == "Available" && v.EngineType != null)
                .Select(v => v.EngineType)
                .Distinct()
                .OrderBy(e => e)
                .ToListAsync();

            ViewBag.Transmissions = await _context.Vehicles
                .Where(v => v.Status == "Available" && v.Transmission != null)
                .Select(v => v.Transmission)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            ViewBag.BodyTypes = await _context.Vehicles
                .Where(v => v.Status == "Available" && v.EngineType != null)
                .Select(v => v.EngineType) // Using EngineType as body type for now
                .Distinct()
                .OrderBy(b => b)
                .ToListAsync();

            ViewBag.FuelTypes = await _context.Vehicles
                .Where(v => v.Status == "Available" && v.EngineType != null)
                .Select(v => v.EngineType) // Using EngineType as fuel type for now
                .Distinct()
                .OrderBy(f => f)
                .ToListAsync();

            // Pagination info
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.TotalCount = totalCount;
            ViewBag.HasPreviousPage = page > 1;
            ViewBag.HasNextPage = page < ViewBag.TotalPages;

            // Current filter values
            ViewBag.CurrentSearchTerm = searchTerm;
            ViewBag.CurrentMake = make;
            ViewBag.CurrentCondition = condition;
            ViewBag.CurrentMinPrice = minPrice;
            ViewBag.CurrentMaxPrice = maxPrice;
            ViewBag.CurrentMinYear = minYear;
            ViewBag.CurrentMaxYear = maxYear;
            ViewBag.CurrentEngineType = engineType;
            ViewBag.CurrentTransmission = transmission;
            ViewBag.CurrentBodyType = bodyType;
            ViewBag.CurrentFuelType = fuelType;
            ViewBag.CurrentSortBy = sortBy;

            return View(vehicleList);
        }

        // GET: VehicleBrowse/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicles
                .Include(v => v.VehicleImages)
                .FirstOrDefaultAsync(m => m.Id == id && m.Status == "Available");

            if (vehicle == null)
            {
                return NotFound();
            }

            // Get similar vehicles (same make, different model or similar price range)
            var similarVehicles = await _context.Vehicles
                .Include(v => v.VehicleImages)
                .Where(v => v.Id != id && 
                           v.Status == "Available" &&
                           ((v.Make != null && vehicle.Make != null && v.Make == vehicle.Make) || 
                            (v.SellingPrice >= vehicle.SellingPrice * 0.8m && v.SellingPrice <= vehicle.SellingPrice * 1.2m)))
                .Take(4)
                .ToListAsync();

            ViewBag.SimilarVehicles = similarVehicles;

            return View(vehicle);
        }

        // POST: VehicleBrowse/Inquire
        [HttpPost]
        public async Task<IActionResult> Inquire([FromBody] InquiryRequest request)
        {
            var vehicle = await _context.Vehicles.FindAsync(request.VehicleId);
            if (vehicle == null || vehicle.Status != "Available")
            {
                return Json(new { success = false, message = "Vehicle not found or not available." });
            }

            // For guest users, we'll use a default customer ID or create a guest customer
            var guestCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.User.Email == request.CustomerEmail);
            if (guestCustomer == null)
            {
                var existingUser = await _userManager.FindByEmailAsync(request.CustomerEmail);
                ApplicationUser guestUser;
                
                if (existingUser == null)
                {
                    guestUser = new ApplicationUser
                    {
                        UserName = request.CustomerEmail,
                        Email = request.CustomerEmail,
                        FirstName = request.CustomerName.Split(' ').FirstOrDefault() ?? "",
                        LastName = request.CustomerName.Split(' ').Skip(1).FirstOrDefault() ?? "",
                        PhoneNumber = request.CustomerPhone,
                        EmailConfirmed = false,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    };
                    
                    var result = await _userManager.CreateAsync(guestUser);
                    if (!result.Succeeded)
                    {
                        return Json(new { success = false, message = "Failed to create user account." });
                    }
                }
                else
                {
                    guestUser = existingUser;
                }

                guestCustomer = new Customer
                {
                    UserId = guestUser.Id,
                    CustomerStatus = "Guest",
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Customers.Add(guestCustomer);
                await _context.SaveChangesAsync();
            }

            var inquiry = new Inquiry
            {
                VehicleId = request.VehicleId,
                CustomerId = guestCustomer.Id,
                InquiryType = request.InquiryType ?? "Information",
                Message = request.Message,
                Status = "Open",
                Priority = "Medium",
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                CustomerPhone = request.CustomerPhone,
                PreferredContactMethod = request.PreferredContactMethod ?? "Email",
                PreferredContactTime = request.PreferredContactTime,
                SpecialRequests = request.SpecialRequests,
                IsTestDriveRequested = request.IsTestDriveRequested,
                IsFinancingInquiry = request.IsFinancingInquiry,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                IsActive = true
            };

            _context.Inquiries.Add(inquiry);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Your inquiry has been submitted successfully!" });
        }

        public class InquiryRequest
        {
            public int VehicleId { get; set; }
            public string CustomerName { get; set; } = string.Empty;
            public string CustomerEmail { get; set; } = string.Empty;
            public string CustomerPhone { get; set; } = string.Empty;
            public string PreferredContactMethod { get; set; } = string.Empty;
            public DateTime? PreferredContactTime { get; set; }
            public string InquiryType { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public string SpecialRequests { get; set; } = string.Empty;
            public bool IsTestDriveRequested { get; set; }
            public bool IsFinancingInquiry { get; set; }
            public bool RequestBrochure { get; set; }
        }

        // POST: VehicleBrowse/Reserve
        [HttpPost]
        public async Task<IActionResult> Reserve([FromBody] ReservationRequest request)
        {
            var vehicle = await _context.Vehicles.FindAsync(request.VehicleId);
            if (vehicle == null || vehicle.Status != "Available")
            {
                return Json(new { success = false, message = "Vehicle not found or not available for reservation." });
            }

            // For guest users, we'll use a default customer ID or create a guest customer
            var guestCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.User.Email == request.CustomerEmail);
            if (guestCustomer == null)
            {
                var existingUser = await _userManager.FindByEmailAsync(request.CustomerEmail);
                ApplicationUser guestUser;
                
                if (existingUser == null)
                {
                    guestUser = new ApplicationUser
                    {
                        UserName = request.CustomerEmail,
                        Email = request.CustomerEmail,
                        FirstName = request.CustomerName.Split(' ').FirstOrDefault() ?? "",
                        LastName = request.CustomerName.Split(' ').Skip(1).FirstOrDefault() ?? "",
                        PhoneNumber = request.CustomerPhone,
                        EmailConfirmed = false,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    };
                    
                    var result = await _userManager.CreateAsync(guestUser);
                    if (!result.Succeeded)
                    {
                        return Json(new { success = false, message = "Failed to create user account." });
                    }
                }
                else
                {
                    guestUser = existingUser;
                }

                guestCustomer = new Customer
                {
                    UserId = guestUser.Id,
                    CustomerStatus = "Guest",
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Customers.Add(guestCustomer);
                await _context.SaveChangesAsync();
            }

            // Check if user already has a reservation for this vehicle
            var existingReservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.VehicleId == request.VehicleId && 
                                         r.CustomerId == guestCustomer.Id &&
                                         r.Status == "Active");

            if (existingReservation != null)
            {
                return Json(new { success = false, message = "You already have an active reservation for this vehicle." });
            }

            // Generate reservation number
            var reservationNumber = "RES" + DateTime.Now.ToString("yyyyMMddHHmmss");
            
            var reservation = new Reservation
            {
                VehicleId = request.VehicleId,
                CustomerId = guestCustomer.Id,
                ReservationNumber = reservationNumber,
                ReservationDate = DateTime.Now,
                ExpiryDate = DateTime.Now.AddDays(request.ReservationDuration),
                DepositAmount = request.DepositAmount,
                Status = "Active",
                PaymentMethod = request.PaymentMethod,
                PaymentStatus = "Pending", // In real implementation, process payment here
                Notes = request.Notes,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                IsActive = true
            };

            _context.Reservations.Add(reservation);
            
            // Update vehicle status
            vehicle.Status = "Reserved";
            
            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                message = $"Vehicle reserved successfully! Your reservation number is {reservationNumber}. Valid until {reservation.ExpiryDate.ToShortDateString()}.",
                reservationNumber = reservationNumber,
                expiryDate = reservation.ExpiryDate.ToShortDateString()
            });
        }

        public class ReservationRequest
        {
            public int VehicleId { get; set; }
            public string CustomerName { get; set; } = string.Empty;
            public string CustomerEmail { get; set; } = string.Empty;
            public string CustomerPhone { get; set; } = string.Empty;
            public int ReservationDuration { get; set; } = 7;
            public decimal DepositAmount { get; set; }
            public string PaymentMethod { get; set; } = string.Empty;
            public string Notes { get; set; } = string.Empty;
            public string CardNumber { get; set; } = string.Empty;
            public string Cvv { get; set; } = string.Empty;
            public string ExpiryMonth { get; set; } = string.Empty;
            public string ExpiryYear { get; set; } = string.Empty;
            public string CardholderName { get; set; } = string.Empty;
        }
    }
}