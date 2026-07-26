using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Smart_Farm_and_Crop_Yeild_Management_System.Models;
using SmartFarmMVC.Models.ViewModels;

namespace SmartFarmMVC.Controllers
{
    public class FarmController : Controller
    {
        private readonly SmartFarmDbContext _context;

        public FarmController(SmartFarmDbContext context)
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

        // GET: /Farm
        // List all farms for the logged-in Farmer
        public IActionResult Index()
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var farms = _context.Farms
                .Where(f => f.FarmerId == farmer.FarmerId)
                .ToList();

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Farmer";

            return View(farms);
        }

        // GET: /Farm/Details/{id}
        // Show Farm details and list of land plots
        public IActionResult Details(int id)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var farm = _context.Farms
                .Include(f => f.LandPlots)
                .FirstOrDefault(f => f.FarmId == id && f.FarmerId == farmer.FarmerId);

            if (farm == null)
            {
                TempData["ErrorMessage"] = "Farm not found or access denied.";
                return RedirectToAction("Index");
            }

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Farmer";

            return View(farm);
        }

        // GET: /Farm/Create
        public IActionResult Create()
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Farmer";

            return View(new FarmViewModel());
        }

        // POST: /Farm/Create
        [HttpPost]
        public IActionResult Create(FarmViewModel model)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                ViewData["UserName"] = HttpContext.Session.GetString("UserName");
                ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
                ViewData["UserRole"] = "Farmer";
                return View(model);
            }

            try
            {
                var farm = new Farm
                {
                    FarmerId = farmer.FarmerId,
                    FarmName = model.FarmName.Trim(),
                    Village = model.Village.Trim(),
                    Taluka = model.Taluka.Trim(),
                    District = model.District.Trim(),
                    State = model.State.Trim(),
                    Pincode = model.Pincode.Trim(),
                    CreatedDate = DateTime.Now
                };

                _context.Farms.Add(farm);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Farm created successfully. Please add land plots to your farm.";

                // Redirect directly to adding land plots for this farm
                return RedirectToAction("Create", "LandPlot", new { farmId = farm.FarmId });
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "Error creating farm: " + ex.Message;
                ViewData["UserName"] = HttpContext.Session.GetString("UserName");
                ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
                ViewData["UserRole"] = "Farmer";
                return View(model);
            }
        }

        // GET: /Farm/Edit/{id}
        public IActionResult Edit(int id)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var farm = _context.Farms.FirstOrDefault(f => f.FarmId == id && f.FarmerId == farmer.FarmerId);
            if (farm == null)
            {
                TempData["ErrorMessage"] = "Farm not found.";
                return RedirectToAction("Index");
            }

            var model = new FarmViewModel
            {
                FarmName = farm.FarmName,
                Village = farm.Village ?? "",
                Taluka = farm.Taluka ?? "",
                District = farm.District ?? "",
                State = farm.State ?? "",
                Pincode = farm.Pincode ?? ""
            };

            ViewData["FarmId"] = farm.FarmId;
            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Farmer";

            return View(model);
        }

        // POST: /Farm/Edit/{id}
        [HttpPost]
        public IActionResult Edit(int id, FarmViewModel model)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var farm = _context.Farms.FirstOrDefault(f => f.FarmId == id && f.FarmerId == farmer.FarmerId);
            if (farm == null)
            {
                TempData["ErrorMessage"] = "Farm not found.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                ViewData["FarmId"] = farm.FarmId;
                ViewData["UserName"] = HttpContext.Session.GetString("UserName");
                ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
                ViewData["UserRole"] = "Farmer";
                return View(model);
            }

            try
            {
                farm.FarmName = model.FarmName.Trim();
                farm.Village = model.Village.Trim();
                farm.Taluka = model.Taluka.Trim();
                farm.District = model.District.Trim();
                farm.State = model.State.Trim();
                farm.Pincode = model.Pincode.Trim();

                _context.SaveChanges();
                TempData["SuccessMessage"] = "Farm updated successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "Error updating farm: " + ex.Message;
                ViewData["FarmId"] = farm.FarmId;
                ViewData["UserName"] = HttpContext.Session.GetString("UserName");
                ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
                ViewData["UserRole"] = "Farmer";
                return View(model);
            }
        }

        // GET: /Farm/Delete/{id}
        public IActionResult Delete(int id)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var farm = _context.Farms.FirstOrDefault(f => f.FarmId == id && f.FarmerId == farmer.FarmerId);
            if (farm == null)
            {
                TempData["ErrorMessage"] = "Farm not found.";
                return RedirectToAction("Index");
            }

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Farmer";

            return View(farm);
        }

        // POST: /Farm/Delete/{id}
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var farm = _context.Farms
                .Include(f => f.LandPlots)
                    .ThenInclude(p => p.CropCycles)
                        .ThenInclude(c => c.CropMonitorings)
                .Include(f => f.LandPlots)
                    .ThenInclude(p => p.CropCycles)
                        .ThenInclude(c => c.Harvests)
                .Include(f => f.LandPlots)
                    .ThenInclude(p => p.CropCycles)
                        .ThenInclude(c => c.PestCases)
                .FirstOrDefault(f => f.FarmId == id && f.FarmerId == farmer.FarmerId);

            if (farm == null)
            {
                TempData["ErrorMessage"] = "Farm not found.";
                return RedirectToAction("Index");
            }

            try
            {
                // Delete all related data in the correct order
                foreach (var plot in farm.LandPlots.ToList())
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

                        // Now delete the harvests
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
                }

                // Finally delete the farm
                _context.Farms.Remove(farm);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Farm and all related data deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting farm: " + (ex.InnerException?.Message ?? ex.Message);
            }

            return RedirectToAction("Index");
        }
    }
}
