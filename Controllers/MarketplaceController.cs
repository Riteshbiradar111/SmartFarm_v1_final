using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Smart_Farm_and_Crop_Yeild_Management_System.Models;
using SmartFarmMVC.Models.ViewModels;

namespace SmartFarmMVC.Controllers
{
    // This controller manages B2B Produce Sales for Farmers, including marketplace listings,
    // order acceptance, and shipping simulation updates.
    // Written with clear comments and linear logic for interview walkthroughs.
    public class MarketplaceController : Controller
    {
        private readonly SmartFarmDbContext _context;

        public MarketplaceController(SmartFarmDbContext context)
        {
            _context = context;
        }

        // Helper check for logged-in sessions
        private string? GetSessionRole() => HttpContext.Session.GetString("UserRole");
        private string? GetSessionUsername() => HttpContext.Session.GetString("UserUsername");

        // GET: /Marketplace
        // Displays list of listings and buyer orders (for Farmers)
        public IActionResult Index(string? searchCrop, string? searchRegion, decimal? maxPrice)
        {
            var role = GetSessionRole();
            var username = GetSessionUsername();
            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(username)) return RedirectToAction("Login", "Auth");

            var farmer = _context.Farmers.FirstOrDefault(f => f.User.Username == username);
            if (farmer == null && role == "Farmer") return RedirectToAction("Login", "Auth");

            // 1. Fetch Listings
            IQueryable<CropListing> query = _context.CropListings
                .Include(l => l.Harvest)
                    .ThenInclude(h => h.CropCycle)
                        .ThenInclude(c => c.Crop)
                .Include(l => l.Harvest)
                    .ThenInclude(h => h.CropCycle)
                        .ThenInclude(c => c.LandPlot)
                            .ThenInclude(p => p.Farm)
                                .ThenInclude(f => f.Farmer)
                .Include(l => l.Buyer);

            if (role == "Farmer")
            {
                query = query.Where(l => l.Harvest.CropCycle.LandPlot.Farm.FarmerId == farmer!.FarmerId);
            }
            else if (role == "Buyer")
            {
                // Buyers see only available listings
                query = query.Where(l => l.Status == "Available");
            }

            // Apply Filters
            if (!string.IsNullOrEmpty(searchCrop))
            {
                query = query.Where(l => l.Harvest.CropCycle.Crop.CropName.Contains(searchCrop));
            }
            if (!string.IsNullOrEmpty(searchRegion))
            {
                query = query.Where(l => l.Harvest.CropCycle.LandPlot.Farm.Village.Contains(searchRegion) ||
                                         l.Harvest.CropCycle.LandPlot.Farm.District.Contains(searchRegion));
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(l => l.PricePerUnit <= maxPrice.Value);
            }

            var listings = query.OrderByDescending(l => l.ListedDate).ToList();

            // 2. Fetch Buyer Orders (if Farmer)
            if (role == "Farmer" && farmer != null)
            {
                var orders = _context.CropOrders
                    .Include(o => o.CropListing)
                        .ThenInclude(l => l.Harvest)
                            .ThenInclude(h => h.CropCycle)
                                .ThenInclude(c => c.Crop)
                    .Include(o => o.Harvest)
                        .ThenInclude(h => h.CropCycle)
                            .ThenInclude(c => c.Crop)
                    .Include(o => o.Buyer)
                    .Where(o => o.FarmerId == farmer.FarmerId)
                    .OrderByDescending(o => o.OrderDate)
                    .ToList();

                ViewBag.BuyerOrders = orders;
            }

            ViewBag.SearchCrop = searchCrop;
            ViewBag.SearchRegion = searchRegion;
            ViewBag.MaxPrice = maxPrice;

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = role;

            return View(listings);
        }

        // GET: /Marketplace/Create
        // Farmer lists a harvested crop yield. Can pre-select harvestId from query params.
        public IActionResult Create(int? harvestId)
        {
            var farmer = _context.Farmers.FirstOrDefault(f => f.User.Username == GetSessionUsername());
            if (farmer == null) return RedirectToAction("Login", "Auth");

            // Load all harvests for this farmer that have actual stock remaining
            var harvests = _context.Harvests
                .Include(h => h.CropCycle)
                    .ThenInclude(c => c.Crop)
                .Include(h => h.CropCycle)
                    .ThenInclude(c => c.LandPlot)
                .Where(h => h.CropCycle.LandPlot.Farm.FarmerId == farmer.FarmerId && !h.CropListings.Any())
                .ToList();

            if (harvests.Count == 0)
            {
                TempData["ErrorMessage"] = "You have no warehouse stock to list. Please log a harvest first.";
                return RedirectToAction("Index", "Harvest");
            }

            ViewBag.Harvests = harvests;

            var model = new MarketplaceViewModel();
            if (harvestId.HasValue)
            {
                model.HarvestId = harvestId.Value;
                var selected = harvests.FirstOrDefault(h => h.HarvestId == harvestId.Value);
                if (selected != null)
                {
                    model.AvailableQuantity = selected.ActualQuantity;
                    model.Unit = selected.Unit;
                }
            }

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Farmer";

            return View(model);
        }

        // POST: /Marketplace/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MarketplaceViewModel model, IFormFile? cropImage)
        {
            var farmer = _context.Farmers.FirstOrDefault(f => f.User.Username == GetSessionUsername());
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var harvests = _context.Harvests
                .Include(h => h.CropCycle)
                    .ThenInclude(c => c.Crop)
                .Include(h => h.CropCycle)
                    .ThenInclude(c => c.LandPlot)
                .Where(h => h.CropCycle.LandPlot.Farm.FarmerId == farmer.FarmerId)
                .ToList();

            ViewBag.Harvests = harvests.Where(h => h.ActualQuantity > 0 || h.HarvestId == model.HarvestId).ToList();

            var selectedHarvest = harvests.FirstOrDefault(h => h.HarvestId == model.HarvestId);
            if (selectedHarvest == null)
            {
                ModelState.AddModelError("HarvestId", "Invalid harvest record selected.");
            }
            else
            {
                // Check how much of this harvest is already listed
                var alreadyListed = _context.CropListings
                    .Where(l => l.HarvestId == model.HarvestId && l.Status != "Sold")
                    .Sum(l => (decimal?)l.AvailableQuantity) ?? 0;

                var remainingToList = selectedHarvest.ActualQuantity - alreadyListed;

                if (model.AvailableQuantity > remainingToList)
                {
                    ModelState.AddModelError("AvailableQuantity", $"You can only list {remainingToList} {selectedHarvest.Unit} more. Already listed: {alreadyListed} {selectedHarvest.Unit}.");
                }
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
                // Handle image upload
                string? imagePath = null;
                if (cropImage != null && cropImage.Length > 0)
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(cropImage.FileName).ToLowerInvariant();

                    if (allowedExtensions.Contains(extension))
                    {
                        // Create uploads directory if it doesn't exist
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "crops");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Generate unique filename
                        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // Save file
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            cropImage.CopyTo(fileStream);
                        }

                        // Store relative path for database
                        imagePath = $"/uploads/crops/{uniqueFileName}";
                    }
                }

                // 1. Create crop listing record
                var listing = new CropListing
                {
                    HarvestId = model.HarvestId,
                    PricePerUnit = model.PricePerUnit,
                    AvailableQuantity = model.AvailableQuantity,
                    Unit = model.Unit,
                    Status = "Available",
                    ListedDate = DateTime.Now,
                    ImagePath = imagePath
                };

                // 2. Mark harvest as listed (but don't reduce ActualQuantity)
                // ActualQuantity represents the total harvested amount (for reporting)
                // AvailableQuantity in CropListing tracks remaining stock for sale
                if (selectedHarvest.ActualQuantity == model.AvailableQuantity)
                {
                    selectedHarvest.Status = "Listed";
                }

                _context.CropListings.Add(listing);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Produce listed on the marketplace successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "Error listing harvest: " + ex.Message;
                ViewData["UserName"] = HttpContext.Session.GetString("UserName");
                ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
                ViewData["UserRole"] = "Farmer";
                return View(model);
            }
        }

        // POST: /Marketplace/AcceptOrder
        // Farmer accepts an incoming buyer request
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AcceptOrder(int orderId)
        {
            var farmer = _context.Farmers.FirstOrDefault(f => f.User.Username == GetSessionUsername());
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var order = _context.CropOrders.FirstOrDefault(o => o.OrderId == orderId && o.FarmerId == farmer.FarmerId);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("Index");
            }

            if (order.Status == "Delivered")
            {
                TempData["ErrorMessage"] = "Cannot modify a delivered order.";
                return RedirectToAction("Index");
            }

            if (order.HarvestId.HasValue)
            {
                var isListed = _context.CropListings.Any(l => l.HarvestId == order.HarvestId.Value && l.Status == "Available");
                if (!isListed)
                {
                    TempData["ErrorMessage"] = "You must list this crop on the Marketplace before you can accept the pre-order request.";
                    return RedirectToAction("Index");
                }
            }

            order.Status = "Farmer Accepted";
            order.AcceptedDate = DateTime.Now;
            _context.SaveChanges();

            // Create notification for the buyer
            var buyer = _context.Buyers.Find(order.BuyerId);
            if (buyer != null)
            {
                var notif = new Notification
                {
                    UserId = buyer.UserId,
                    Title = "Order Accepted",
                    Message = $"Farmer {farmer.FullName} accepted your order request #{order.OrderId}.",
                    IsRead = false,
                    CreatedDate = DateTime.Now
                };
                _context.Notifications.Add(notif);
                _context.SaveChanges();
            }

            TempData["SuccessMessage"] = "Order request accepted successfully.";
            return RedirectToAction("Index");
        }

        // POST: /Marketplace/UpdateOrderStatus
        // Farmer updates shipping/delivery timeline stages
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderStatus(int orderId, string status)
        {
            var farmer = _context.Farmers.FirstOrDefault(f => f.User.Username == GetSessionUsername());
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var order = _context.CropOrders.FirstOrDefault(o => o.OrderId == orderId && o.FarmerId == farmer.FarmerId);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("Index");
            }

            if (order.Status == "Delivered")
            {
                TempData["ErrorMessage"] = "Cannot modify a delivered order.";
                return RedirectToAction("Index");
            }

            order.Status = status;
            if (status == "Delivered")
            {
                order.DeliveryDate = DateTime.Now;
            }
            _context.SaveChanges();

            // Notify buyer
            var buyer = _context.Buyers.Find(order.BuyerId);
            if (buyer != null)
            {
                var notif = new Notification
                {
                    UserId = buyer.UserId,
                    Title = "Order Status Updated",
                    Message = $"Your order #{order.OrderId} is now: {status}.",
                    IsRead = false,
                    CreatedDate = DateTime.Now
                };
                _context.Notifications.Add(notif);
                _context.SaveChanges();
            }

            TempData["SuccessMessage"] = $"Shipment status updated to: {status}";
            return RedirectToAction("Index");
        }

        // POST: /Marketplace/Purchase/{id}
        // Legacy fallback method for direct purchase
        [HttpPost]
        public IActionResult Purchase(int id)
        {
            var role = GetSessionRole();
            var username = GetSessionUsername();
            if (role != "Buyer" || string.IsNullOrEmpty(username)) return Unauthorized();

            var buyer = _context.Buyers.FirstOrDefault(b => b.User.Username == username);
            if (buyer == null) return RedirectToAction("Login", "Auth");

            var listing = _context.CropListings
                .Include(l => l.Harvest)
                .FirstOrDefault(l => l.ListingId == id);

            if (listing == null || listing.Status != "Available")
            {
                TempData["ErrorMessage"] = "This listing is no longer available.";
                return RedirectToAction("Index");
            }

            try
            {
                listing.BuyerId = buyer.BuyerId;
                listing.PurchasedQuantity = listing.AvailableQuantity;
                listing.PurchaseDate = DateTime.Now;
                listing.Status = "Sold";

                _context.SaveChanges();
                TempData["SuccessMessage"] = "Purchase completed successfully! The seller has been notified.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error completing purchase: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // POST: /Marketplace/Delete/{id}
        // Deletes a crop listing (only if no buyer has purchased it)
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var role = GetSessionRole();
            var username = GetSessionUsername();
            if (role != "Farmer" || string.IsNullOrEmpty(username)) return Unauthorized();

            var farmer = _context.Farmers.FirstOrDefault(f => f.User.Username == username);
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var listing = _context.CropListings
                .Include(l => l.Harvest)
                    .ThenInclude(h => h.CropCycle)
                        .ThenInclude(c => c.LandPlot)
                            .ThenInclude(p => p.Farm)
                .FirstOrDefault(l => l.ListingId == id);

            if (listing == null)
            {
                TempData["ErrorMessage"] = "Listing not found.";
                return RedirectToAction("Index");
            }

            // Verify ownership
            if (listing.Harvest.CropCycle.LandPlot.Farm.FarmerId != farmer.FarmerId)
            {
                TempData["ErrorMessage"] = "You don't have permission to delete this listing.";
                return RedirectToAction("Index");
            }

            // Check if listing has a buyer
            if (listing.BuyerId != null || listing.Status != "Available")
            {
                TempData["ErrorMessage"] = "Cannot delete a listing that has already been purchased.";
                return RedirectToAction("Index");
            }

            try
            {
                _context.CropListings.Remove(listing);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Listing deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting listing: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
