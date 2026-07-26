using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Smart_Farm_and_Crop_Yeild_Management_System.Models;
using SmartFarmMVC.Models.ViewModels;

namespace SmartFarmMVC.Controllers
{
    public class FarmerController : Controller
    {
        private readonly SmartFarmDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly Smart_Farm_and_Crop_Yeild_Management_System.Services.IWeatherService _weatherService;

        public FarmerController(SmartFarmDbContext context, IWebHostEnvironment hostEnvironment, Smart_Farm_and_Crop_Yeild_Management_System.Services.IWeatherService weatherService)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            _weatherService = weatherService;
        }

        // Helper check to validate Farmer Session and retrieve Farmer record
        private Smart_Farm_and_Crop_Yeild_Management_System.Models.Farmer? GetActiveFarmer()
        {
            var role = HttpContext.Session.GetString("UserRole");
            var username = HttpContext.Session.GetString("UserUsername");

            if (role != "Farmer" || string.IsNullOrEmpty(username))
            {
                return null;
            }

            return _context.Farmers.Include(f => f.User).FirstOrDefault(f => f.User.Username == username);
        }

        // Helper to extract user initials for avatar
        private string GetInitials(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return "RP";
            string[] parts = fullName.Split(new char[] { ' ', '_' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2) return (parts[0][0].ToString() + parts[1][0].ToString()).ToUpper();
            if (parts.Length == 1 && parts[0].Length >= 2) return parts[0].Substring(0, 2).ToUpper();
            return "RP";
        }

        // GET: /Farmer/Dashboard
        public IActionResult Dashboard()
        {
            var farmer = GetActiveFarmer();
            if (farmer == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            string name = farmer.FullName;
            string initials = HttpContext.Session.GetString("UserInitials") ?? GetInitials(name);
            string firstName = name.Split(' ')[0];

            ViewData["Title"] = "Farmer Dashboard";
            ViewData["Subtitle"] = $"Welcome back, {firstName}! Here's your farm overview for today.";
            ViewData["UserRole"] = "Farmer";
            ViewData["UserName"] = name;
            ViewData["UserInitials"] = initials;
            ViewData["RoleColor"] = "#40916C";

            // Database dynamic metrics
            int farmerId = farmer.FarmerId;
            ViewBag.TotalFarms = _context.Farms.Count(f => f.FarmerId == farmerId);

            var farmIds = _context.Farms.Where(f => f.FarmerId == farmerId).Select(f => f.FarmId).ToList();
            ViewBag.TotalPlots = _context.LandPlots.Count(p => farmIds.Contains(p.FarmId));

            var plotIds = _context.LandPlots.Where(p => farmIds.Contains(p.FarmId)).Select(p => p.PlotId).ToList();
            ViewBag.ActiveCycles = _context.CropCycles.Count(c => plotIds.Contains(c.PlotId) && c.Status == "Active");

            var cycleIds = _context.CropCycles.Where(c => plotIds.Contains(c.PlotId)).Select(c => c.CropCycleId).ToList();
            ViewBag.PendingPests = _context.PestCases.Count(p => cycleIds.Contains(p.CropCycleId) && p.Status != "Resolved");

            // Crop cycle harvesting within 7 days
            ViewBag.PendingHarvests = _context.CropCycles.Count(c => plotIds.Contains(c.PlotId) && c.Status == "Active" && c.ExpectedHarvestDate <= DateTime.Today.AddDays(7));

            var harvestIds = _context.Harvests.Where(h => cycleIds.Contains(h.CropCycleId)).Select(h => h.HarvestId).ToList();
            ViewBag.MarketplaceListings = _context.CropListings.Count(l => harvestIds.Contains(l.HarvestId) && l.Status == "Available");

            // Seed default notification if none present
            if (!_context.Notifications.Any(n => n.UserId == farmer.UserId))
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = farmer.UserId,
                    Title = "Welcome to Smart Farm!",
                    Message = "Your digital profile is successfully configured. Map your first farm plots to begin weather tracking and crop cycles.",
                    CreatedDate = DateTime.Now,
                    IsRead = false
                });
                _context.SaveChanges();
            }

            // Fetch notifications
            ViewBag.RecentNotifications = _context.Notifications
                .Where(n => n.UserId == farmer.UserId)
                .OrderByDescending(n => n.CreatedDate)
                .Take(5)
                .ToList();

            // Fetch latest crop monitoring log
            ViewBag.LatestMonitoring = _context.CropMonitorings
                .Include(m => m.CropCycle)
                    .ThenInclude(c => c.Crop)
                .Where(m => cycleIds.Contains(m.CropCycleId))
                .OrderByDescending(m => m.ObservationDate)
                .FirstOrDefault();

            // Fetch all farms with their first plot coordinates for weather widgets
            var farmsData = _context.Farms
                .Include(f => f.LandPlots)
                .Where(f => f.FarmerId == farmerId)
                .ToList();

            var farmsWithWeather = farmsData
                .Where(f => f.LandPlots != null && f.LandPlots.Any())
                .Select(f =>
                {
                    var firstPlot = f.LandPlots.FirstOrDefault();
                    return new
                    {
                        FarmId = f.FarmId,
                        FarmName = f.FarmName,
                        FirstPlot = firstPlot != null ? new
                        {
                            PlotName = firstPlot.PlotName,
                            Latitude = firstPlot.Latitude,
                            Longitude = firstPlot.Longitude
                        } : null
                    };
                })
                .Where(f => f.FirstPlot != null)
                .ToList();

            // Debug logging
            System.Diagnostics.Debug.WriteLine($"Total farms found: {farmsData.Count}");
            System.Diagnostics.Debug.WriteLine($"Farms with plots: {farmsWithWeather.Count}");
            foreach (var farm in farmsWithWeather)
            {
                System.Diagnostics.Debug.WriteLine($"Farm: {farm.FarmName}, Plot: {farm.FirstPlot?.PlotName}, Lat: {farm.FirstPlot?.Latitude}, Lng: {farm.FirstPlot?.Longitude}");
            }

            ViewBag.FarmsWithWeather = farmsWithWeather;

            // Determine current season based on month for crop recommendations
            int currentMonth = DateTime.Now.Month;
            string currentSeason = "";
            string seasonDescription = "";

            if (currentMonth >= 6 && currentMonth <= 10)
            {
                currentSeason = "Kharif";
                seasonDescription = "Monsoon Season (June - October)";
            }
            else if (currentMonth >= 11 || currentMonth <= 2)
            {
                currentSeason = "Rabi";
                seasonDescription = "Winter Season (November - February)";
            }
            else
            {
                currentSeason = "Zaid";
                seasonDescription = "Spring/Summer Season (March - May)";
            }

            ViewBag.CurrentSeason = currentSeason;
            ViewBag.SeasonDescription = seasonDescription;
            ViewBag.CurrentMonth = currentMonth;

            return View();
        }

        // GET: /Farmer/Profile
        [HttpGet]
        public IActionResult Profile()
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var model = new FarmerProfileViewModel
            {
                FullName = farmer.FullName,
                MobileNumber = farmer.MobileNumber,
                Address = farmer.Address,
                Village = farmer.Village,
                Taluka = farmer.Taluka,
                District = farmer.District,
                State = farmer.State,
                PinCode = farmer.PinCode,
                Gender = farmer.Gender,
                DateOfBirth = farmer.DateOfBirth,
                EmergencyContact = farmer.EmergencyContact
            };

            ViewBag.ProfilePicturePath = farmer.ProfilePicturePath;
            ViewData["UserName"] = farmer.FullName;
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials") ?? GetInitials(farmer.FullName);
            ViewData["UserRole"] = "Farmer";

            return View(model);
        }

        // POST: /Farmer/Profile
        [HttpPost]
        public async Task<IActionResult> Profile(FarmerProfileViewModel model)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                ViewBag.ProfilePicturePath = farmer.ProfilePicturePath;
                ViewData["UserName"] = farmer.FullName;
                ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials") ?? GetInitials(farmer.FullName);
                ViewData["UserRole"] = "Farmer";
                return View(model);
            }

            try
            {
                // Update demographics
                farmer.FullName = model.FullName.Trim();
                farmer.MobileNumber = model.MobileNumber.Trim();
                farmer.Address = model.Address?.Trim();
                farmer.Village = model.Village?.Trim();
                farmer.Taluka = model.Taluka?.Trim();
                farmer.District = model.District?.Trim();
                farmer.State = model.State?.Trim();
                farmer.PinCode = model.PinCode?.Trim();
                farmer.Gender = model.Gender?.Trim();
                farmer.DateOfBirth = model.DateOfBirth;
                farmer.EmergencyContact = model.EmergencyContact?.Trim();

                // Handle file upload for profile picture
                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ProfilePicture.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfilePicture.CopyToAsync(fileStream);
                    }

                    farmer.ProfilePicturePath = "/uploads/" + uniqueFileName;
                }

                // Handle password update if provided
                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    var user = _context.Users.FirstOrDefault(u => u.UserId == farmer.UserId);
                    if (user == null)
                    {
                        ViewData["ErrorMessage"] = "User account not found.";
                        ViewBag.ProfilePicturePath = farmer.ProfilePicturePath;
                        return View(model);
                    }

                    if (user.PasswordHash != model.CurrentPassword)
                    {
                        ModelState.AddModelError("CurrentPassword", "Current password does not match.");
                        ViewBag.ProfilePicturePath = farmer.ProfilePicturePath;
                        ViewData["UserName"] = farmer.FullName;
                        ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials") ?? GetInitials(farmer.FullName);
                        ViewData["UserRole"] = "Farmer";
                        return View(model);
                    }

                    user.PasswordHash = model.NewPassword;
                }

                _context.SaveChanges();

                // Update session
                HttpContext.Session.SetString("UserName", farmer.FullName);
                HttpContext.Session.SetString("UserInitials", GetInitials(farmer.FullName));

                TempData["SuccessMessage"] = "Profile updated successfully.";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "Error updating profile: " + ex.Message;
                ViewBag.ProfilePicturePath = farmer.ProfilePicturePath;
                ViewData["UserName"] = farmer.FullName;
                ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials") ?? GetInitials(farmer.FullName);
                ViewData["UserRole"] = "Farmer";
                return View(model);
            }
        }

        // GET: /Farmer/PestCaseApprovals  — list pest cases waiting for farmer approval
        [HttpGet]
        public IActionResult PestCaseApprovals()
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            ViewData["UserName"] = farmer.FullName;
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials") ?? GetInitials(farmer.FullName);
            ViewData["UserRole"] = "Farmer";

            int farmerId = farmer.FarmerId;
            var farmIds  = _context.Farms.Where(f => f.FarmerId == farmerId).Select(f => f.FarmId).ToList();
            var plotIds  = _context.LandPlots.Where(p => farmIds.Contains(p.FarmId)).Select(p => p.PlotId).ToList();
            var cycleIds = _context.CropCycles.Where(c => plotIds.Contains(c.PlotId)).Select(c => c.CropCycleId).ToList();

            var pending = _context.PestCases
                .Include(p => p.CropCycle)
                    .ThenInclude(c => c.Crop)
                .Include(p => p.CropCycle)
                    .ThenInclude(c => c.LandPlot)
                .Include(p => p.AssignedOfficer)
                .Where(p => cycleIds.Contains(p.CropCycleId) && p.Status == "Pending Farmer Approval")
                .OrderByDescending(p => p.FieldVisitCompletedDate)
                .ToList();

            return View(pending);
        }

        // POST: /Farmer/ApprovePestCase
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApprovePestCase(int pestCaseId)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var pestCase = _context.PestCases
                .Include(p => p.AssignedOfficer)
                .FirstOrDefault(p => p.PestCaseId == pestCaseId);

            if (pestCase == null)
            {
                TempData["ErrorMessage"] = "Pest case not found.";
                return RedirectToAction("PestCaseApprovals");
            }

            pestCase.Status      = "Resolved";
            pestCase.IsClosed    = true;
            pestCase.ResolvedDate = DateTime.Now;
            pestCase.ClosedDate  = DateTime.Now;
            pestCase.FarmerResponseToReport = "Approved";
            pestCase.FarmerResponseDate = DateTime.Now;

            // Notify the field officer that farmer approved
            if (pestCase.AssignedOfficer != null)
            {
                _context.Notifications.Add(new Smart_Farm_and_Crop_Yeild_Management_System.Models.Notification
                {
                    UserId      = pestCase.AssignedOfficer.UserId,
                    Title       = "Farmer Approved — Case Resolved",
                    Message     = $"Farmer {farmer.FullName} has approved your field report for incident #{pestCaseId}. The case is now marked as Resolved.",
                    IsRead      = false,
                    CreatedDate = DateTime.Now
                });
            }

            _context.SaveChanges();
            TempData["SuccessMessage"] = "You have approved the resolution. The issue is now marked as Resolved.";
            return RedirectToAction("PestCaseApprovals");
        }

        // POST: /Farmer/RejectPestCase
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectPestCase(int pestCaseId, string rejectionReason)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var pestCase = _context.PestCases
                .Include(p => p.AssignedOfficer)
                .FirstOrDefault(p => p.PestCaseId == pestCaseId);

            if (pestCase == null)
            {
                TempData["ErrorMessage"] = "Pest case not found.";
                return RedirectToAction("PestCaseApprovals");
            }

            // Escalate back to officer for re-visit
            pestCase.Status = "ESCALATED TO OFFICER";
            pestCase.FarmerResponseToReport = "Rejected";
            pestCase.FarmerResponseDate = DateTime.Now;
            pestCase.FieldReport = (pestCase.FieldReport ?? "") + $"\n\n[Farmer Rejection Reason: {rejectionReason}]";

            // Notify the officer
            if (pestCase.AssignedOfficer != null)
            {
                _context.Notifications.Add(new Smart_Farm_and_Crop_Yeild_Management_System.Models.Notification
                {
                    UserId      = pestCase.AssignedOfficer.UserId,
                    Title       = "Farmer Rejected Resolution — Re-visit Required",
                    Message     = $"Farmer {farmer.FullName} rejected your field report for incident #{pestCaseId}. Reason: {rejectionReason}. Please schedule a follow-up visit.",
                    IsRead      = false,
                    CreatedDate = DateTime.Now
                });
            }

            _context.SaveChanges();
            TempData["WarningMessage"] = "You have rejected the resolution. The field officer will be notified to re-visit.";
            return RedirectToAction("PestCaseApprovals");
        }

        // GET: /Farmer/Reports
        [HttpGet]
        public IActionResult Reports()
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            int farmerId = farmer.FarmerId;

            // Fetch Farm and Plot list
            var farmIds = _context.Farms.Where(f => f.FarmerId == farmerId).Select(f => f.FarmId).ToList();
            var plotIds = _context.LandPlots.Where(p => farmIds.Contains(p.FarmId)).Select(p => p.PlotId).ToList();
            var cycleIds = _context.CropCycles.Where(c => plotIds.Contains(c.PlotId)).Select(c => c.CropCycleId).ToList();

            // Yield Weight grouped by Crop Type
            var harvestedCrops = _context.Harvests
                .Include(h => h.CropCycle)
                    .ThenInclude(c => c.Crop)
                .Where(h => cycleIds.Contains(h.CropCycleId))
                .AsEnumerable()
                .GroupBy(h => h.CropCycle.Crop.CropName)
                .Select(g => new {
                    CropName = g.Key,
                    TotalYield = g.Sum(h => h.ActualQuantity),
                    Unit = g.FirstOrDefault()?.Unit ?? "Quintal"
                })
                .ToList();

            ViewBag.YieldSummary = harvestedCrops;

            // Total Financial Sales from orders & listings
            var harvestIds = _context.Harvests.Where(h => cycleIds.Contains(h.CropCycleId)).Select(h => h.HarvestId).ToList();
            var paidStatuses = new[] { "Paid", "Completed", "Delivered", "Sold" };
            var paidOrdersSales = _context.CropOrders
                .Where(o => o.FarmerId == farmerId && paidStatuses.Contains(o.Status))
                .Sum(o => (decimal?)o.TotalAmount) ?? 0m;
            var listingSales = _context.CropListings
                .Where(l => harvestIds.Contains(l.HarvestId) && (l.Status == "Sold" || (l.PurchasedQuantity != null && l.PurchasedQuantity > 0)))
                .Sum(l => (decimal?)(l.PricePerUnit * (l.PurchasedQuantity ?? l.AvailableQuantity))) ?? 0m;
            ViewBag.TotalSales = Math.Max(paidOrdersSales, listingSales);

            // Log table of all cycles
            var allCycles = _context.CropCycles
                .Include(c => c.Crop)
                .Include(c => c.LandPlot)
                .Where(c => plotIds.Contains(c.PlotId))
                .OrderByDescending(c => c.SowingDate)
                .ToList();
            ViewBag.CyclesLog = allCycles;

            // Active plots list
            var activePlots = _context.LandPlots
                .Include(p => p.Farm)
                .Where(p => farmIds.Contains(p.FarmId))
                .ToList();
            ViewBag.PlotsList = activePlots;

            ViewData["UserName"] = farmer.FullName;
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials") ?? GetInitials(farmer.FullName);
            ViewData["UserRole"] = "Farmer";

            return View();
        }
    }
}
