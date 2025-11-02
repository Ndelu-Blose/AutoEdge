using AutoEdge.Data;
using AutoEdge.Models.Entities;
using AutoEdge.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AutoEdge.Controllers
{
    [Authorize] // Allow all authenticated users to access vehicle management
    public class VehicleController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public VehicleController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Vehicle
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentFilter"] = searchString;

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;
            ViewData["MakeSortParm"] = String.IsNullOrEmpty(sortOrder) ? "make_desc" : "";
            ViewData["ModelSortParm"] = sortOrder == "Model" ? "model_desc" : "Model";
            ViewData["YearSortParm"] = sortOrder == "Year" ? "year_desc" : "Year";
            ViewData["PriceSortParm"] = sortOrder == "Price" ? "price_desc" : "Price";
            ViewData["StatusSortParm"] = sortOrder == "Status" ? "status_desc" : "Status";

            // Build filter expression
            Expression<Func<Vehicle, bool>>? filter = null;
            if (!String.IsNullOrEmpty(searchString))
            {
                filter = v => (v.Make != null && v.Make.Contains(searchString)) ||
                             (v.Model != null && v.Model.Contains(searchString)) ||
                             (v.VIN != null && v.VIN.Contains(searchString));
            }

            // Build order expression
            Func<IQueryable<Vehicle>, IOrderedQueryable<Vehicle>>? orderBy = null;
            switch (sortOrder)
            {
                case "make_desc":
                    orderBy = q => q.OrderByDescending(v => v.Make ?? "");
                    break;
                case "Model":
                    orderBy = q => q.OrderBy(v => v.Model ?? "");
                    break;
                case "model_desc":
                    orderBy = q => q.OrderByDescending(v => v.Model ?? "");
                    break;
                case "Year":
                    orderBy = q => q.OrderBy(v => v.Year);
                    break;
                case "year_desc":
                    orderBy = q => q.OrderByDescending(v => v.Year);
                    break;
                case "Price":
                    orderBy = q => q.OrderBy(v => v.SellingPrice);
                    break;
                case "price_desc":
                    orderBy = q => q.OrderByDescending(v => v.SellingPrice);
                    break;
                case "Status":
                    orderBy = q => q.OrderBy(v => v.Status ?? "");
                    break;
                case "status_desc":
                    orderBy = q => q.OrderByDescending(v => v.Status ?? "");
                    break;
                default:
                    orderBy = q => q.OrderBy(v => v.Make ?? "");
                    break;
            }

            int pageSize = 10;
            int pageNumber = page ?? 1;
            
            // Use GetPagedAsync for proper database-level pagination
            var pagedVehicles = await _unitOfWork.Vehicles.GetPagedAsync(
                pageNumber, pageSize, filter, orderBy, "VehicleImages");

            // Get total count for pagination
            var totalCount = await _unitOfWork.Vehicles.CountAsync(filter);

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.HasPreviousPage = pageNumber > 1;
            ViewBag.HasNextPage = pageNumber < ViewBag.TotalPages;

            return View(pagedVehicles);
        }

        // GET: Vehicle/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _unitOfWork.Vehicles.GetWithIncludeAsync(
                filter: v => v.Id == id.Value,
                includeProperties: "VehicleImages");
            var vehicleEntity = vehicle.FirstOrDefault();
            if (vehicleEntity == null)
            {
                return NotFound();
            }

            // Get related inquiries for this vehicle
            var inquiries = await _unitOfWork.Inquiries.FindAsync(i => i.VehicleId == id.Value);
            ViewBag.Inquiries = inquiries.OrderByDescending(i => i.CreatedDate).Take(5);

            return View(vehicleEntity);
        }

        // GET: Vehicle/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Vehicle/Create
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Create(Vehicle vehicle, List<IFormFile> images)
        // {
        //     // Debug: Log form submission
        //     Console.WriteLine($"Vehicle Create POST called - VIN: {vehicle.VIN}, Make: {vehicle.Make}, Model: {vehicle.Model}");
            
        //     // Debug: Check ModelState
        //     if (!ModelState.IsValid)
        //     {
        //         Console.WriteLine("ModelState is invalid:");
        //         foreach (var error in ModelState)
        //         {
        //             Console.WriteLine($"Key: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
        //         }
        //     }
            
        //     if (ModelState.IsValid)
        //     {
        //         // Set timestamps first
        //         vehicle.CreatedDate = DateTime.UtcNow;
        //         vehicle.ModifiedDate = DateTime.UtcNow;
                
        //         // Save vehicle first to get the generated Id
        //         await _unitOfWork.Vehicles.AddAsync(vehicle);
        //         await _unitOfWork.SaveChangesAsync();
                
        //         // Handle image uploads after vehicle is saved
        //         if (images != null && images.Count > 0)
        //         {
        //             var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "vehicles");
                    
        //             if (!Directory.Exists(uploadsFolder))
        //             {
        //                 Directory.CreateDirectory(uploadsFolder);
        //             }

        //             foreach (var image in images)
        //             {
        //                 if (image.Length > 0)
        //                 {
        //                     var uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
        //                     var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                            
        //                     using (var fileStream = new FileStream(filePath, FileMode.Create))
        //                     {
        //                         await image.CopyToAsync(fileStream);
        //                     }
                            
        //                     var vehicleImage = new VehicleImage
        //                     {
        //                         ImagePath = "/images/vehicles/" + uniqueFileName,
        //                         VehicleId = vehicle.Id // Now vehicle.Id has the correct value
        //                     };
                            
        //                     await _unitOfWork.VehicleImages.AddAsync(vehicleImage);
        //                 }
        //             }
                    
        //             // Save the images
        //             await _unitOfWork.SaveChangesAsync();
        //         }
                
        //         TempData["SuccessMessage"] = "Vehicle created successfully!";
        //         return RedirectToAction(nameof(Index));
        //     }
            
        //     return View(vehicle);
        // }

//         [HttpPost]
// [ValidateAntiForgeryToken]
// public async Task<IActionResult> Create(Vehicle vehicle, List<IFormFile> images)
// {
//     // Debug: log incoming vehicle
//     Console.WriteLine($"Creating Vehicle - VIN: {vehicle.VIN}, Make: {vehicle.Make}, Model: {vehicle.Model}");

//     if (!ModelState.IsValid)
//     {
//         Console.WriteLine("ModelState invalid:");
//         foreach (var error in ModelState)
//         {
//             Console.WriteLine($"Key: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
//         }
//         return View(vehicle);
//     }

//     try
//     {
//         // Set timestamps
//         vehicle.CreatedDate = DateTime.UtcNow;
//         vehicle.ModifiedDate = DateTime.UtcNow;

//         // Ensure VehicleImages collection is initialized
//         vehicle.VehicleImages = vehicle.VehicleImages ?? new List<VehicleImage>();

//         // 1️⃣ Save vehicle first to generate Id
//         await _unitOfWork.Vehicles.AddAsync(vehicle);
//         await _unitOfWork.SaveChangesAsync();

//         // 2️⃣ Handle image uploads
//         if (images != null && images.Count > 0)
//         {
//             var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "vehicles");
//             if (!Directory.Exists(uploadsFolder))
//                 Directory.CreateDirectory(uploadsFolder);

//             foreach (var image in images)
//             {
//                 if (image.Length > 0)
//                 {
//                     // Generate unique file name
//                     var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image.FileName);
//                     var filePath = Path.Combine(uploadsFolder, uniqueFileName);

//                     // Save file
//                     using (var fileStream = new FileStream(filePath, FileMode.Create))
//                     {
//                         await image.CopyToAsync(fileStream);
//                     }

//                     // Add VehicleImage
//                     var vehicleImage = new VehicleImage
//                     {
//                         ImagePath = "/images/vehicles/" + uniqueFileName,
//                         Vehicle = vehicle,  // EF sets VehicleId automatically
//                         FileName = image.FileName,
//                         FileSize = image.Length,
//                         ContentType = image.ContentType
//                     };

//                     vehicle.VehicleImages.Add(vehicleImage);
//                 }
//             }

//             // Save images
//             await _unitOfWork.SaveChangesAsync();
//         }

//         TempData["SuccessMessage"] = "Vehicle created successfully!";
//         return RedirectToAction(nameof(Index));
//     }
//     catch (Exception ex)
//     {
//         Console.WriteLine($"Error creating vehicle: {ex.Message}");
//         ModelState.AddModelError(string.Empty, "An error occurred while saving the vehicle. Please try again.");
//         return View(vehicle);
//     }
// }

                [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Vehicle vehicle, List<IFormFile> images)
        {
            if (!ModelState.IsValid)
                return View(vehicle);

            vehicle.CreatedDate = DateTime.UtcNow;
            vehicle.ModifiedDate = DateTime.UtcNow;

            if (images != null && images.Count > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "vehicles");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                foreach (var image in images)
                {
                    if (image.Length <= 0) continue;

                    var uniqueFileName = Guid.NewGuid() + "_" + image.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                        await image.CopyToAsync(fileStream);

                    var vehicleImage = new VehicleImage
                    {
                        ImagePath = "/images/vehicles/" + uniqueFileName
                    };
                    vehicle.VehicleImages.Add(vehicleImage);
                }
            }

            await _unitOfWork.Vehicles.AddAsync(vehicle);
            await _unitOfWork.SaveChangesAsync(); // EF Core now generates Vehicle.Id and assigns VehicleId for images

            TempData["SuccessMessage"] = "Vehicle created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Vehicle/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(id.Value);
            if (vehicle == null)
            {
                return NotFound();
            }
            
            return View(vehicle);
        }

        // POST: Vehicle/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Vehicle vehicle, List<IFormFile> newImages, string? removeImages)
        {
            if (id != vehicle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingVehicle = await _unitOfWork.Vehicles.GetByIdAsync(id);
                    if (existingVehicle == null)
                    {
                        return NotFound();
                    }

                    // Update vehicle properties
                    existingVehicle.Make = vehicle.Make;
                    existingVehicle.Model = vehicle.Model;
                    existingVehicle.Year = vehicle.Year;
                    existingVehicle.VIN = vehicle.VIN;
                    existingVehicle.SellingPrice = vehicle.SellingPrice;
                    existingVehicle.Mileage = vehicle.Mileage;
                    existingVehicle.ExteriorColor = vehicle.ExteriorColor;
                    existingVehicle.EngineType = vehicle.EngineType;
                    existingVehicle.Transmission = vehicle.Transmission;
                    existingVehicle.Condition = vehicle.Condition;
                    existingVehicle.EngineType = vehicle.EngineType;
                    existingVehicle.Features = vehicle.Features;
                    existingVehicle.Status = vehicle.Status;
                    existingVehicle.ModifiedDate = DateTime.UtcNow;

                    // Handle image removal
                    if (!string.IsNullOrEmpty(removeImages))
                    {
                        var imagesToRemove = removeImages.Split(',');
                        foreach (var imageToRemove in imagesToRemove)
                        {
                            var vehicleImage = existingVehicle.VehicleImages.FirstOrDefault(vi => vi.ImagePath == imageToRemove);
                            if (vehicleImage != null)
                            {
                                existingVehicle.VehicleImages.Remove(vehicleImage);
                                // Delete physical file
                                var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, imageToRemove.TrimStart('/'));
                                if (System.IO.File.Exists(physicalPath))
                                {
                                    System.IO.File.Delete(physicalPath);
                                }
                            }
                        }
                    }

                    // Handle new image uploads
                    if (newImages != null && newImages.Count > 0)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "vehicles");
                        
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        foreach (var image in newImages)
                        {
                            if (image.Length > 0)
                            {
                                var uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                                
                                using (var fileStream = new FileStream(filePath, FileMode.Create))
                                {
                                    await image.CopyToAsync(fileStream);
                                }
                                
                                var vehicleImage = new VehicleImage
                                {
                                    ImagePath = "/images/vehicles/" + uniqueFileName,
                                    VehicleId = existingVehicle.Id
                                };
                                existingVehicle.VehicleImages.Add(vehicleImage);
                            }
                        }
                    }

                    _unitOfWork.Vehicles.Update(existingVehicle);
                    await _unitOfWork.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Vehicle updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await VehicleExists(vehicle.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            
            return View(vehicle);
        }

        // GET: Vehicle/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(id.Value);
            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }

        // POST: Vehicle/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(id);
            if (vehicle != null)
            {
                // Delete associated images
                if (vehicle.VehicleImages != null && vehicle.VehicleImages.Any())
                {
                    foreach (var vehicleImage in vehicle.VehicleImages)
                    {
                        var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, vehicleImage.ImagePath.TrimStart('/'));
                        if (System.IO.File.Exists(physicalPath))
                        {
                            System.IO.File.Delete(physicalPath);
                        }
                    }
                }

                _unitOfWork.Vehicles.Remove(vehicle);
                await _unitOfWork.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Vehicle deleted successfully!";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // POST: Vehicle/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }

            vehicle.Status = vehicle.Status == "Available" ? "Sold" : "Available";
            vehicle.ModifiedDate = DateTime.UtcNow;
            
            _unitOfWork.Vehicles.Update(vehicle);
            await _unitOfWork.SaveChangesAsync();
            
            TempData["SuccessMessage"] = $"Vehicle status updated to {vehicle.Status}!";
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> VehicleExists(int id)
        {
            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(id);
            return vehicle != null;
        }
    }
}