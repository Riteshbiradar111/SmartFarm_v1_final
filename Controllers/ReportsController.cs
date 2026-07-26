using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Smart_Farm_and_Crop_Yeild_Management_System.Models;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Controllers
{
    public class ReportsController : Controller
    {
        private readonly SmartFarmDbContext _context;

        public ReportsController(SmartFarmDbContext context)
        {
            _context = context;
        }

        // Helper to validate Farmer Session
        private Smart_Farm_and_Crop_Yeild_Management_System.Models.Farmer? GetActiveFarmer()
        {
            var role = HttpContext.Session.GetString("UserRole");
            var username = HttpContext.Session.GetString("UserUsername");

            if (role != "Farmer" || string.IsNullOrEmpty(username))
            {
                return null;
            }

            return _context.Farmers
                .Include(f => f.User)
                .FirstOrDefault(f => f.User.Username == username);
        }

        // GET: /Reports
        // Dashboard with farm statistics and analytics
        public IActionResult Index()
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            // Get all farms for the farmer (count only)
            var totalFarms = _context.Farms
                .Where(f => f.FarmerId == farmer.FarmerId)
                .Count();

            var farmIds = _context.Farms
                .Where(f => f.FarmerId == farmer.FarmerId)
                .Select(f => f.FarmId)
                .ToList();

            // Get all land plots (count only)
            var totalPlots = _context.LandPlots
                .Where(lp => farmIds.Contains(lp.FarmId))
                .Count();

            var plotIds = _context.LandPlots
                .Where(lp => farmIds.Contains(lp.FarmId))
                .Select(lp => lp.PlotId)
                .ToList();

            // Get crop cycles (minimal data)
            var activeCropCycles = _context.CropCycles
                .Where(cc => plotIds.Contains(cc.PlotId) && cc.Status == "Active")
                .Count();

            var completedCropCycles = _context.CropCycles
                .Where(cc => plotIds.Contains(cc.PlotId) && cc.Status == "Completed")
                .Count();

            // Get harvests (only recent ones with includes)
            var recentHarvests = _context.Harvests
                .Include(h => h.CropCycle)
                    .ThenInclude(cc => cc.Crop)
                .Include(h => h.CropCycle)
                    .ThenInclude(cc => cc.LandPlot)
                .Where(h => plotIds.Contains(h.CropCycle.PlotId))
                .OrderByDescending(h => h.HarvestDate)
                .Take(5)
                .AsNoTracking()
                .ToList();

            // Get harvest statistics (without loading all data)
            var totalHarvests = _context.Harvests
                .Where(h => plotIds.Contains(h.CropCycle.PlotId))
                .Count();

            var harvestStats = _context.Harvests
                .Where(h => plotIds.Contains(h.CropCycle.PlotId))
                .GroupBy(h => 1)
                .Select(g => new
                {
                    TotalActual = g.Sum(h => h.ActualQuantity),
                    TotalExpected = g.Sum(h => h.ExpectedQuantity)
                })
                .FirstOrDefault();

            var totalHarvestQuantity = harvestStats?.TotalActual ?? 0;
            var totalExpectedQuantity = harvestStats?.TotalExpected ?? 0;

            // Get pest cases (count only)
            var totalPestCases = _context.PestCases
                .Where(pc => plotIds.Contains(pc.CropCycle.PlotId))
                .Count();

            var activePestCases = _context.PestCases
                .Where(pc => plotIds.Contains(pc.CropCycle.PlotId) && 
                            (pc.Status == "Active" || pc.Status == "Under Treatment"))
                .Count();

            // Get crop listings statistics
            var harvestIds = _context.Harvests
                .Where(h => plotIds.Contains(h.CropCycle.PlotId))
                .Select(h => h.HarvestId)
                .ToList();

            var totalListedProduce = _context.CropListings
                .Where(cl => harvestIds.Contains(cl.HarvestId))
                .Count();

            var soldProduce = _context.CropListings
                .Where(cl => harvestIds.Contains(cl.HarvestId) && cl.Status == "Sold")
                .Count();

            // Calculate statistics
            var averageYieldVariance = totalExpectedQuantity > 0 
                ? ((totalHarvestQuantity - totalExpectedQuantity) / totalExpectedQuantity) * 100 
                : 0;

            // Calculate revenue from buyer orders & listings
            decimal totalRevenue = 0;
            decimal pendingRevenue = 0;

            try
            {
                var farmerOrders = _context.CropOrders
                    .Where(o => o.FarmerId == farmer.FarmerId)
                    .ToList();

                var paidStatuses = new[] { "Paid", "Completed", "Delivered", "Sold" };
                var paidOrderTotal = farmerOrders.Where(o => paidStatuses.Contains(o.Status)).Sum(o => o.TotalAmount);

                var listingPaidTotal = _context.CropListings
                    .Where(cl => harvestIds.Contains(cl.HarvestId) && (cl.Status == "Sold" || (cl.PurchasedQuantity != null && cl.PurchasedQuantity > 0)))
                    .Sum(cl => cl.PricePerUnit * (cl.PurchasedQuantity ?? cl.AvailableQuantity));

                totalRevenue = Math.Max(paidOrderTotal, listingPaidTotal);

                pendingRevenue = farmerOrders
                    .Where(o => !paidStatuses.Contains(o.Status) && o.Status != "Cancelled")
                    .Sum(o => o.TotalAmount);
            }
            catch
            {
                totalRevenue = 0;
                pendingRevenue = 0;
            }

            // Top performing crops (limited query)
            var topCrops = _context.Harvests
                .Where(h => plotIds.Contains(h.CropCycle.PlotId))
                .Include(h => h.CropCycle)
                    .ThenInclude(cc => cc.Crop)
                .AsNoTracking()
                .ToList()
                .GroupBy(h => h.CropCycle.Crop.CropName)
                .Select(g => new
                {
                    CropName = g.Key ?? "Unknown",
                    TotalYield = g.Sum(h => h.ActualQuantity),
                    HarvestCount = g.Count(),
                    AverageYield = g.Average(h => h.ActualQuantity)
                })
                .OrderByDescending(c => c.TotalYield)
                .Take(5)
                .ToList();

            // Pass data to view
            ViewBag.TotalFarms = totalFarms;
            ViewBag.TotalPlots = totalPlots;
            ViewBag.ActiveCropCycles = activeCropCycles;
            ViewBag.CompletedCropCycles = completedCropCycles;
            ViewBag.TotalHarvests = totalHarvests;
            ViewBag.TotalHarvestQuantity = totalHarvestQuantity;
            ViewBag.TotalExpectedQuantity = totalExpectedQuantity;
            ViewBag.AverageYieldVariance = averageYieldVariance;
            ViewBag.TotalPestCases = totalPestCases;
            ViewBag.ActivePestCases = activePestCases;
            ViewBag.TotalListedProduce = totalListedProduce;
            ViewBag.SoldProduce = soldProduce;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.PendingRevenue = pendingRevenue;
            ViewBag.TopCrops = topCrops;
            ViewBag.RecentHarvests = recentHarvests;

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Farmer";
            ViewData["Title"] = "Farm Reports & Analytics";

            return View();
        }
    }
}
