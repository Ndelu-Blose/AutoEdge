using AutoEdge.Data;
using AutoEdge.Models.Entities;
using AutoEdge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoEdge.Controllers
{
    [Authorize(Roles = "ServiceManager,Administrator")]
    public class SchedulingController : Controller
    {
        private readonly ISchedulingService _schedulingService;
        private readonly ApplicationDbContext _db;

        public SchedulingController(ISchedulingService schedulingService, ApplicationDbContext db)
        {
            _schedulingService = schedulingService;
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var pending = await _db.ServiceBookings
                .Where(b => b.Status == ServiceBookingStatus.Pending)
                .OrderBy(b => b.PreferredDate)
                .ToListAsync();
            var mechanics = await _db.Mechanics.OrderByDescending(m => m.Rating).ToListAsync();
            ViewBag.Mechanics = mechanics;
            return View(pending);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int bookingId, DateTime start, int? mechanicId)
        {
            var date = DateOnly.FromDateTime(start);
            var time = TimeOnly.FromDateTime(start);
            var job = await _schedulingService.ScheduleJobAsync(bookingId, date, time, mechanicId);
            return RedirectToAction("Index");
        }
    }
}