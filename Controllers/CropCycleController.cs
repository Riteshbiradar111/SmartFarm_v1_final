using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Smart_Farm_and_Crop_Yeild_Management_System.Models;
using SmartFarmMVC.Models.ViewModels;

namespace SmartFarmMVC.Controllers
{
    public class CropCycleController : Controller
    {
        private readonly SmartFarmDbContext _context;

        public CropCycleController(SmartFarmDbContext context)
        {
            _context = context;
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

        // Seeds master crop data if it is empty
        private void SeedCropsIfEmpty()
        {
            if (!_context.Crops.Any())
            {
                _context.Crops.AddRange(
                    new Crop { CropName = "Wheat", Season = "Rabi", DurationDays = 120, Description = "High-yield standard grain." },
                    new Crop { CropName = "Cotton", Season = "Kharif", DurationDays = 180, Description = "Bt Cotton variety." },
                    new Crop { CropName = "Onion", Season = "Rabi", DurationDays = 100, Description = "Nasik Red variety." },
                    new Crop { CropName = "Soybean", Season = "Kharif", DurationDays = 110, Description = "JS-335 high protein bean." },
                    new Crop { CropName = "Sugarcane", Season = "Annual", DurationDays = 360, Description = "Co-86032 cash crop." },
                    new Crop { CropName = "Tomato", Season = "Zaid", DurationDays = 90, Description = "Hybrid yielding tomato." }
                );
                _context.SaveChanges();
            }
        }

        // GET: /CropCycle
        // List all active and past crop cycles for the logged-in Farmer
        public IActionResult Index()
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var cycles = _context.CropCycles
                .Include(c => c.LandPlot)
                    .ThenInclude(p => p.Farm)
                .Include(c => c.Crop)
                .Where(c => c.LandPlot.Farm.FarmerId == farmer.FarmerId)
                .ToList();

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Farmer";

            return View(cycles);
        }

        // GET: /CropCycle/Details/{id}
        public IActionResult Details(int id)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var cycle = _context.CropCycles
                .Include(c => c.LandPlot)
                    .ThenInclude(p => p.Farm)
                .Include(c => c.Crop)
                .Include(c => c.CropMonitorings)
                .Include(c => c.PestCases)
                .Include(c => c.Harvests)
                .FirstOrDefault(c => c.CropCycleId == id && c.LandPlot.Farm.FarmerId == farmer.FarmerId);

            if (cycle == null)
            {
                TempData["ErrorMessage"] = "Crop cycle not found.";
                return RedirectToAction("Index");
            }

            var sensorReading = _context.SensorReadings.FirstOrDefault(s => s.PlotId == cycle.PlotId);
            ViewBag.SensorReading = sensorReading;

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Farmer";

            return View(cycle);
        }

        // GET: /CropCycle/Create
        public IActionResult Create(int? plotId)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            SeedCropsIfEmpty();

            var plots = _context.LandPlots
                .Include(p => p.Farm)
                .Where(p => p.Farm.FarmerId == farmer.FarmerId)
                .ToList();
            if (plots.Count == 0)
            {
                TempData["ErrorMessage"] = "You must create a land plot first before adding crop cycles.";
                return RedirectToAction("Create", "LandPlot");
            }

            ViewBag.Plots = plots;
            ViewBag.Crops = _context.Crops.ToList();

            var model = new CropCycleViewModel();
            if (plotId.HasValue)
            {
                model.PlotId = plotId.Value;
            }

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Farmer";

            return View(model);
        }

        // POST: /CropCycle/Create
        [HttpPost]
        public IActionResult Create(CropCycleViewModel model)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var plots = _context.LandPlots
                .Include(p => p.Farm)
                .Where(p => p.Farm.FarmerId == farmer.FarmerId)
                .ToList();
            ViewBag.Plots = plots;
            ViewBag.Crops = _context.Crops.ToList();

            if (!plots.Any(p => p.PlotId == model.PlotId))
            {
                ModelState.AddModelError("PlotId", "Invalid plot selection.");
            }

            // Check if there is already an active CropCycle (Status == "Active") for the selected PlotId
            if (model.Status == "Active" && _context.CropCycles.Any(c => c.PlotId == model.PlotId && c.Status == "Active"))
            {
                ModelState.AddModelError("PlotId", "This plot already has an active crop cycle. You must harvest or complete the current cycle before sowing a new one.");
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
                var cycle = new CropCycle
                {
                    PlotId = model.PlotId,
                    CropId = model.CropId,
                    SowingDate = model.SowingDate,
                    ExpectedHarvestDate = model.ExpectedHarvestDate,
                    CurrentStage = model.CurrentStage.Trim(),
                    Status = model.Status
                };

                _context.CropCycles.Add(cycle);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Crop cycle sowing recorded successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "Error sowing crop cycle: " + ex.Message;
                ViewData["UserName"] = HttpContext.Session.GetString("UserName");
                ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
                ViewData["UserRole"] = "Farmer";
                return View(model);
            }
        }

        // GET: /CropCycle/Edit/{id}
        public IActionResult Edit(int id)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var cycle = _context.CropCycles
                .Include(c => c.LandPlot)
                .FirstOrDefault(c => c.CropCycleId == id && c.LandPlot.Farm.FarmerId == farmer.FarmerId);

            if (cycle == null)
            {
                TempData["ErrorMessage"] = "Crop cycle not found.";
                return RedirectToAction("Index");
            }

            ViewBag.Plots = _context.LandPlots
                .Include(p => p.Farm)
                .Where(p => p.Farm.FarmerId == farmer.FarmerId)
                .ToList();
            ViewBag.Crops = _context.Crops.ToList();

            var model = new CropCycleViewModel
            {
                PlotId = cycle.PlotId,
                CropId = cycle.CropId,
                SowingDate = cycle.SowingDate,
                ExpectedHarvestDate = cycle.ExpectedHarvestDate,
                CurrentStage = cycle.CurrentStage ?? "Sowing",
                Status = cycle.Status ?? "Active"
            };

            ViewData["CropCycleId"] = cycle.CropCycleId;
            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Farmer";

            return View(model);
        }

        // POST: /CropCycle/Edit/{id}
        [HttpPost]
        public IActionResult Edit(int id, CropCycleViewModel model)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var cycle = _context.CropCycles
                .Include(c => c.LandPlot)
                .FirstOrDefault(c => c.CropCycleId == id && c.LandPlot.Farm.FarmerId == farmer.FarmerId);

            if (cycle == null)
            {
                TempData["ErrorMessage"] = "Crop cycle not found.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Plots = _context.LandPlots
                    .Include(p => p.Farm)
                    .Where(p => p.Farm.FarmerId == farmer.FarmerId)
                    .ToList();
                ViewBag.Crops = _context.Crops.ToList();
                ViewData["CropCycleId"] = cycle.CropCycleId;
                ViewData["UserName"] = HttpContext.Session.GetString("UserName");
                ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
                ViewData["UserRole"] = "Farmer";
                return View(model);
            }

            try
            {
                cycle.PlotId = model.PlotId;
                cycle.CropId = model.CropId;
                cycle.SowingDate = model.SowingDate;
                cycle.ExpectedHarvestDate = model.ExpectedHarvestDate;
                cycle.CurrentStage = model.CurrentStage.Trim();
                cycle.Status = model.Status;

                _context.SaveChanges();
                TempData["SuccessMessage"] = "Crop cycle stage updated successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "Error updating crop cycle: " + ex.Message;
                ViewBag.Plots = _context.LandPlots
                    .Include(p => p.Farm)
                    .Where(p => p.Farm.FarmerId == farmer.FarmerId)
                    .ToList();
                ViewBag.Crops = _context.Crops.ToList();
                ViewData["CropCycleId"] = cycle.CropCycleId;
                ViewData["UserName"] = HttpContext.Session.GetString("UserName");
                ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
                ViewData["UserRole"] = "Farmer";
                return View(model);
            }
        }

        // GET: /CropCycle/Delete/{id}
        public IActionResult Delete(int id)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var cycle = _context.CropCycles
                .Include(c => c.LandPlot)
                .Include(c => c.Crop)
                .FirstOrDefault(c => c.CropCycleId == id && c.LandPlot.Farm.FarmerId == farmer.FarmerId);

            if (cycle == null)
            {
                TempData["ErrorMessage"] = "Crop cycle not found.";
                return RedirectToAction("Index");
            }

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Farmer";

            return View(cycle);
        }

        // POST: /CropCycle/Delete/{id}
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var cycle = _context.CropCycles
                .Include(c => c.LandPlot)
                .Include(c => c.CropMonitorings)
                .Include(c => c.Harvests)
                .Include(c => c.PestCases)
                .FirstOrDefault(c => c.CropCycleId == id && c.LandPlot.Farm.FarmerId == farmer.FarmerId);

            if (cycle == null)
            {
                TempData["ErrorMessage"] = "Crop cycle not found.";
                return RedirectToAction("Index");
            }

            try
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

                // Delete other related records
                _context.CropMonitorings.RemoveRange(cycle.CropMonitorings);
                _context.PestCases.RemoveRange(cycle.PestCases);

                // Finally delete the crop cycle
                _context.CropCycles.Remove(cycle);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Crop cycle removed successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error removing crop cycle: " + (ex.InnerException?.Message ?? ex.Message);
            }

            return RedirectToAction("Index");
        }
    }
}
