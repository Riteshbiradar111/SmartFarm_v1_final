using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Smart_Farm_and_Crop_Yeild_Management_System.Models;
using SmartFarmMVC.Models.ViewModels;
using SmartFarmMVC.Models;
using Smart_Farm_and_Crop_Yeild_Management_System.Services;
using System.Threading.Tasks;

namespace SmartFarmMVC.Controllers
{
    public class LandPlotController : Controller
    {
        private readonly SmartFarmDbContext _context;
        private readonly IWeatherService _weatherService;

        public LandPlotController(SmartFarmDbContext context, IWeatherService weatherService)
        {
            _context = context;
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

        // GET: /LandPlot
        // List all plots belonging to the logged-in Farmer's farms
        public IActionResult Index()
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var plots = _context.LandPlots
                .Include(p => p.Farm)
                .Where(p => p.Farm.FarmerId == farmer.FarmerId)
                .ToList();

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Farmer";

            return View(plots);
        }

        // GET: /LandPlot/Details/{id}
        // Show Plot details and weather metrics
        public IActionResult Details(int id)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var plot = _context.LandPlots
                .Include(p => p.Farm)
                .Include(p => p.CropCycles)
                    .ThenInclude(c => c.Crop)
                .FirstOrDefault(p => p.PlotId == id && p.Farm.FarmerId == farmer.FarmerId);

            if (plot == null)
            {
                TempData["ErrorMessage"] = "Land plot not found.";
                return RedirectToAction("Index");
            }

            // Fetch latest sensor readings for this plot
            var sensorReading = _context.SensorReadings.FirstOrDefault(s => s.PlotId == id);
            ViewBag.SensorReading = sensorReading;

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Farmer";

            return View(plot);
        }

        // GET: /LandPlot/Create
        public IActionResult Create(int? farmId)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var farms = _context.Farms.Where(f => f.FarmerId == farmer.FarmerId).ToList();
            if (farms.Count == 0)
            {
                TempData["ErrorMessage"] = "You must create a farm first before adding land plots.";
                return RedirectToAction("Create", "Farm");
            }

            ViewBag.Farms = farms;

            var model = new LandPlotViewModel();
            if (farmId.HasValue)
            {
                model.FarmId = farmId.Value;
            }

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Farmer";

            return View(model);
        }

        // POST: /LandPlot/Create
        [HttpPost]
        public async Task<IActionResult> Create(LandPlotViewModel model)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var farms = _context.Farms.Where(f => f.FarmerId == farmer.FarmerId).ToList();
            ViewBag.Farms = farms;

            // Check if selected farm belongs to this farmer
            if (!farms.Any(f => f.FarmId == model.FarmId))
            {
                ModelState.AddModelError("FarmId", "Invalid farm selection.");
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
                var plot = new LandPlot
                {
                    FarmId = model.FarmId,
                    PlotName = model.PlotName.Trim(),
                    PlotCode = model.PlotCode.Trim(),
                    Area = model.Area,
                    AreaUnit = model.AreaUnit,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude,
                    SoilType = model.SoilType.Trim(),
                    IrrigationType = model.IrrigationType.Trim(),
                    Status = model.Status
                };

                _context.LandPlots.Add(plot);
                _context.SaveChanges();

                // Trigger IoT telemetry simulator on plot creation
                try
                {
                    await TelemetrySimulator.SimulateReadingAsync(plot.PlotId, plot.Latitude, plot.Longitude, _context);
                }
                catch (Exception simEx)
                {
                    Console.WriteLine($"[LandPlotController] Telemetry simulation failed during plot creation: {simEx.Message}");
                }

                TempData["SuccessMessage"] = "Land plot added successfully. Telemetry has been simulated based on live weather data.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "Error adding land plot: " + ex.Message;
                ViewData["UserName"] = HttpContext.Session.GetString("UserName");
                ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
                ViewData["UserRole"] = "Farmer";
                return View(model);
            }
        }

        // GET: /LandPlot/Edit/{id}
        public IActionResult Edit(int id)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var plot = _context.LandPlots
                .Include(p => p.Farm)
                .FirstOrDefault(p => p.PlotId == id && p.Farm.FarmerId == farmer.FarmerId);

            if (plot == null)
            {
                TempData["ErrorMessage"] = "Land plot not found.";
                return RedirectToAction("Index");
            }

            var farms = _context.Farms.Where(f => f.FarmerId == farmer.FarmerId).ToList();
            ViewBag.Farms = farms;

            var model = new LandPlotViewModel
            {
                FarmId = plot.FarmId,
                PlotName = plot.PlotName,
                PlotCode = plot.PlotCode,
                Area = plot.Area,
                AreaUnit = plot.AreaUnit,
                Latitude = plot.Latitude,
                Longitude = plot.Longitude,
                SoilType = plot.SoilType ?? "",
                IrrigationType = plot.IrrigationType ?? "",
                Status = plot.Status ?? "Active"
            };

            ViewData["PlotId"] = plot.PlotId;
            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Farmer";

            return View(model);
        }

        // POST: /LandPlot/Edit/{id}
        [HttpPost]
        public IActionResult Edit(int id, LandPlotViewModel model)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var plot = _context.LandPlots
                .Include(p => p.Farm)
                .FirstOrDefault(p => p.PlotId == id && p.Farm.FarmerId == farmer.FarmerId);

            if (plot == null)
            {
                TempData["ErrorMessage"] = "Land plot not found.";
                return RedirectToAction("Index");
            }

            var farms = _context.Farms.Where(f => f.FarmerId == farmer.FarmerId).ToList();
            ViewBag.Farms = farms;

            if (!farms.Any(f => f.FarmId == model.FarmId))
            {
                ModelState.AddModelError("FarmId", "Invalid farm selection.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["PlotId"] = plot.PlotId;
                ViewData["UserName"] = HttpContext.Session.GetString("UserName");
                ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
                ViewData["UserRole"] = "Farmer";
                return View(model);
            }

            try
            {
                plot.FarmId = model.FarmId;
                plot.PlotName = model.PlotName.Trim();
                plot.PlotCode = model.PlotCode.Trim();
                plot.Area = model.Area;
                plot.AreaUnit = model.AreaUnit;
                plot.Latitude = model.Latitude;
                plot.Longitude = model.Longitude;
                plot.SoilType = model.SoilType.Trim();
                plot.IrrigationType = model.IrrigationType.Trim();
                plot.Status = model.Status;

                _context.SaveChanges();
                TempData["SuccessMessage"] = "Land plot updated successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "Error updating land plot: " + ex.Message;
                ViewData["PlotId"] = plot.PlotId;
                ViewData["UserName"] = HttpContext.Session.GetString("UserName");
                ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
                ViewData["UserRole"] = "Farmer";
                return View(model);
            }
        }

        // GET: /LandPlot/Delete/{id}
        public IActionResult Delete(int id)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var plot = _context.LandPlots
                .Include(p => p.Farm)
                .FirstOrDefault(p => p.PlotId == id && p.Farm.FarmerId == farmer.FarmerId);

            if (plot == null)
            {
                TempData["ErrorMessage"] = "Land plot not found.";
                return RedirectToAction("Index");
            }

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Farmer";

            return View(plot);
        }

        // POST: /LandPlot/Delete/{id}
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var plot = _context.LandPlots
                .Include(p => p.Farm)
                .Include(p => p.CropCycles)
                    .ThenInclude(c => c.CropMonitorings)
                .Include(p => p.CropCycles)
                    .ThenInclude(c => c.Harvests)
                .Include(p => p.CropCycles)
                    .ThenInclude(c => c.PestCases)
                .FirstOrDefault(p => p.PlotId == id && p.Farm.FarmerId == farmer.FarmerId);

            if (plot == null)
            {
                TempData["ErrorMessage"] = "Land plot not found.";
                return RedirectToAction("Index");
            }

            try
            {
                // Delete crop cycle related records
                foreach (var cycle in plot.CropCycles.ToList())
                {
                    // Delete harvest-related records first
                    foreach (var harvest in cycle.Harvests.ToList())
                    {
                        // Get all listings for this harvest
                        var cropListings = _context.CropListings.Where(cl => cl.HarvestId == harvest.HarvestId).ToList();

                        // Delete crop orders linked to these listings first
                        foreach (var listing in cropListings)
                        {
                            var ordersForListing = _context.CropOrders.Where(co => co.ListingId == listing.ListingId);
                            _context.CropOrders.RemoveRange(ordersForListing);
                        }

                        // Delete crop orders directly linked to harvest (pre-orders)
                        var ordersForHarvest = _context.CropOrders.Where(co => co.HarvestId == harvest.HarvestId);
                        _context.CropOrders.RemoveRange(ordersForHarvest);

                        // Now delete the crop listings
                        _context.CropListings.RemoveRange(cropListings);
                    }

                    // Delete harvests
                    _context.Harvests.RemoveRange(cycle.Harvests);

                    // Delete other crop cycle records
                    _context.CropMonitorings.RemoveRange(cycle.CropMonitorings);
                    _context.PestCases.RemoveRange(cycle.PestCases);
                }

                // Delete crop cycles
                _context.CropCycles.RemoveRange(plot.CropCycles);

                // Delete sensor readings for this plot
                var sensorReadings = _context.SensorReadings.Where(sr => sr.PlotId == plot.PlotId);
                _context.SensorReadings.RemoveRange(sensorReadings);

                // Delete the land plot
                _context.LandPlots.Remove(plot);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Land plot deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting land plot: " + (ex.InnerException?.Message ?? ex.Message);
            }

            return RedirectToAction("Index");
        }

        // POST: /LandPlot/RefreshTelemetry/{id}
        [HttpPost]
        public async Task<IActionResult> RefreshTelemetry(int id)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var plot = _context.LandPlots
                .Include(p => p.Farm)
                .FirstOrDefault(p => p.PlotId == id && p.Farm.FarmerId == farmer.FarmerId);

            if (plot == null)
            {
                TempData["ErrorMessage"] = "Land plot not found.";
                return RedirectToAction("Index");
            }

            try
            {
                await TelemetrySimulator.SimulateReadingAsync(plot.PlotId, plot.Latitude, plot.Longitude, _context);
                TempData["SuccessMessage"] = "Telemetry data refreshed successfully using real-time weather data!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error refreshing telemetry: " + ex.Message;
            }

            return RedirectToAction("Details", new { id = plot.PlotId });
        }

        // GET: /LandPlot/SoilWeather/{id?}
        // Display environmental conditions, weather forecast, and soil metrics for a selected land plot
        public async Task<IActionResult> SoilWeather(int? id)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var farms = _context.Farms
                .Include(f => f.LandPlots)
                .Where(f => f.FarmerId == farmer.FarmerId)
                .OrderBy(f => f.FarmName) // Order farms alphabetically
                .ToList();

            // Order plots within each farm for consistent display
            foreach (var farm in farms)
            {
                farm.LandPlots = farm.LandPlots.OrderBy(p => p.PlotCode).ToList();
            }

            if (farms.Count == 0)
            {
                TempData["ErrorMessage"] = "You must create a farm and a land plot first to view Soil & Weather analytics.";
                return RedirectToAction("Index");
            }

            // Default to the first farm if no ID is specified
            var selectedFarm = id.HasValue
                ? farms.FirstOrDefault(f => f.FarmId == id.Value)
                : farms.FirstOrDefault();

            if (selectedFarm == null)
            {
                selectedFarm = farms.FirstOrDefault();
            }

            // For each plot in the selected farm, fetch latest sensor reading and weather
            var plotReadings = new System.Collections.Generic.Dictionary<int, SensorReading?>();
            var plotWeather = new System.Collections.Generic.Dictionary<int, Smart_Farm_and_Crop_Yeild_Management_System.Models.ViewModels.WeatherViewModel?>();

            foreach (var plot in selectedFarm.LandPlots)
            {
                var reading = _context.SensorReadings.FirstOrDefault(s => s.PlotId == plot.PlotId);
                plotReadings[plot.PlotId] = reading;

                try
                {
                    var weather = await _weatherService.GetWeatherAsync(plot.Latitude, plot.Longitude, false);
                    plotWeather[plot.PlotId] = weather;
                }
                catch
                {
                    plotWeather[plot.PlotId] = null;
                }
            }

            ViewBag.Farms = farms;
            ViewBag.SelectedFarmId = selectedFarm?.FarmId;
            ViewBag.PlotReadings = plotReadings;
            ViewBag.PlotWeather = plotWeather;

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Farmer";

            return View(selectedFarm);
        }

        // POST: /LandPlot/RefreshSoilData/{id}
        [HttpPost]
        public async Task<IActionResult> RefreshSoilData(int id)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var plot = _context.LandPlots
                .Include(p => p.Farm)
                .FirstOrDefault(p => p.PlotId == id && p.Farm.FarmerId == farmer.FarmerId);

            if (plot == null)
            {
                TempData["ErrorMessage"] = "Land plot not found.";
                return RedirectToAction("Index");
            }

            try
            {
                await TelemetrySimulator.SimulateReadingAsync(plot.PlotId, plot.Latitude, plot.Longitude, _context);
                TempData["SuccessMessage"] = $"Soil telemetry for plot '{plot.PlotName}' refreshed successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error simulating soil telemetry: " + ex.Message;
            }

            return RedirectToAction("SoilWeather", new { id = plot.PlotId });
        }

        // POST: /LandPlot/RefreshWeather/{id}
        [HttpPost]
        public async Task<IActionResult> RefreshWeather(int id)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var plot = _context.LandPlots
                .Include(p => p.Farm)
                .FirstOrDefault(p => p.PlotId == id && p.Farm.FarmerId == farmer.FarmerId);

            if (plot == null)
            {
                TempData["ErrorMessage"] = "Land plot not found.";
                return RedirectToAction("Index");
            }

            try
            {
                // Force refresh weather cache
                await _weatherService.GetWeatherAsync(plot.Latitude, plot.Longitude, true);
                TempData["SuccessMessage"] = $"Weather refreshed for plot '{plot.PlotName}'.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error refreshing weather: " + ex.Message;
            }

            return RedirectToAction("SoilWeather", new { id = plot.PlotId });
        }


    }
}
