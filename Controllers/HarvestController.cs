using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Smart_Farm_and_Crop_Yeild_Management_System.Models;
using SmartFarmMVC.Models.ViewModels;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Controllers
{
    public class HarvestController : Controller
    {
        private readonly SmartFarmDbContext _context;

        public HarvestController(SmartFarmDbContext context)
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

        // GET: /Harvest
        // List harvests for the farmer
        public IActionResult Index()
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            try
            {
                // Get farmer's farm IDs first
                var farmIds = _context.Farms
                    .Where(f => f.FarmerId == farmer.FarmerId)
                    .Select(f => f.FarmId)
                    .ToList();

                if (!farmIds.Any())
                {
                    ViewData["UserName"] = HttpContext.Session.GetString("UserName");
                    ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
                    ViewData["UserRole"] = "Farmer";
                    return View(new List<Harvest>());
                }

                // Get plot IDs
                var plotIds = _context.LandPlots
                    .Where(lp => farmIds.Contains(lp.FarmId))
                    .Select(lp => lp.PlotId)
                    .ToList();

                if (!plotIds.Any())
                {
                    ViewData["UserName"] = HttpContext.Session.GetString("UserName");
                    ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
                    ViewData["UserRole"] = "Farmer";
                    return View(new List<Harvest>());
                }

                // Get harvests with minimal includes
                var harvests = _context.Harvests
                    .Include(h => h.CropCycle)
                        .ThenInclude(c => c.Crop)
                    .Include(h => h.CropCycle.LandPlot)
                        .ThenInclude(lp => lp.Farm)
                    .Include(h => h.CropListings)
                    .Where(h => plotIds.Contains(h.CropCycle.PlotId))
                    .OrderByDescending(h => h.HarvestDate)
                    .AsNoTracking()
                    .ToList();

                ViewData["UserName"] = HttpContext.Session.GetString("UserName");
                ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
                ViewData["UserRole"] = "Farmer";

                return View(harvests);
            }
            catch (Exception ex)
            {
                ViewData["UserName"] = HttpContext.Session.GetString("UserName");
                ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
                ViewData["UserRole"] = "Farmer";
                ViewData["ErrorMessage"] = "Error loading harvests: " + ex.Message;
                return View(new List<Harvest>());
            }
        }

        // GET: /Harvest/Create
        public IActionResult Create(int? cycleId)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var cycles = _context.CropCycles
                .Include(c => c.Crop)
                .Include(c => c.LandPlot)
                    .ThenInclude(lp => lp.Farm)
                .Where(c => c.LandPlot.Farm.FarmerId == farmer.FarmerId && c.Status == "Active")
                .ToList();

            if (cycles.Count == 0)
            {
                TempData["ErrorMessage"] = "You must have an active crop cycle to record harvests.";
                return RedirectToAction("Index", "CropCycle");
            }

            ViewBag.Cycles = cycles;

            var model = new HarvestViewModel();
            if (cycleId.HasValue)
            {
                model.CropCycleId = cycleId.Value;
            }

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Farmer";

            return View(model);
        }

        // POST: /Harvest/Create
        [HttpPost]
        public IActionResult Create(HarvestViewModel model)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var cycles = _context.CropCycles
                .Include(c => c.Crop)
                .Include(c => c.LandPlot)
                    .ThenInclude(lp => lp.Farm)
                .Where(c => c.LandPlot.Farm.FarmerId == farmer.FarmerId && c.Status == "Active")
                .ToList();

            ViewBag.Cycles = cycles;

            if (!cycles.Any(c => c.CropCycleId == model.CropCycleId))
            {
                ModelState.AddModelError("CropCycleId", "Invalid crop cycle selection.");
            }

            var selectedCycle = cycles.FirstOrDefault(c => c.CropCycleId == model.CropCycleId);
            if (selectedCycle != null && model.HarvestDate < selectedCycle.SowingDate)
            {
                ModelState.AddModelError("HarvestDate", "The harvest date must be on or after the sowing date (" + selectedCycle.SowingDate.ToString("dd-MM-yyyy") + ").");
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
                var harvest = new Harvest
                {
                    CropCycleId = model.CropCycleId,
                    HarvestDate = model.HarvestDate,
                    ExpectedQuantity = model.ExpectedQuantity,
                    ActualQuantity = model.ActualQuantity,
                    Unit = model.Unit,
                    Status = model.Status
                };

                _context.Harvests.Add(harvest);

                // Update crop cycle status to Completed
                var cycle = _context.CropCycles.Find(model.CropCycleId);
                if (cycle != null)
                {
                    cycle.Status = "Completed";
                    cycle.CurrentStage = "Harvested";
                }

                _context.SaveChanges();

                TempData["SuccessMessage"] = "Harvest yield logged successfully. You can now list this harvest on the Marketplace.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "Error logging harvest: " + ex.Message;
                ViewData["UserName"] = HttpContext.Session.GetString("UserName");
                ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
                ViewData["UserRole"] = "Farmer";
                return View(model);
            }
        }

        // POST: /Harvest/Delete/{id}
        // Deletes a harvest record (only if not listed on marketplace)
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var harvest = _context.Harvests
                .Include(h => h.CropCycle)
                    .ThenInclude(c => c.LandPlot)
                        .ThenInclude(p => p.Farm)
                .Include(h => h.CropListings)
                .FirstOrDefault(h => h.HarvestId == id);

            if (harvest == null)
            {
                TempData["ErrorMessage"] = "Harvest record not found.";
                return RedirectToAction("Index");
            }

            // Verify ownership
            if (harvest.CropCycle.LandPlot.Farm.FarmerId != farmer.FarmerId)
            {
                TempData["ErrorMessage"] = "You don't have permission to delete this harvest record.";
                return RedirectToAction("Index");
            }

            // Check if harvest has been listed on marketplace
            if (harvest.CropListings.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete a harvest that has been listed on the marketplace. Delete the listing first.";
                return RedirectToAction("Index");
            }

            try
            {
                _context.Harvests.Remove(harvest);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Harvest record deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting harvest: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
