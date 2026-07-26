using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using Smart_Farm_and_Crop_Yeild_Management_System.Models;
using SmartFarmMVC.Models.ViewModels;
using SmartFarmMVC.Models;
using System.Threading.Tasks;

namespace SmartFarmMVC.Controllers
{
    public class CropMonitoringController : Controller
    {
        private readonly SmartFarmDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly Smart_Farm_and_Crop_Yeild_Management_System.Services.IWeatherService _weatherService;

        public CropMonitoringController(SmartFarmDbContext context, IWebHostEnvironment environment, Smart_Farm_and_Crop_Yeild_Management_System.Services.IWeatherService weatherService)
        {
            _context = context;
            _environment = environment;
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

            return _context.Farmers.FirstOrDefault(f => f.User.Username == username);
        }

        // GET: /CropMonitoring
        // List observations for the farmer
        public async Task<IActionResult> Index()
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var monitorings = _context.CropMonitorings
                .Include(m => m.CropCycle)
                    .ThenInclude(c => c.Crop)
                .Include(m => m.CropCycle)
                    .ThenInclude(c => c.LandPlot)
                .Where(m => m.CropCycle.LandPlot.Farm.FarmerId == farmer.FarmerId)
                .OrderByDescending(m => m.ObservationDate)
                .ToList();

            // If there are no monitorings yet, try to fetch live weather and telemetry for the farmer's default plot
            if (!monitorings.Any())
            {
                try
                {
                    // Find a default plot for this farmer
                    var farmIds = _context.Farms.Where(f => f.FarmerId == farmer.FarmerId).Select(f => f.FarmId).ToList();
                    var firstPlot = _context.LandPlots.FirstOrDefault(p => farmIds.Contains(p.FarmId));
                    if (firstPlot != null)
                    {
                        // Fetch weather via injected service (if available)
                        try
                        {
                            var weather = await _weatherService.GetWeatherAsync(firstPlot.Latitude, firstPlot.Longitude, false);
                            ViewBag.Weather = weather;
                        }
                        catch
                        {
                            ViewBag.Weather = null;
                        }

                        // Simulate or fetch telemetry reading
                        try
                        {
                            var reading = await TelemetrySimulator.SimulateReadingAsync(firstPlot.PlotId, firstPlot.Latitude, firstPlot.Longitude, _context);
                            ViewBag.SensorReading = reading;
                        }
                        catch
                        {
                            ViewBag.SensorReading = null;
                        }
                    }
                    else
                    {
                        ViewBag.Weather = null;
                        ViewBag.SensorReading = null;
                    }
                }
                catch
                {
                    ViewBag.Weather = null;
                    ViewBag.SensorReading = null;
                }
            }

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Farmer";

            return View(monitorings);
        }

        // GET: /CropMonitoring/Details/{id}
        // Shows observation details and live weather for the plot
        public IActionResult Details(int id)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var monitoring = _context.CropMonitorings
                .Include(m => m.CropCycle)
                    .ThenInclude(c => c.Crop)
                .Include(m => m.CropCycle)
                    .ThenInclude(c => c.LandPlot)
                .FirstOrDefault(m => m.MonitoringId == id && m.CropCycle.LandPlot.Farm.FarmerId == farmer.FarmerId);

            if (monitoring == null)
            {
                TempData["ErrorMessage"] = "Monitoring log not found.";
                return RedirectToAction("Index");
            }

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Farmer";
            return View(monitoring);
        }



        // GET: /CropMonitoring/Create
        public IActionResult Create(int? cycleId)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var cycles = _context.CropCycles
                .Include(c => c.Crop)
                .Include(c => c.LandPlot)
                .Where(c => c.LandPlot.Farm.FarmerId == farmer.FarmerId && c.Status == "Active")
                .ToList();

            if (cycles.Count == 0)
            {
                TempData["ErrorMessage"] = "You must have an active crop cycle to add monitoring observations.";
                return RedirectToAction("Index", "CropCycle");
            }

            ViewBag.Cycles = cycles;

            var model = new CropMonitoringViewModel();
            if (cycleId.HasValue)
            {
                model.CropCycleId = cycleId.Value;
            }

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Farmer";

            return View(model);
        }

        // POST: /CropMonitoring/Create
        [HttpPost]
        public IActionResult Create(CropMonitoringViewModel model)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var cycles = _context.CropCycles
                .Include(c => c.Crop)
                .Include(c => c.LandPlot)
                .Where(c => c.LandPlot.Farm.FarmerId == farmer.FarmerId && c.Status == "Active")
                .ToList();

            ViewBag.Cycles = cycles;

            if (!cycles.Any(c => c.CropCycleId == model.CropCycleId))
            {
                ModelState.AddModelError("CropCycleId", "Invalid crop cycle selection.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["UserName"] = HttpContext.Session.GetString("UserName");
                ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
                ViewData["UserRole"] = "Farmer";
                return View(model);
            }

            try
            {
                string? relativePath = null;

                // Handle Image File Upload
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        model.ImageFile.CopyTo(fileStream);
                    }

                    relativePath = "/uploads/" + uniqueFileName;
                }

                var monitoring = new CropMonitoring
                {
                    CropCycleId = model.CropCycleId,
                    ObservationDate = model.ObservationDate,
                    GrowthStage = model.GrowthStage.Trim(),
                    PlantHeight = model.PlantHeight,
                    CropHealth = model.CropHealth.Trim(),
                    Remarks = model.Remarks?.Trim(),
                    ImagePath = relativePath
                };

                _context.CropMonitorings.Add(monitoring);

                // Update current stage on crop cycle
                var activeCycle = _context.CropCycles.Find(model.CropCycleId);
                if (activeCycle != null)
                {
                    activeCycle.CurrentStage = model.GrowthStage.Trim();
                }

                _context.SaveChanges();

                TempData["SuccessMessage"] = "Growth monitoring observation logged successfully.";
                return RedirectToAction("Details", "CropCycle", new { id = model.CropCycleId });
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "Error logging observation: " + ex.Message;
                ViewData["UserName"] = HttpContext.Session.GetString("UserName");
                ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
                ViewData["UserRole"] = "Farmer";
                return View(model);
            }
        }
    }
}
