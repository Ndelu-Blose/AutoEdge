using AutoEdge.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace AutoEdge.Data
{
    public static class SeedData
    {
        public static async Task SeedDocumentTypesAsync(ApplicationDbContext context)
        {
            // Check if DocumentTypes already exist
            if (await context.DocumentTypes.AnyAsync())
            {
                return; // Data already seeded
            }

            var documentTypes = new List<DocumentType>
            {
                new DocumentType
                {
                    Name = "Driver's License",
                    Description = "Valid driver's license for identity verification",
                    IsRequired = true,
                    IsActive = true,
                    AllowedFileTypes = ".pdf,.jpg,.jpeg,.png",
                    MaxFileSizeMB = 5
                },
                new DocumentType
                {
                    Name = "National ID",
                    Description = "National identification card or passport",
                    IsRequired = true,
                    IsActive = true,
                    AllowedFileTypes = ".pdf,.jpg,.jpeg,.png",
                    MaxFileSizeMB = 5
                },
                new DocumentType
                {
                    Name = "Proof of Income",
                    Description = "Pay stubs, tax returns, or employment letter",
                    IsRequired = true,
                    IsActive = true,
                    AllowedFileTypes = ".pdf,.doc,.docx,.jpg,.jpeg,.png",
                    MaxFileSizeMB = 10
                },
                new DocumentType
                {
                    Name = "Bank Statement",
                    Description = "Recent bank statements (last 3 months)",
                    IsRequired = false,
                    IsActive = true,
                    AllowedFileTypes = ".pdf,.jpg,.jpeg,.png",
                    MaxFileSizeMB = 10
                },
                new DocumentType
                {
                    Name = "Insurance Certificate",
                    Description = "Valid auto insurance certificate",
                    IsRequired = true,
                    IsActive = true,
                    AllowedFileTypes = ".pdf,.jpg,.jpeg,.png",
                    MaxFileSizeMB = 5
                },
                new DocumentType
                {
                    Name = "Credit Report",
                    Description = "Recent credit report for financing approval",
                    IsRequired = false,
                    IsActive = true,
                    AllowedFileTypes = ".pdf",
                    MaxFileSizeMB = 5
                },
                new DocumentType
                {
                    Name = "Utility Bill",
                    Description = "Recent utility bill for address verification",
                    IsRequired = false,
                    IsActive = true,
                    AllowedFileTypes = ".pdf,.jpg,.jpeg,.png",
                    MaxFileSizeMB = 5
                }
            };

            context.DocumentTypes.AddRange(documentTypes);
            await context.SaveChangesAsync();
        }

        public static async Task SeedVehiclesAsync(ApplicationDbContext context)
        {
            // Check if Vehicles already exist
            if (await context.Vehicles.AnyAsync())
            {
                return; // Data already seeded
            }

            var vehicles = new List<Vehicle>
            {
                // Luxury Sedans
                new Vehicle
                {
                    VIN = "1HGBH41JXMN109186",
                    LicensePlate = "LUX001",
                    Make = "BMW",
                    Model = "3 Series",
                    Year = 2023,
                    Mileage = 15000,
                    EngineType = "2.0L Turbo I4",
                    Transmission = "Automatic",
                    InteriorColor = "Black Leather",
                    ExteriorColor = "Alpine White",
                    Condition = "Used",
                    Features = "Air Conditioning,Bluetooth,Navigation,Sunroof,Heated Seats,Backup Camera,Alloy Wheels,Cruise Control",
                    CostPrice = 532000.00m,
                    SellingPrice = 538500.00m,
                    NegotiableRange = 40000.00m,
                    Status = "Available",
                    Location = "Main Lot A-1",
                    DateListed = DateTime.UtcNow.AddDays(-10),
                    Source = "Trade-in",
                    PurchaseDate = DateTime.UtcNow.AddDays(-30),
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow.AddDays(-30),
                    ModifiedDate = DateTime.UtcNow.AddDays(-10),
                    IsActive = true,
                    IsDeleted = false
                },
                new Vehicle
                {
                    VIN = "WBAVA33598NM12345",
                    LicensePlate = "LUX002",
                    Make = "Mercedes-Benz",
                    Model = "C-Class",
                    Year = 2022,
                    Mileage = 22000,
                    EngineType = "2.0L Turbo I4",
                    Transmission = "Automatic",
                    InteriorColor = "Beige Leather",
                    ExteriorColor = "Obsidian Black",
                    Condition = "Certified Pre-owned",
                    Features = "Air Conditioning,Bluetooth,Navigation,Sunroof,Heated Seats,Backup Camera,Alloy Wheels,Cruise Control,Premuim Sound",
                    CostPrice = 728000.00m,
                    SellingPrice = 834900.00m,
                    NegotiableRange = 51500.00m,
                    Status = "Available",
                    Location = "Main Lot A-2",
                    DateListed = DateTime.UtcNow.AddDays(-5),
                    Source = "Auction",
                    PurchaseDate = DateTime.UtcNow.AddDays(-20),
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow.AddDays(-20),
                    ModifiedDate = DateTime.UtcNow.AddDays(-5),
                    IsActive = true,
                    IsDeleted = false
                },
                // SUVs
                new Vehicle
                {
                    VIN = "5UXWX9C50F0D12345",
                    LicensePlate = "SUV001",
                    Make = "Toyota",
                    Model = "RAV4",
                    Year = 2023,
                    Mileage = 8500,
                    EngineType = "2.5L Hybrid I4",
                    Transmission = "CVT",
                    InteriorColor = "Gray Cloth",
                    ExteriorColor = "Magnetic Gray",
                    Condition = "Used",
                    Features = "Air Conditioning,Bluetooth,Navigation,Sunroof,Heated Seats,Backup Camera,Alloy Wheels,Cruise Control,All Wheel Drive",
                    CostPrice = 526000.00m,
                    SellingPrice = 631200.00m,
                    NegotiableRange = 41200.00m,
                    Status = "Available",
                    Location = "Main Lot B-1",
                    DateListed = DateTime.UtcNow.AddDays(-3),
                    Source = "Direct Purchase",
                    PurchaseDate = DateTime.UtcNow.AddDays(-15),
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow.AddDays(-15),
                    ModifiedDate = DateTime.UtcNow.AddDays(-3),
                    IsActive = true,
                    IsDeleted = false
                },
                new Vehicle
                {
                    VIN = "1FMCU9GD8KUA12345",
                    LicensePlate = "SUV002",
                    Make = "Ford",
                    Model = "Escape",
                    Year = 2021,
                    Mileage = 35000,
                    EngineType = "1.5L Turbo I3",
                    Transmission = "Automatic",
                    InteriorColor = "Black Cloth",
                    ExteriorColor = "Oxford White",
                    Condition = "Used",
                    Features = "Air Conditioning,Bluetooth,Navigation,Sunroof,Heated Seats,Backup Camera,Alloy Wheels,Cruise Control",
                    CostPrice = 418500.00m,
                    SellingPrice = 523800.00m,
                    NegotiableRange = 41000.00m,
                    Status = "Available",
                    Location = "Main Lot B-2",
                    DateListed = DateTime.UtcNow.AddDays(-7),
                    Source = "Trade-in",
                    PurchaseDate = DateTime.UtcNow.AddDays(-25),
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow.AddDays(-25),
                    ModifiedDate = DateTime.UtcNow.AddDays(-7),
                    IsActive = true,
                    IsDeleted = false
                },
                // Trucks
                new Vehicle
                {
                    VIN = "1FTFW1ET5DFC12345",
                    LicensePlate = "TRK001",
                    Make = "Ford",
                    Model = "F-150",
                    Year = 2022,
                    Mileage = 28000,
                    EngineType = "3.5L V6 EcoBoost",
                    Transmission = "Automatic",
                    InteriorColor = "Gray Cloth",
                    ExteriorColor = "Agate Black",
                    Condition = "Used",
                    Features = "Air Conditioning,Bluetooth,Navigation,Sunroof,Heated Seats,Backup Camera,Alloy Wheels,Cruise Control,All Wheel Drive,Bed Liner",
                    CostPrice = 435000.00m,
                    SellingPrice = 542500.00m,
                    NegotiableRange = 22500.00m,
                    Status = "Available",
                    Location = "Main Lot C-1",
                    DateListed = DateTime.UtcNow.AddDays(-12),
                    Source = "Auction",
                    PurchaseDate = DateTime.UtcNow.AddDays(-35),
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow.AddDays(-35),
                    ModifiedDate = DateTime.UtcNow.AddDays(-12),
                    IsActive = true,
                    IsDeleted = false
                },
                // Compact Cars
                new Vehicle
                {
                    VIN = "KMHL14JA5KA123456",
                    LicensePlate = "CMP001",
                    Make = "Hyundai",
                    Model = "Elantra",
                    Year = 2020,
                    Mileage = 45000,
                    EngineType = "2.0L I4",
                    Transmission = "CVT",
                    InteriorColor = "Black Cloth",
                    ExteriorColor = "Intense Blue",
                    Condition = "Used",
                    Features = "Air Conditioning,Bluetooth,Navigation,Sunroof,Heated Seats,Backup Camera,Alloy Wheels,Cruise Control,All Wheel Drive",
                    CostPrice = 312000.00m,
                    SellingPrice = 416800.00m,
                    NegotiableRange = 1800.00m,
                    Status = "Available",
                    Location = "Economy Lot E-1",
                    DateListed = DateTime.UtcNow.AddDays(-8),
                    Source = "Trade-in",
                    PurchaseDate = DateTime.UtcNow.AddDays(-22),
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow.AddDays(-22),
                    ModifiedDate = DateTime.UtcNow.AddDays(-8),
                    IsActive = true,
                    IsDeleted = false
                },
                new Vehicle
                {
                    VIN = "1N4AL3AP8JC123456",
                    LicensePlate = "CMP002",
                    Make = "Nissan",
                    Model = "Altima",
                    Year = 2019,
                    Mileage = 52000,
                    EngineType = "2.5L I4",
                    Transmission = "CVT",
                    InteriorColor = "Beige Cloth",
                    ExteriorColor = "Pearl White",
                    Condition = "Used",
                    Features = "Air Conditioning,Bluetooth,Navigation,Sunroof,Heated Seats,Backup Camera,Alloy Wheels,Cruise Control,All Wheel Drive",
                    CostPrice = 514000.00m,
                    SellingPrice = 618900.00m,
                    NegotiableRange = 19000.00m,
                    Status = "Available",
                    Location = "Economy Lot E-2",
                    DateListed = DateTime.UtcNow.AddDays(-6),
                    Source = "Direct Purchase",
                    PurchaseDate = DateTime.UtcNow.AddDays(-18),
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow.AddDays(-18),
                    ModifiedDate = DateTime.UtcNow.AddDays(-6),
                    IsActive = true,
                    IsDeleted = false
                },
                // Sports Cars
                new Vehicle
                {
                    VIN = "1G1FB1RX8H0123456",
                    LicensePlate = "SPT001",
                    Make = "Chevrolet",
                    Model = "Camaro",
                    Year = 2021,
                    Mileage = 18000,
                    EngineType = "3.6L V6",
                    Transmission = "Manual",
                    InteriorColor = "Red Leather",
                    ExteriorColor = "Rally Red",
                    Condition = "Used",
                    Features = "Air Conditioning,Bluetooth,Navigation,Sunroof,Heated Seats,Backup Camera,Alloy Wheels,Cruise Control,All Wheel Drive,Sport Mode,Performance Package",
                    CostPrice = 428000.00m,
                    SellingPrice = 534500.00m,
                    NegotiableRange = 31500.00m,
                    Status = "Available",
                    Location = "Premium Lot P-1",
                    DateListed = DateTime.UtcNow.AddDays(-4),
                    Source = "Trade-in",
                    PurchaseDate = DateTime.UtcNow.AddDays(-16),
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow.AddDays(-16),
                    ModifiedDate = DateTime.UtcNow.AddDays(-4),
                    IsActive = true,
                    IsDeleted = false
                },
                //// Electric Vehicles
                //new Vehicle
                //{
                //    VIN = "5YJ3E1EA4KF123456",
                //    LicensePlate = "EV001",
                //    Make = "Tesla",
                //    Model = "Model 3",
                //    Year = 2022,
                //    Mileage = 12000,
                //    EngineType = "Electric Motor",
                //    Transmission = "Single Speed",
                //    InteriorColor = "White Interior",
                //    ExteriorColor = "Midnight Silver",
                //    Condition = "Used",
                //    Features = "{\"airConditioning\":true,\"bluetooth\":true,\"navigation\":true,\"sunroof\":true,\"heatedSeats\":true,\"backupCamera\":true,\"alloyWheels\":true,\"cruiseControl\":true,\"autopilot\":true,\"supercharging\":true}",
                //    CostPrice = 38000.00m,
                //    SellingPrice = 45900.00m,
                //    NegotiableRange = 2000.00m,
                //    Status = "Available",
                //    Location = "Premium Lot P-2",
                //    DateListed = DateTime.UtcNow.AddDays(-2),
                //    Source = "Direct Purchase",
                //    PurchaseDate = DateTime.UtcNow.AddDays(-12),
                //    CreatedBy = "System",
                //    CreatedDate = DateTime.UtcNow.AddDays(-12),
                //    ModifiedDate = DateTime.UtcNow.AddDays(-2),
                //    IsActive = true,
                //    IsDeleted = false
                //},
                //// Reserved Vehicle (for testing)
                //new Vehicle
                //{
                //    VIN = "JM1BK32F781123456",
                //    LicensePlate = "RSV001",
                //    Make = "Mazda",
                //    Model = "CX-5",
                //    Year = 2023,
                //    Mileage = 5000,
                //    EngineType = "2.5L I4",
                //    Transmission = "Automatic",
                //    InteriorColor = "Black Leather",
                //    ExteriorColor = "Soul Red Crystal",
                //    Condition = "Used",
                //    Features = "{\"airConditioning\":true,\"bluetooth\":true,\"navigation\":true,\"sunroof\":true,\"heatedSeats\":true,\"backupCamera\":true,\"alloyWheels\":true,\"cruiseControl\":true,\"allWheelDrive\":true}",
                //    CostPrice = 24000.00m,
                //    SellingPrice = 29800.00m,
                //    NegotiableRange = 1300.00m,
                //    Status = "Reserved",
                //    Location = "Main Lot B-3",
                //    DateListed = DateTime.UtcNow.AddDays(-9),
                //    Source = "Trade-in",
                //    PurchaseDate = DateTime.UtcNow.AddDays(-28),
                //    CreatedBy = "System",
                //    CreatedDate = DateTime.UtcNow.AddDays(-28),
                //    ModifiedDate = DateTime.UtcNow.AddDays(-1),
                //    IsActive = true,
                //    IsDeleted = false
                //},
                //// Under Maintenance Vehicle (for testing)
                //new Vehicle
                //{
                //    VIN = "WVWZZZ3CZHE123456",
                //    LicensePlate = "MNT001",
                //    Make = "Volkswagen",
                //    Model = "Jetta",
                //    Year = 2020,
                //    Mileage = 38000,
                //    EngineType = "1.4L Turbo I4",
                //    Transmission = "Automatic",
                //    InteriorColor = "Black Cloth",
                //    ExteriorColor = "Pure White",
                //    Condition = "Used",
                //    Features = "{\"airConditioning\":true,\"bluetooth\":true,\"navigation\":false,\"sunroof\":true,\"heatedSeats\":false,\"backupCamera\":true,\"alloyWheels\":true,\"cruiseControl\":true}",
                //    CostPrice = 15000.00m,
                //    SellingPrice = 20500.00m,
                //    NegotiableRange = 1000.00m,
                //    Status = "Under Maintenance",
                //    Location = "Service Bay 1",
                //    DateListed = DateTime.UtcNow.AddDays(-15),
                //    Source = "Auction",
                //    PurchaseDate = DateTime.UtcNow.AddDays(-40),
                //    CreatedBy = "System",
                //    CreatedDate = DateTime.UtcNow.AddDays(-40),
                //    ModifiedDate = DateTime.UtcNow.AddDays(-3),
                //    IsActive = true,
                //    IsDeleted = false
                //}
            };

            context.Vehicles.AddRange(vehicles);
            await context.SaveChangesAsync();
        }

        public static async Task SeedVehicleImagesAsync(ApplicationDbContext context)
        {
            // Check if VehicleImages already exist
            if (await context.VehicleImages.AnyAsync())
            {
                return; // Data already seeded
            }

            // Get existing vehicles
            var vehicles = await context.Vehicles.ToListAsync();
            if (!vehicles.Any())
            {
                return; // No vehicles to assign images to
            }

            // List of available image files in wwwroot/images/vehicles
            var imageFiles = new List<string>
            {
                "2f9779e6-927c-4c47-bb39-db2ba73936ca_2-25.png",
                "3b8a4d2e-5f1c-4a89-9e7b-1c3d5e7f9a2b_car1.jpg",
                "4c9b5e3f-6g2d-5b90-af8c-2d4e6f8g0b3c_car2.jpg",
                "5d0c6f4g-7h3e-6c01-bg9d-3e5f7g9h1c4d_car3.jpg"
            };

            var vehicleImages = new List<VehicleImage>();
            var random = new Random();

            // Assign 1-3 random images to each vehicle
            foreach (var vehicle in vehicles)
            {
                var imageCount = random.Next(1, 4); // 1 to 3 images per vehicle
                var shuffledImages = imageFiles.OrderBy(x => random.Next()).Take(imageCount).ToList();

                for (int i = 0; i < shuffledImages.Count; i++)
                {
                    var imagePath = $"/images/vehicles/{shuffledImages[i]}";
                    var fileName = shuffledImages[i];
                    
                    vehicleImages.Add(new VehicleImage
                    {
                        VehicleId = vehicle.Id,
                        ImagePath = imagePath,
                        ImageType = i == 0 ? "Primary" : "Gallery",
                        UploadDate = DateTime.UtcNow,
                        DisplayOrder = i + 1,
                        IsActive = true,
                        FileName = fileName,
                        FileSize = 1024000, // Approximate 1MB
                        ContentType = fileName.EndsWith(".png") ? "image/png" : "image/jpeg",
                        AltText = $"{vehicle.Make} {vehicle.Model} {vehicle.Year} - Image {i + 1}"
                    });
                }
            }

            if (vehicleImages.Any())
            {
                context.VehicleImages.AddRange(vehicleImages);
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedDriverUserAsync(ApplicationDbContext context)
        {
            // Check if driver user already exists
            var driverUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "driver@autoedge.com");
            if (driverUser != null)
            {
                // Check if driver has the Driver role assigned
                var driverRoleId = await context.Roles.Where(r => r.Name == "Driver").Select(r => r.Id).FirstOrDefaultAsync();
                if (driverRoleId != null)
                {
                    var hasDriverRole = await context.UserRoles.AnyAsync(ur => ur.UserId == driverUser.Id && ur.RoleId == driverRoleId);
                    if (!hasDriverRole)
                    {
                        // Assign Driver role to existing user
                        context.UserRoles.Add(new Microsoft.AspNetCore.Identity.IdentityUserRole<string>
                        {
                            UserId = driverUser.Id,
                            RoleId = driverRoleId
                        });
                        await context.SaveChangesAsync();
                    }
                }
                return; // Driver user already exists
            }

            // Create driver user
            var driver = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "driver@autoedge.com",
                NormalizedUserName = "DRIVER@AUTOEDGE.COM",
                Email = "driver@autoedge.com",
                NormalizedEmail = "DRIVER@AUTOEDGE.COM",
                EmailConfirmed = true,
                PasswordHash = "AQAAAAEAACcQAAAAEDummyHashForTestingPurposes123456789",
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = true,
                AccessFailedCount = 0,
                FirstName = "Test",
                LastName = "Driver",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            context.Users.Add(driver);
            await context.SaveChangesAsync();

            // Assign Driver role to the new user
            var roleId = await context.Roles.Where(r => r.Name == "Driver").Select(r => r.Id).FirstOrDefaultAsync();
            if (roleId != null)
            {
                context.UserRoles.Add(new Microsoft.AspNetCore.Identity.IdentityUserRole<string>
                {
                    UserId = driver.Id,
                    RoleId = roleId
                });
                await context.SaveChangesAsync();
            }

            // Update existing deliveries to assign them to the driver
            var deliveries = await context.Deliveries
                .Where(d => string.IsNullOrEmpty(d.DriverUserId))
                .ToListAsync();

            foreach (var delivery in deliveries)
            {
                delivery.DriverUserId = driver.Id;
                delivery.DriverName = $"{driver.FirstName} {driver.LastName}";
                delivery.DriverPhone = driver.PhoneNumber ?? "";
            }

            if (deliveries.Any())
            {
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedPickupDriverAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            // Check if pickup driver already exists
            var driverEmail = "mkhizesigcino69@gmail.com";
            var driverUser = await userManager.FindByEmailAsync(driverEmail);
            
            if (driverUser != null)
            {
                // Check if Driver entity exists for this user
                var driverEntity = await context.Drivers.FirstOrDefaultAsync(d => d.UserId == driverUser.Id);
                if (driverEntity != null)
                {
                    return; // Driver already fully set up
                }

                // User exists but no Driver entity - create one
                var driver = new Driver
                {
                    UserId = driverUser.Id,
                    Name = $"{driverUser.FirstName} {driverUser.LastName}",
                    Phone = driverUser.PhoneNumber ?? "0821234567",
                    LicenseNumber = "SA123456789",
                    VehicleRegistration = "ABC123GP",
                    VehicleMake = "Toyota",
                    VehicleModel = "Hilux",
                    VehicleColor = "White",
                    IsAvailable = true,
                    Rating = 5.0m,
                    TotalPickups = 0,
                    SuccessfulPickups = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.Drivers.Add(driver);
                await context.SaveChangesAsync();
                return;
            }

            // Create new driver user
            driverUser = new ApplicationUser
            {
                UserName = driverEmail,
                Email = driverEmail,
                NormalizedUserName = driverEmail.ToUpper(),
                NormalizedEmail = driverEmail.ToUpper(),
                FirstName = "Sigcino",
                LastName = "Mkhize",
                PhoneNumber = "0821234567",
                EmailConfirmed = true,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(driverUser, "Driver@2024");
            if (result.Succeeded)
            {
                // Assign Driver role
                await userManager.AddToRoleAsync(driverUser, "Driver");

                // Create Driver entity
                var driver = new Driver
                {
                    UserId = driverUser.Id,
                    Name = "Sigcino Mkhize",
                    Phone = "0821234567",
                    LicenseNumber = "SA123456789",
                    VehicleRegistration = "ABC123GP",
                    VehicleMake = "Toyota",
                    VehicleModel = "Hilux",
                    VehicleColor = "White",
                    IsAvailable = true,
                    Rating = 5.0m,
                    TotalPickups = 0,
                    SuccessfulPickups = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.Drivers.Add(driver);
                await context.SaveChangesAsync();
            }
        }

        //public static async Task SeedRecruitmentUsersAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        //{
        //    // Seed Recruiter User
        //    var recruiterEmail = "recruiter@autoedge.com";
        //    var recruiterUser = await userManager.FindByEmailAsync(recruiterEmail);
            
        //    if (recruiterUser == null)
        //    {
        //        recruiterUser = new ApplicationUser
        //        {
        //            UserName = recruiterEmail,
        //            Email = recruiterEmail,
        //            FirstName = "Sarah",
        //            LastName = "Johnson",
        //            PhoneNumber = "555-0101",
        //            Address = "123 Recruitment Street",
        //            City = "Recruitment City",
        //            State = "RC",
        //            ZipCode = "12345",
        //            Country = "USA",
        //            EmailConfirmed = true,
        //            IsActive = true,
        //            CreatedDate = DateTime.UtcNow
        //        };
                
        //        var result = await userManager.CreateAsync(recruiterUser, "Recruiter@123");
        //        if (result.Succeeded)
        //        {
        //            await userManager.AddToRoleAsync(recruiterUser, "Recruiter");
        //        }
        //    }

        //    // Seed Test Applicant Users
        //    var applicantEmails = new[]
        //    {
        //        ("applicant1@autoedge.com", "John", "Smith", "Applicant@123"),
        //        ("applicant2@autoedge.com", "Emily", "Davis", "Applicant@123"),
        //        ("applicant3@autoedge.com", "Michael", "Wilson", "Applicant@123"),
        //        ("applicant4@autoedge.com", "Sarah", "Brown", "Applicant@123"),
        //        ("applicant5@autoedge.com", "David", "Miller", "Applicant@123")
        //    };

        //    foreach (var (email, firstName, lastName, password) in applicantEmails)
        //    {
        //        var applicantUser = await userManager.FindByEmailAsync(email);
                
        //        if (applicantUser == null)
        //        {
        //            applicantUser = new ApplicationUser
        //            {
        //                UserName = email,
        //                Email = email,
        //                FirstName = firstName,
        //                LastName = lastName,
        //                PhoneNumber = "555-0" + new Random().Next(100, 999),
        //                Address = $"{new Random().Next(100, 999)} Applicant Street",
        //                City = "Applicant City",
        //                State = "AC",
        //                ZipCode = "12345",
        //                Country = "USA",
        //                EmailConfirmed = true,
        //                IsActive = true,
        //                CreatedDate = DateTime.UtcNow
        //            };
                    
        //            var result = await userManager.CreateAsync(applicantUser, password);
        //            if (result.Succeeded)
        //            {
        //                await userManager.AddToRoleAsync(applicantUser, "Applicant");
        //            }
        //        }
        //    }
        //}
        public static async Task SeedRecruitmentUsersAsync(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager)
{
    // 1.  MAKE SURE THE ROLES EXIST
    foreach (var roleName in new[] { "Recruiter", "Applicant" })
    {
        if (!await roleManager.RoleExistsAsync(roleName))
            await roleManager.CreateAsync(new IdentityRole(roleName));
    }

    // 2.  Seed Recruiter user
    const string recruiterEmail = "recruiter@autoedge.com";
    var recruiterUser = await userManager.FindByEmailAsync(recruiterEmail);
    if (recruiterUser == null)
    {
        recruiterUser = new ApplicationUser
        {
            UserName = recruiterEmail,
            Email = recruiterEmail,
            FirstName = "Sarah",
            LastName = "Johnson",
            PhoneNumber = "555-0101",
            Address = "123 Recruitment Street",
            City = "Recruitment City",
            State = "RC",
            ZipCode = "12345",
            Country = "USA",
            EmailConfirmed = true,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(recruiterUser, "Recruiter@123");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(recruiterUser, "Recruiter");
    }

    // 3.  Seed five test Applicants
    var applicantSeed = new[]
    {
        ("applicant1@autoedge.com", "John",    "Smith",   "Applicant@123"),
        ("applicant2@autoedge.com", "Emily",   "Davis",   "Applicant@123"),
        ("applicant3@autoedge.com", "Michael", "Wilson",  "Applicant@123"),
        ("applicant4@autoedge.com", "Sarah",   "Brown",   "Applicant@123"),
        ("applicant5@autoedge.com", "David",   "Miller",  "Applicant@123")
    };

    foreach (var (email, first, last, password) in applicantSeed)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user != null) continue;

        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = first,
            LastName = last,
            PhoneNumber = $"555-0{new Random().Next(100, 999)}",
            Address = $"{new Random().Next(100, 999)} Applicant Street",
            City = "Applicant City",
            State = "AC",
            ZipCode = "12345",
            Country = "USA",
            EmailConfirmed = true,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        var created = await userManager.CreateAsync(user, password);
        if (created.Succeeded)
            await userManager.AddToRoleAsync(user, "Applicant");
    }
}


        public static async Task SeedJobPostingsAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            // Check if job postings already exist
            if (await context.JobPostings.AnyAsync())
            {
                return; // Data already seeded
            }

            var adminUser = await userManager.FindByEmailAsync("admin@autoedge.com");
            if (adminUser == null) return;

            var jobPostings = new List<JobPosting>
            {
                new JobPosting
                {
                    JobTitle = "Senior Mechanical Engineer",
                    Department = "Mechanical Engineer",
                    JobDescription = "We are seeking a highly skilled Senior Mechanical Engineer to join our automotive engineering team. You will be responsible for designing, developing, and testing mechanical systems for our vehicle fleet.",
                    Requirements = "Bachelor's degree in Mechanical Engineering, 5+ years experience in automotive industry, Proficiency in CAD software (SolidWorks, AutoCAD), Strong knowledge of automotive systems and components, Experience with finite element analysis (FEA), Knowledge of manufacturing processes and materials",
                    Responsibilities = "Design and develop mechanical systems for vehicles, Conduct engineering analysis and testing, Collaborate with cross-functional teams, Ensure compliance with safety and quality standards, Mentor junior engineers, Manage engineering projects from concept to production",
                    MinYearsExperience = 5,
                    RequiredQualifications = "Bachelor's degree in Mechanical Engineering, Professional Engineer (PE) license preferred, Automotive industry experience, CAD certification",
                    PositionsAvailable = 2,
                    PostedDate = DateTime.UtcNow.AddDays(-10),
                    ClosingDate = DateTime.UtcNow.AddDays(20),
                    Status = "Active",
                    CreatedByUserId = adminUser.Id,
                    CreatedDate = DateTime.UtcNow.AddDays(-10),
                    ModifiedDate = DateTime.UtcNow.AddDays(-10),
                    IsActive = true,
                    Notes = "Urgent hiring for new vehicle development project"
                },
                new JobPosting
                {
                    JobTitle = "Sales Representative",
                    Department = "Sales Representative",
                    JobDescription = "Join our dynamic sales team as a Sales Representative. You will be responsible for selling vehicles, building customer relationships, and achieving sales targets in our fast-paced automotive dealership.",
                    Requirements = "High school diploma or equivalent, 2+ years sales experience, Excellent communication and interpersonal skills, Strong negotiation abilities, Customer service orientation, Valid driver's license, Automotive sales experience preferred",
                    Responsibilities = "Sell new and used vehicles to customers, Build and maintain customer relationships, Meet and exceed sales targets, Conduct vehicle demonstrations and test drives, Process sales paperwork and financing, Follow up with customers post-sale",
                    MinYearsExperience = 2,
                    RequiredQualifications = "High school diploma, Sales certification preferred, Automotive sales experience, Customer service training",
                    PositionsAvailable = 3,
                    PostedDate = DateTime.UtcNow.AddDays(-7),
                    ClosingDate = DateTime.UtcNow.AddDays(23),
                    Status = "Active",
                    CreatedByUserId = adminUser.Id,
                    CreatedDate = DateTime.UtcNow.AddDays(-7),
                    ModifiedDate = DateTime.UtcNow.AddDays(-7),
                    IsActive = true,
                    Notes = "Commission-based position with excellent earning potential"
                },
                new JobPosting
                {
                    JobTitle = "Delivery Driver",
                    Department = "Driver",
                    JobDescription = "We are looking for reliable and professional delivery drivers to join our team. You will be responsible for safely delivering vehicles to customers and providing excellent customer service during the delivery process.",
                    Requirements = "Valid commercial driver's license (CDL), Clean driving record, 3+ years driving experience, Excellent customer service skills, Physical ability to drive various vehicle types, Knowledge of local routes and traffic patterns, DOT medical certificate",
                    Responsibilities = "Safely deliver vehicles to customers, Perform pre-delivery vehicle inspections, Complete delivery documentation, Provide customer orientation on vehicle features, Maintain delivery schedule and communicate with dispatch, Ensure vehicle cleanliness and presentation",
                    MinYearsExperience = 3,
                    RequiredQualifications = "Valid CDL license, DOT medical certificate, Clean driving record, Customer service experience",
                    PositionsAvailable = 4,
                    PostedDate = DateTime.UtcNow.AddDays(-5),
                    ClosingDate = DateTime.UtcNow.AddDays(25),
                    Status = "Active",
                    CreatedByUserId = adminUser.Id,
                    CreatedDate = DateTime.UtcNow.AddDays(-5),
                    ModifiedDate = DateTime.UtcNow.AddDays(-5),
                    IsActive = true,
                    Notes = "Full-time position with benefits and competitive pay"
                },
                new JobPosting
                {
                    JobTitle = "Desktop Support Technician",
                    Department = "Desktop Technician",
                    JobDescription = "Join our IT team as a Desktop Support Technician. You will provide technical support to our dealership staff, maintain computer systems, and ensure smooth operation of our technology infrastructure.",
                    Requirements = "Associate degree in IT or related field, 2+ years desktop support experience, Knowledge of Windows and Mac operating systems, Experience with hardware troubleshooting, Strong problem-solving skills, Excellent communication skills, A+ certification preferred",
                    Responsibilities = "Provide technical support to end users, Install and configure computer hardware and software, Troubleshoot network and connectivity issues, Maintain IT equipment inventory, Document technical procedures and solutions, Assist with IT projects and upgrades",
                    MinYearsExperience = 2,
                    RequiredQualifications = "Associate degree in IT, A+ certification preferred, Network+ certification preferred, Desktop support experience",
                    PositionsAvailable = 2,
                    PostedDate = DateTime.UtcNow.AddDays(-3),
                    ClosingDate = DateTime.UtcNow.AddDays(27),
                    Status = "Active",
                    CreatedByUserId = adminUser.Id,
                    CreatedDate = DateTime.UtcNow.AddDays(-3),
                    ModifiedDate = DateTime.UtcNow.AddDays(-3),
                    IsActive = true,
                    Notes = "Great opportunity for career growth in IT"
                }
            };

            context.JobPostings.AddRange(jobPostings);
            await context.SaveChangesAsync();
        }

        public static async Task SeedSampleApplicationsAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            // Check if applications already exist
            if (await context.Applications.AnyAsync())
            {
                return; // Data already seeded
            }

            var jobPostings = await context.JobPostings.ToListAsync();
            if (!jobPostings.Any())
            {
                return; // No job postings to apply for
            }

            var sampleApplications = new List<Application>
            {
                new Application
                {
                    JobId = jobPostings[0].JobId, // First job posting
                    FirstName = "John",
                    LastName = "Smith",
                    Email = "john.smith@email.com",
                    PhoneNumber = "555-0101",
                    Address = "123 Main Street, City, State 12345",
                    YearsOfExperience = 6,
                    HighestQualification = "Bachelor's Degree in Mechanical Engineering",
                    WhySuitableForRole = "I have 6 years of experience in automotive engineering with expertise in CAD design, FEA analysis, and vehicle systems. I have worked on multiple vehicle development projects and have a strong understanding of automotive manufacturing processes.",
                    MatchScore = 85.5m,
                    Status = "Submitted",
                    SubmittedDate = DateTime.UtcNow.AddDays(-5),
                    CreatedDate = DateTime.UtcNow.AddDays(-5),
                    ModifiedDate = DateTime.UtcNow.AddDays(-5),
                    IsActive = true,
                    ExtractedSkills = "CAD Design, FEA Analysis, Automotive Systems, Project Management, Team Leadership",
                    ExtractedEducation = "Bachelor's Degree in Mechanical Engineering, Professional Engineer License"
                },
                new Application
                {
                    JobId = jobPostings[0].JobId, // First job posting
                    FirstName = "Sarah",
                    LastName = "Johnson",
                    Email = "sarah.johnson@email.com",
                    PhoneNumber = "555-0102",
                    Address = "456 Oak Avenue, City, State 12345",
                    YearsOfExperience = 4,
                    HighestQualification = "Master's Degree in Mechanical Engineering",
                    WhySuitableForRole = "I have a Master's degree in Mechanical Engineering and 4 years of experience in automotive design. I specialize in advanced CAD modeling and have worked on electric vehicle projects.",
                    MatchScore = 78.2m,
                    Status = "Submitted",
                    SubmittedDate = DateTime.UtcNow.AddDays(-3),
                    CreatedDate = DateTime.UtcNow.AddDays(-3),
                    ModifiedDate = DateTime.UtcNow.AddDays(-3),
                    IsActive = true,
                    ExtractedSkills = "Advanced CAD, Electric Vehicles, Design Optimization, MATLAB, Python",
                    ExtractedEducation = "Master's Degree in Mechanical Engineering, CAD Certification"
                },
                new Application
                {
                    JobId = jobPostings[1].JobId, // Second job posting (Sales)
                    FirstName = "Mike",
                    LastName = "Davis",
                    Email = "mike.davis@email.com",
                    PhoneNumber = "555-0103",
                    Address = "789 Pine Street, City, State 12345",
                    YearsOfExperience = 3,
                    HighestQualification = "Bachelor's Degree in Business Administration",
                    WhySuitableForRole = "I have 3 years of sales experience in the automotive industry with a proven track record of exceeding sales targets. I have excellent communication skills and customer service experience.",
                    MatchScore = 72.8m,
                    Status = "Submitted",
                    SubmittedDate = DateTime.UtcNow.AddDays(-2),
                    CreatedDate = DateTime.UtcNow.AddDays(-2),
                    ModifiedDate = DateTime.UtcNow.AddDays(-2),
                    IsActive = true,
                    ExtractedSkills = "Sales, Customer Service, Negotiation, CRM Systems, Automotive Knowledge",
                    ExtractedEducation = "Bachelor's Degree in Business Administration, Sales Certification"
                },
                new Application
                {
                    JobId = jobPostings[1].JobId, // Second job posting (Sales)
                    FirstName = "Emily",
                    LastName = "Wilson",
                    Email = "emily.wilson@email.com",
                    PhoneNumber = "555-0104",
                    Address = "321 Elm Street, City, State 12345",
                    YearsOfExperience = 1,
                    HighestQualification = "Associate's Degree in Marketing",
                    WhySuitableForRole = "I am a recent graduate with a passion for sales and customer service. I have completed internships in retail sales and have strong interpersonal skills.",
                    MatchScore = 45.3m,
                    Status = "Submitted",
                    SubmittedDate = DateTime.UtcNow.AddDays(-1),
                    CreatedDate = DateTime.UtcNow.AddDays(-1),
                    ModifiedDate = DateTime.UtcNow.AddDays(-1),
                    IsActive = true,
                    ExtractedSkills = "Customer Service, Communication, Marketing, Social Media, Microsoft Office",
                    ExtractedEducation = "Associate's Degree in Marketing"
                }
            };

            context.Applications.AddRange(sampleApplications);
            await context.SaveChangesAsync();
        }


        public static async Task UpdateExistingApplicationsWithUserIdsAsync(ApplicationDbContext context)
        {
            // Get all applications that don't have a UserId set
            var applicationsWithoutUserId = await context.Applications
                .Where(a => string.IsNullOrEmpty(a.UserId))
                .ToListAsync();

            foreach (var application in applicationsWithoutUserId)
            {
                // Try to find a user with matching email
                var user = await context.Users
                    .FirstOrDefaultAsync(u => u.Email == application.Email);

                if (user != null)
                {
                    application.UserId = user.Id;
                    context.Update(application);
                }
            }

            await context.SaveChangesAsync();
        }

        public static async Task SeedQuestionsAsync(ApplicationDbContext context)
        {
            // Check if questions already exist
            if (await context.Questions.AnyAsync())
            {
                return; // Questions already seeded
            }

            var questions = new List<Question>
            {
                // Mechanical Engineer Questions
                new Question
                {
                    QuestionCode = "ME_Q1",
                    QuestionText = "What is the primary purpose of CAD software in mechanical engineering?",
                    Type = QuestionType.MultipleChoice,
                    Options = System.Text.Json.JsonSerializer.Serialize(new[] { "Documentation", "Design and modeling", "Cost estimation", "Quality control" }),
                    CorrectAnswer = "Design and modeling",
                    Points = 10,
                    Category = "Technical",
                    Department = "Mechanical Engineer",
                    IsActive = true
                },
                new Question
                {
                    QuestionCode = "ME_Q2",
                    QuestionText = "Explain the difference between stress and strain in materials.",
                    Type = QuestionType.ShortAnswer,
                    CorrectAnswer = "stress, strain, force, deformation",
                    Points = 15,
                    Category = "Technical",
                    Department = "Mechanical Engineer",
                    IsActive = true
                },
                new Question
                {
                    QuestionCode = "ME_Q3",
                    QuestionText = "Describe your experience with 3D modeling and simulation software.",
                    Type = QuestionType.Essay,
                    Points = 20,
                    Category = "Experience",
                    Department = "Mechanical Engineer",
                    IsActive = true
                },
                new Question
                {
                    QuestionCode = "ME_Q4",
                    QuestionText = "Which material property is most important for structural components?",
                    Type = QuestionType.MultipleChoice,
                    Options = System.Text.Json.JsonSerializer.Serialize(new[] { "Density", "Tensile strength", "Color", "Thermal conductivity" }),
                    CorrectAnswer = "Tensile strength",
                    Points = 10,
                    Category = "Technical",
                    Department = "Mechanical Engineer",
                    IsActive = true
                },
                new Question
                {
                    QuestionCode = "ME_Q5",
                    QuestionText = "What does FEA stand for in engineering?",
                    Type = QuestionType.MultipleChoice,
                    Options = System.Text.Json.JsonSerializer.Serialize(new[] { "Finite Element Analysis", "Final Engineering Assessment", "Fabrication Engineering Analysis", "Field Engineering Application" }),
                    CorrectAnswer = "Finite Element Analysis",
                    Points = 10,
                    Category = "Technical",
                    Department = "Mechanical Engineer",
                    IsActive = true
                },

                // Sales Representative Questions
                new Question
                {
                    QuestionCode = "SR_Q1",
                    QuestionText = "What is the most important factor in building customer relationships?",
                    Type = QuestionType.MultipleChoice,
                    Options = System.Text.Json.JsonSerializer.Serialize(new[] { "Price", "Trust and communication", "Product features", "Speed of delivery" }),
                    CorrectAnswer = "Trust and communication",
                    Points = 10,
                    Category = "Behavioral",
                    Department = "Sales Representative",
                    IsActive = true
                },
                new Question
                {
                    QuestionCode = "SR_Q2",
                    QuestionText = "How would you handle a customer complaint about a delayed delivery?",
                    Type = QuestionType.ShortAnswer,
                    CorrectAnswer = "apologize, investigate, solution, follow-up",
                    Points = 15,
                    Category = "Behavioral",
                    Department = "Sales Representative",
                    IsActive = true
                },
                new Question
                {
                    QuestionCode = "SR_Q3",
                    QuestionText = "Describe your approach to identifying and qualifying sales leads.",
                    Type = QuestionType.Essay,
                    Points = 20,
                    Category = "Experience",
                    Department = "Sales Representative",
                    IsActive = true
                },
                new Question
                {
                    QuestionCode = "SR_Q4",
                    QuestionText = "What is the first step in the sales process?",
                    Type = QuestionType.MultipleChoice,
                    Options = System.Text.Json.JsonSerializer.Serialize(new[] { "Closing", "Prospecting", "Presentation", "Follow-up" }),
                    CorrectAnswer = "Prospecting",
                    Points = 10,
                    Category = "Technical",
                    Department = "Sales Representative",
                    IsActive = true
                },
                new Question
                {
                    QuestionCode = "SR_Q5",
                    QuestionText = "How do you handle price objections from customers?",
                    Type = QuestionType.ShortAnswer,
                    CorrectAnswer = "value, benefits, negotiation, alternatives",
                    Points = 15,
                    Category = "Behavioral",
                    Department = "Sales Representative",
                    IsActive = true
                },

                // Driver Questions
                new Question
                {
                    QuestionCode = "DR_Q1",
                    QuestionText = "What is the maximum speed limit in a school zone?",
                    Type = QuestionType.MultipleChoice,
                    Options = System.Text.Json.JsonSerializer.Serialize(new[] { "25 mph", "30 mph", "35 mph", "40 mph" }),
                    CorrectAnswer = "25 mph",
                    Points = 10,
                    Category = "Safety",
                    Department = "Driver",
                    IsActive = true
                },
                new Question
                {
                    QuestionCode = "DR_Q2",
                    QuestionText = "What should you check before starting your vehicle each day?",
                    Type = QuestionType.ShortAnswer,
                    CorrectAnswer = "tires, brakes, lights, fluids, mirrors",
                    Points = 15,
                    Category = "Safety",
                    Department = "Driver",
                    IsActive = true
                },
                new Question
                {
                    QuestionCode = "DR_Q3",
                    QuestionText = "Describe your experience with long-distance driving and route planning.",
                    Type = QuestionType.Essay,
                    Points = 20,
                    Category = "Experience",
                    Department = "Driver",
                    IsActive = true
                },
                new Question
                {
                    QuestionCode = "DR_Q4",
                    QuestionText = "What is the minimum following distance in good weather conditions?",
                    Type = QuestionType.MultipleChoice,
                    Options = System.Text.Json.JsonSerializer.Serialize(new[] { "1 second", "2 seconds", "3 seconds", "4 seconds" }),
                    CorrectAnswer = "3 seconds",
                    Points = 10,
                    Category = "Safety",
                    Department = "Driver",
                    IsActive = true
                },
                new Question
                {
                    QuestionCode = "DR_Q5",
                    QuestionText = "How do you handle aggressive drivers on the road?",
                    Type = QuestionType.ShortAnswer,
                    CorrectAnswer = "calm, avoid, report, safety",
                    Points = 15,
                    Category = "Behavioral",
                    Department = "Driver",
                    IsActive = true
                },

                // Desktop Technician Questions
                new Question
                {
                    QuestionCode = "DT_Q1",
                    QuestionText = "What does RAM stand for in computer terminology?",
                    Type = QuestionType.MultipleChoice,
                    Options = System.Text.Json.JsonSerializer.Serialize(new[] { "Random Access Memory", "Read Access Memory", "Rapid Access Memory", "Remote Access Memory" }),
                    CorrectAnswer = "Random Access Memory",
                    Points = 10,
                    Category = "Technical",
                    Department = "Desktop Technician",
                    IsActive = true
                },
                new Question
                {
                    QuestionCode = "DT_Q2",
                    QuestionText = "How would you troubleshoot a computer that won't start?",
                    Type = QuestionType.ShortAnswer,
                    CorrectAnswer = "power, cables, hardware, BIOS, operating system",
                    Points = 15,
                    Category = "Technical",
                    Department = "Desktop Technician",
                    IsActive = true
                },
                new Question
                {
                    QuestionCode = "DT_Q3",
                    QuestionText = "Describe your experience with network troubleshooting and user support.",
                    Type = QuestionType.Essay,
                    Points = 20,
                    Category = "Experience",
                    Department = "Desktop Technician",
                    IsActive = true
                },
                new Question
                {
                    QuestionCode = "DT_Q4",
                    QuestionText = "What is the purpose of a firewall in network security?",
                    Type = QuestionType.MultipleChoice,
                    Options = System.Text.Json.JsonSerializer.Serialize(new[] { "Speed up internet", "Block unauthorized access", "Store files", "Print documents" }),
                    CorrectAnswer = "Block unauthorized access",
                    Points = 10,
                    Category = "Technical",
                    Department = "Desktop Technician",
                    IsActive = true
                },
                new Question
                {
                    QuestionCode = "DT_Q5",
                    QuestionText = "How do you prioritize IT support tickets?",
                    Type = QuestionType.ShortAnswer,
                    CorrectAnswer = "urgency, impact, severity, business",
                    Points = 15,
                    Category = "Behavioral",
                    Department = "Desktop Technician",
                    IsActive = true
                }
            };

            context.Questions.AddRange(questions);
            await context.SaveChangesAsync();
        }
         public static async Task SeedTechnicianUserAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Ensure Technician role exists
            var technicianRole = "Technician";
            if (!await roleManager.RoleExistsAsync(technicianRole))
            {
                await roleManager.CreateAsync(new IdentityRole(technicianRole));
            }
            // Ensure Mechanic role exists
            var mechanicRole = "Mechanic";
            if (!await roleManager.RoleExistsAsync(mechanicRole))
            {
                await roleManager.CreateAsync(new IdentityRole(mechanicRole));
            }

            // Create technician user if not exists
            var techEmail = "tech@autoedge.com";
            var technician = await userManager.FindByEmailAsync(techEmail);
            if (technician == null)
            {
                technician = new ApplicationUser
                {
                    UserName = techEmail,
                    Email = techEmail,
                    FirstName = "Assigned",
                    LastName = "Technician",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                var createResult = await userManager.CreateAsync(technician, "Tech@123");
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(technician, technicianRole);
                    await userManager.AddToRoleAsync(technician, mechanicRole);
                }
            }

            // Create additional technician user for testing
            var tech2Email = "mechanic@autoedge.com";
            var technician2 = await userManager.FindByEmailAsync(tech2Email);
            if (technician2 == null)
            {
                technician2 = new ApplicationUser
                {
                    UserName = tech2Email,
                    Email = tech2Email,
                    FirstName = "John",
                    LastName = "Mechanic",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                var createResult2 = await userManager.CreateAsync(technician2, "Mechanic@2024");
                if (createResult2.Succeeded)
                {
                    await userManager.AddToRoleAsync(technician2, technicianRole);
                    await userManager.AddToRoleAsync(technician2, mechanicRole);
                }
            }
            else
            {
                // Ensure the user is in Technician role
                if (!await userManager.IsInRoleAsync(technician, technicianRole))
                {
                    await userManager.AddToRoleAsync(technician, technicianRole);
                }
                if (!await userManager.IsInRoleAsync(technician, mechanicRole))
                {
                    await userManager.AddToRoleAsync(technician, mechanicRole);
                }
            }
        }

        public static async Task SeedMechanicUserLinkAsync(IServiceProvider sp)
        {
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Ensure mechanics exist to link
            var mechanic1 = await db.Mechanics.FirstOrDefaultAsync(m => m.Name == "Tech One");
            if (mechanic1 == null)
            {
                mechanic1 = new Mechanic { Name = "Tech One", Skills = new List<string> { "repairs", "inspection" }, IsAvailable = true, Rating = 4.5 };
                db.Mechanics.Add(mechanic1);
                await db.SaveChangesAsync();
            }

            var mechanic2 = await db.Mechanics.FirstOrDefaultAsync(m => m.Name == "John Mechanic");
            if (mechanic2 == null)
            {
                mechanic2 = new Mechanic { Name = "John Mechanic", Skills = new List<string> { "maintenance", "repairs" }, IsAvailable = true, Rating = 4.8 };
                db.Mechanics.Add(mechanic2);
                await db.SaveChangesAsync();
            }

            // Link first technician
            var user1 = await userManager.FindByEmailAsync("tech@autoedge.com");
            if (user1 != null && !await db.MechanicUsers.AnyAsync(mu => mu.UserId == user1.Id))
            {
                db.MechanicUsers.Add(new MechanicUser { UserId = user1.Id, MechanicId = mechanic1.Id });
                await db.SaveChangesAsync();
            }

            // Link second technician
            var user2 = await userManager.FindByEmailAsync("mechanic@autoedge.com");
            if (user2 != null && !await db.MechanicUsers.AnyAsync(mu => mu.UserId == user2.Id))
            {
                db.MechanicUsers.Add(new MechanicUser { UserId = user2.Id, MechanicId = mechanic2.Id });
                await db.SaveChangesAsync();
            }
        }
    }
}