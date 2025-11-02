using AutoEdge.Models.Entities;
using AutoEdge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AutoEdge.Controllers
{
    [Authorize(Roles = "Customer")]
    public class BookingsController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingsController(IBookingService bookingService, UserManager<ApplicationUser> userManager)
        {
            _bookingService = bookingService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var request = new CreateBookingRequest
            {
                CustomerId = currentUser.Id,
                CustomerName = $"{currentUser.FirstName} {currentUser.LastName}".Trim(),
                CustomerEmail = currentUser.Email ?? string.Empty,
                CustomerPhone = currentUser.PhoneNumber
            };

            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] CreateBookingRequest request)
        {
            // Ensure customer information cannot be tampered with
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            // Override customer information with authenticated user data
            request.CustomerId = currentUser.Id;
            request.CustomerName = $"{currentUser.FirstName} {currentUser.LastName}".Trim();
            request.CustomerEmail = currentUser.Email ?? string.Empty;
            request.CustomerPhone = currentUser.PhoneNumber;

            if (!ModelState.IsValid)
            {
                return View(request);
            }
            try
            {
                var booking = await _bookingService.CreateBookingAsync(request);
                return RedirectToAction("Confirm", new { id = booking.Id });
            }
            catch (InvalidOperationException ex) when (ex.Message.StartsWith("A booking already exists"))
            {
                ModelState.AddModelError(string.Empty,
                    "A booking for this vehicle on that date already exists. Pick another time or view your existing booking.");
                return View(request);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("business hours"))
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(request);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("future dates"))
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(request);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("No technicians"))
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(request);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Confirm(int id)
        {
            var booking = await _bookingService.GetBookingWithJobAsync(id);
            if (booking == null) return NotFound();
            return View(booking);
        }
    }
}